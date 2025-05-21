using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using System.Linq;
using System.Threading.Tasks;
using RestaurantQRSystem.Models;
using RestaurantQRSystem.Models.Enums;
using Microsoft.AspNetCore.SignalR;
using RestaurantQRSystem.Hubs;
using RestaurantQRSystem.ViewModels;

namespace RestaurantQRSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IHubContext<OrderHub> _hubContext;
        public OrderController(ApplicationDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Tüm Siparişleri Listele
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
         .Include(o => o.Table)
         .Include(o => o.OrderItems)
             .ThenInclude(oi => oi.Product)
         .OrderByDescending(o => o.Id) // Bu satırı ekleyin/değiştirin
         .ToListAsync();

            return View(orders);
        }

        // Sipariş Detayları
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            // Get restaurant info
            var restaurantInfo = await _context.RestaurantInfos.FirstOrDefaultAsync();

            // Create the view model
            var viewModel = new OrderPrintViewModel
            {
                Order = order,
                RestaurantInfo = restaurantInfo
            };

            return View(viewModel); // Return the ViewModel instead of just the Order
        }

        // GET: Admin/Order/PrintReceipt/5
        [HttpGet]
        public async Task<IActionResult> PrintReceipt(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            // Get restaurant info
            var restaurantInfo = await _context.RestaurantInfos.FirstOrDefaultAsync();

            // Create the view model
            var viewModel = new OrderPrintViewModel
            {
                Order = order,
                RestaurantInfo = restaurantInfo
            };

            return View(viewModel);
        }

        // Sipariş Durumu Güncelle (Kısa method, POST olacak)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                
                if (order == null)
                {
                    return NotFound();
                }

                order.Status = status;
                _context.Update(order);
                await _context.SaveChangesAsync();
                await _hubContext.Clients.Group("AdminGroup").SendAsync(
                "ReceiveNewOrder",
                order.Id,
                order.TableId,
                order.CustomerName,
                order.TotalAmount
                );
                return RedirectToAction("Confirmation", new { id = order.Id });
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}