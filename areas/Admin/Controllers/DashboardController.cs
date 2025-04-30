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
            var today = DateTime.Today;

            // Siparişleri önce belleğe al, sonra filtrele ve topla
            var allOrders = await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            var todayOrders = allOrders
                .Where(o => o.OrderDate.Date == today)
                .ToList();

            // Genel dashboard istatistikleri
            var adminStats = new AdminDashboardViewModel
            {
                TotalCategories = await _context.Categories.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalTables = await _context.Tables.CountAsync(),
                NewOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Rejected),
                ProcessingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Preparing),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Ready),
                TodayOrders = todayOrders.Count,
                TodayRevenue = (double)todayOrders.Sum(o => o.TotalAmount)
            };

            // Son 24 saatteki siparişleri getir
            var recentOrders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderDate >= DateTime.Now.AddDays(-1))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Aktif siparişleri getir (Tamamlanmamış ve iptal edilmemiş)
            var activeOrders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // TÜM masaları getir (boş veya dolu)
            var tables = await _context.Tables.ToListAsync();

            // Her masa için detayları hazırla
            var tableList = new List<TableStatusViewModel>();

            foreach (var table in tables)
            {
                // Bu masa için aktif sipariş var mı?
                var currentOrder = activeOrders
                    .Where(o => o.TableId == table.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefault();

                var tableViewModel = new TableStatusViewModel
                {
                    Id = table.Id,
                    Name = table.Name,
                    IsOccupied = table.IsOccupied || currentOrder != null,
                    OccupiedSince = table.OccupiedSince,
                    CurrentOrderId = currentOrder?.Id,
                    TotalAmount = currentOrder?.TotalAmount ?? 0
                };

                tableList.Add(tableViewModel);
            }

            // En azından bir masa yoksa, manuel olarak bir masa ekleyelim (test için)
            if (tableList.Count == 0)
            {
                // Geçici test masası ekle
                tableList.Add(new TableStatusViewModel
                {
                    Id = 1,
                    Name = "Masa 1",
                    IsOccupied = false,
                    OccupiedSince = null,
                    CurrentOrderId = null,
                    TotalAmount = 0
                });
            }

            // Birleştirilmiş ViewModel oluştur
            var viewModel = new DashboardViewModel
            {
                AdminStats = adminStats,
                RecentOrders = recentOrders,
                ActiveOrders = activeOrders,
                TableList = tableList
            };

            return View(viewModel);
        }
    }
}