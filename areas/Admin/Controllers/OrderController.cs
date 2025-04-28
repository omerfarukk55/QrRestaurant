using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models;
using RestaurantQRSystem.Models.Enums;
using RestaurantQRSystem.Services;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderNotificationService _notificationService;

        public OrderController(ApplicationDbContext context, OrderNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: Admin/Order
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Table)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Admin/Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Order/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            // SignalR ile bildirim gönder
            await _notificationService.SendOrderStatusUpdateAsync(id, status.ToString());

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}