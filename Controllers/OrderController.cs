using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Hubs;
using RestaurantQRSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Controllers
{
    public class OrderController : Controller
    {
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Create(int? tableId)
        {
            ViewBag.TableId = tableId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int TableId, string CartJson, string CustomerName, string CustomerNote)
        {
            if (!ModelState.IsValid)
            {
                return View(); // Hataları göster
            }
            // Gelen cart JSON Deserializasyonu
            var cartItems = JsonSerializer.Deserialize<List<CartItemDto>>(CartJson);
            if (cartItems == null || cartItems.Count == 0)
                return BadRequest("Sepet boş!");

            var table = await _context.Tables.FindAsync(TableId);
            if (table == null)
                return BadRequest("Masa bulunamadı.");

            decimal total = cartItems.Sum(x => x.price * x.quantity);

            var order = new Order
            {
                TableId = TableId,
                OrderDate = DateTime.Now,
                Status = (Models.Enums.OrderStatus)1,
                TotalAmount = total,
                CustomerName = CustomerName,
                CustomerNote = CustomerNote,
                OrderItems = cartItems.Select(x => new OrderItem
                {
                    ProductId = x.id,
                    Quantity = x.quantity,
                    UnitPrice = x.price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }

        // Sipariş geçmişiniz vs. eklemek için burada ek fonksiyonlar oluşturabilirsiniz.
    }

    // DTO
    public class CartItemDto
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
    }
}