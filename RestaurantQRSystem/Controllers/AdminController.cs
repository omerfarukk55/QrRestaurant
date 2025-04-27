using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models.Enums;
using RestaurantQRSystem.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;

            var viewModel = new AdminDashboardViewModel
            {
                TotalCategories = await _context.Categories.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalTables = await _context.Tables.CountAsync(),
                NewOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.New),
                ProcessingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Processing),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Completed),
                TodayOrders = await _context.Orders.CountAsync(o => o.OrderDate.Date == today),
                TodayRevenue = await _context.Orders
                                   .Where(o => o.OrderDate.Date == today && o.Status != OrderStatus.Cancelled)
                                   .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                TableList = await _context.Tables.Select(t => new TableStatusViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    IsOccupied = t.IsOccupied
                }).ToListAsync()
            };

            return View(viewModel);
        }
    }
}