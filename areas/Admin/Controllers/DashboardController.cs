using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models.Enums;
using RestaurantQRSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // SIFIRDAN veri yükleme yaklaşımı kullanalım
                // Önce tüm masaları yükleyelim (siparişler olmadan)
                var tables = await _context.Tables.ToListAsync();

                // Her masa için ayrı ayrı aktif siparişleri yükleyelim
                var tableViewModels = new List<TableDetailsViewModel>();

                foreach (var table in tables)
                {
                    // Her masa için aktif siparişleri direkt olarak sorgulayalım
                    var activeOrders = await _context.Orders
                        .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.Product)
                        .Where(o => o.TableId == table.Id &&
                                  (o.Status == OrderStatus.Received ||
                                   o.Status == OrderStatus.Preparing ||
                                   o.Status == OrderStatus.Ready ||
                                   o.Status == OrderStatus.Delivered))
                        .ToListAsync();

                    // Debug bilgisi
                    System.Diagnostics.Debug.WriteLine($"Masa {table.Id} için {activeOrders.Count} aktif sipariş bulundu");

                    // Her masa için ViewModel oluşturalım
                    var tableViewModel = new TableDetailsViewModel
                    {
                        Id = table.Id,
                        Name = table.Name,
                        Status = table.Status,
                        IsOccupied = table.IsOccupied,
                        OccupiedSince = table.OccupiedSince,
                        // Aktif siparişleri doğrudan ekleyelim
                        Orders = activeOrders.Select(o => new OrderViewModel
                        {
                            Id = o.Id,
                            TableId = o.TableId,
                            CustomerName = o.CustomerName,
                            CustomerNote = o.CustomerNote,
                            Status = o.Status,
                            OrderDate = o.OrderDate,
                            TotalAmount = (int)o.TotalAmount,
                            OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                            {
                                ProductId = oi.ProductId,
                                ProductName = oi.Product?.Name ?? "Ürün Bulunamadı",
                                Quantity = (int)oi.Quantity,
                                UnitPrice = oi.UnitPrice,
                                TotalPrice = oi.Quantity * oi.UnitPrice
                            }).ToList()
                        }).ToList()
                    };

                    // Toplam tutarı hesaplayalım
                    tableViewModel.TotalAmount = tableViewModel.Orders.Sum(o => o.TotalAmount);

                    // ViewModel'i listeye ekleyelim
                    tableViewModels.Add(tableViewModel);
                }

                // Diğer istatistikleri yükleyelim
                var model = new AdminDashboardViewModel
                {
                    TotalCategories = await _context.Categories.CountAsync(),
                    TotalProducts = await _context.Products.CountAsync(),
                    TotalTables = tables.Count,
                    NewOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Received),
                    ProcessingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Preparing),
                    CompletedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid),
                    TodayOrders = await _context.Orders.CountAsync(o => o.OrderDate.Date == DateTime.Today),
                    TodayRevenue = (double)await _context.Orders
                        .Where(o => o.OrderDate.Date == DateTime.Today && (o.Status == OrderStatus.Completed || o.Status == OrderStatus.Paid))
                        .Select(o => o.TotalAmount)
                        .SumAsync(a => (double)a),

                    // Hazır TableViewModel listesini kullan
                    TableList = tableViewModels
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Hata logla
                System.Diagnostics.Debug.WriteLine($"Dashboard Index Hatası: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        // Tüm masaların durumlarını sipariş verilerine göre güncelle
        private async Task UpdateTableStatuses()
        {
            var tables = await _context.Tables.ToListAsync();

            foreach (var table in tables)
            {
                // Bu masa için aktif sipariş var mı kontrol et
                bool hasActiveOrder = await _context.Orders
                    .AnyAsync(o => o.TableId == table.Id &&
                              (o.Status == OrderStatus.Received ||
                               o.Status == OrderStatus.Preparing ||
                               o.Status == OrderStatus.Ready ||
                               o.Status == OrderStatus.Delivered));

                // Masa durumu yanlışsa güncelle
                if (table.IsOccupied != hasActiveOrder)
                {
                    table.IsOccupied = hasActiveOrder;

                    if (!hasActiveOrder)
                    {
                        table.OccupiedSince = null;
                    }
                    else if (table.OccupiedSince == null)
                    {
                        // Eğer aktif sipariş var ama OccupiedSince null ise sipariş zamanını ata
                        var latestOrder = await _context.Orders
                            .Where(o => o.TableId == table.Id &&
                                   (o.Status == OrderStatus.Received ||
                                    o.Status == OrderStatus.Preparing ||
                                    o.Status == OrderStatus.Ready ||
                                    o.Status == OrderStatus.Delivered))
                            .OrderByDescending(o => o.OrderDate)
                            .FirstOrDefaultAsync();

                        if (latestOrder != null)
                        {
                            table.OccupiedSince = latestOrder.OrderDate;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        // Dashboard istatistiklerini hazırla
        private async Task<AdminDashboardViewModel> GetAdminStats()
        {
            var today = DateTime.Today;

            // Bugünün TÜM siparişlerini getir (iptal edilenler hariç)
            var todayOrders = await _context.Orders
                .Where(o => o.OrderDate.Date == today && o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            // Duruma göre siparişleri ayır
            var newOrders = await _context.Orders
                .CountAsync(o => (o.Status == OrderStatus.Received) &&
                                 o.OrderDate.Date == today);

            var processingOrders = await _context.Orders
                .CountAsync(o => (o.Status == OrderStatus.Preparing) &&
                                 o.OrderDate.Date == today);

            // TAMAMLANAN ve ÖDENMİŞ siparişleri dahil et!
            var completedOrders = await _context.Orders
                .CountAsync(o => (o.Status == OrderStatus.Ready ||
                                 o.Status == OrderStatus.Delivered ||
                                 o.Status == OrderStatus.Paid) &&
                                 o.OrderDate.Date == today);

            // Bugünkü ciro - TÜM siparişler (iptal edilenler hariç)
            var todayRevenue = todayOrders.Sum(o => o.TotalAmount);

            // Debug için istatistikleri yazdır
            System.Diagnostics.Debug.WriteLine($"Bugün Yeni: {newOrders}");
            System.Diagnostics.Debug.WriteLine($"Bugün Hazırlanıyor: {processingOrders}");
            System.Diagnostics.Debug.WriteLine($"Bugün Tamamlanan/Ödenen: {completedOrders}");
            System.Diagnostics.Debug.WriteLine($"Bugün Gelir: {todayRevenue}");

            return new AdminDashboardViewModel
            {
                TotalCategories = await _context.Categories.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalTables = await _context.Tables.CountAsync(),
                NewOrders = newOrders,
                ProcessingOrders = processingOrders,
                CompletedOrders = completedOrders,  // Ödenen siparişleri de içerir
                TodayOrders = todayOrders.Count,
                TodayRevenue = (double)todayRevenue
            };
        }

        // TableDetailsViewModel listesi oluştur (masalar için)
        public async Task<IActionResult> TableDetails(int id)
        {
            var table = await _context.Tables
                .Include(t => t.Orders.Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Paid))
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
                return NotFound();

            var model = TableDetailsViewModel.FromTable(table);

            return PartialView("~/Areas/Admin/Views/Shared/_TableDetailPartial.cshtml", model);
        }
        [HttpGet]
        public async Task<IActionResult> GetTablesList()
        {
            try
            {
                // Tüm masaları getir
                var tables = await _context.Tables.ToListAsync();

                // Her masa için TableDetailsViewModel oluştur
                var tableViewModels = new List<TableDetailsViewModel>();

                foreach (var table in tables)
                {
                    // Aynı mantıkla aktif siparişleri getir
                    var activeOrders = await _context.Orders
                        .Include(o => o.OrderItems)
                            .ThenInclude(oi => oi.Product)
                        .Where(o => o.TableId == table.Id &&
                                  (o.Status == OrderStatus.Received ||
                                   o.Status == OrderStatus.Preparing ||
                                   o.Status == OrderStatus.Ready ||
                                   o.Status == OrderStatus.Delivered))
                        .ToListAsync();

                    var tableViewModel = new TableDetailsViewModel
                    {
                        Id = table.Id,
                        Name = table.Name,
                        Status = table.Status,
                        IsOccupied = activeOrders.Any(),
                        Orders = activeOrders.Select(o => new OrderViewModel
                        {
                            Id = o.Id,
                            TableId = o.TableId,
                            CustomerName = o.CustomerName,
                            CustomerNote = o.CustomerNote,
                            Status = o.Status,
                            OrderDate = o.OrderDate,
                            TotalAmount = (int)o.TotalAmount,
                            OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                            {
                                ProductId = oi.ProductId,
                                ProductName = oi.Product?.Name ?? "Ürün Bulunamadı",
                                Quantity = (int)oi.Quantity,
                                UnitPrice = oi.UnitPrice,
                                TotalPrice = oi.Quantity * oi.UnitPrice
                            }).ToList()
                        }).ToList()
                    };

                    tableViewModels.Add(tableViewModel);
                }

                return PartialView("_TablesPartial", tableViewModels);
            }
            catch (Exception ex)
            {
                // Hata logla
                System.Diagnostics.Debug.WriteLine($"GetTablesList Error: {ex.Message}");
                return PartialView("_ErrorPartial", "Masa listesi alınamadı: " + ex.Message);
            }
        }
    }
}