using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            // Varsayılan aralık bu ay olsun
            startDate ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            endDate ??= DateTime.Today.AddDays(1);

            var orders = await _context.Orders
                .Include(o => o.Table)
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                .Where(o => o.Status == OrderStatus.Completed || o.Status == OrderStatus.Cancelled)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.TotalSales = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalAmount);
            ViewBag.TotalCount = orders.Count(o => o.Status == OrderStatus.Completed);
            ViewBag.TotalCancelled = orders.Count(o => o.Status == OrderStatus.Cancelled);
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.AddDays(-1).ToString("yyyy-MM-dd"); // arama için gün dahil bitsin

            return View(orders);
        }
    }
}