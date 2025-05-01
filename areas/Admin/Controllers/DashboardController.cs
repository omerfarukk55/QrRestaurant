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
            // Masa durumlarını güncelle
            await UpdateTableStatuses();

            // Dashboard için gerekli verileri tek seferde hazırla
            var viewModel = new DashboardViewModel
            {
                AdminStats = await GetAdminStats(),
                TableList = await GetTableDetailsList()
            };

            return View(viewModel);
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

            // Bugünkü siparişleri getir
            var todayOrders = await _context.Orders
                .Where(o => o.OrderDate.Date == today && o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            return new AdminDashboardViewModel
            {
                TotalCategories = await _context.Categories.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalTables = await _context.Tables.CountAsync(),
                NewOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Received),
                ProcessingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Preparing),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Ready),
                TodayOrders = todayOrders.Count,
                TodayRevenue = (double)todayOrders.Sum(o => o.TotalAmount)
            };
        }

        // TableDetailsViewModel listesi oluştur (masalar için)
        private async Task<List<TableDetailsViewModel>> GetTableDetailsList()
        {
            var tables = await _context.Tables.OrderBy(t => t.Name).ToListAsync();
            var result = new List<TableDetailsViewModel>();

            foreach (var table in tables)
            {
                // Masa modeli oluştur
                var tableModel = new TableDetailsViewModel
                {
                    Id = table.Id,
                    Name = table.Name,
                    IsOccupied = table.IsOccupied,
                    OccupiedSince = table.OccupiedSince,
                    OrderItems = new List<OrderItemViewModel>()
                };

                // Aktif siparişi kontrol et
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.TableId == table.Id &&
                              (o.Status == OrderStatus.Received ||
                               o.Status == OrderStatus.Preparing ||
                               o.Status == OrderStatus.Ready ||
                               o.Status == OrderStatus.Delivered))
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefaultAsync();

                if (order != null)
                {
                    tableModel.CurrentOrderId = order.Id;

                    // Sipariş ürünlerini ekle
                    foreach (var item in order.OrderItems)
                    {
                        tableModel.OrderItems.Add(new OrderItemViewModel
                        {
                            ProductName = item.Product.Name,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalPrice = item.Quantity * item.UnitPrice
                        });

                        tableModel.TotalAmount += item.Quantity * item.UnitPrice;
                    }
                }

                result.Add(tableModel);
            }

            return result;
        }
    }
}