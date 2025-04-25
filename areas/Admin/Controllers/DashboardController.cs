using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models.Enums;
using RestaurantQRSystem.ViewModels;
using System;
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

            var viewModel = new AdminDashboardViewModel
            {
                TotalCategories = await _context.Categories.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalTables = await _context.Tables.CountAsync(),
                NewOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.New),
                ProcessingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Processing),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Completed),
                TodayOrders = todayOrders.Count,
                TodayRevenue = todayOrders.Sum(o => o.TotalAmount)
            };

            return View(viewModel);
        }
    }
} 
