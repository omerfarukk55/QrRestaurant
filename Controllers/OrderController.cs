using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Hubs;
using RestaurantQRSystem.Models;
using RestaurantQRSystem.Models.Enums;
using RestaurantQRSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderController(ApplicationDbContext context, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: /Order/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? tableId)
        {
            try
            {
                if (tableId == null)
                {
                    ViewBag.Error = "Masa ID bulunamadı. Lütfen QR kodu tekrar okutun.";
                    return View("Error");
                }

                // Masa kontrolü
                var table = await _context.Tables.FindAsync(tableId);
                if (table == null)
                {
                    ViewBag.Error = $"Masa ID: {tableId} için masa bulunamadı.";
                    return View("Error");
                }

                // Masa bilgilerini ViewData'ya aktar
                ViewData["TableId"] = tableId;
                ViewData["TableName"] = table.Name;

                return View();
            }
            catch (Exception ex)
            {
                // Hatayı loglayın
                Console.WriteLine($"Create action error: {ex.Message}");
                ViewBag.Error = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return View("Error");
            }
        }

        // POST: /Order/Create
        [HttpPost]
        public async Task<IActionResult> Create(OrderViewModel model)
        {
            try
            {
                // Masa kontrolü
                var table = await _context.Tables.FindAsync(model.TableId);
                if (table == null)
                {
                    ModelState.AddModelError("", "Masa bulunamadı.");
                    return View(model);
                }

                // Sepet içeriğini deserialize ediyoruz
                List<CartItem> cartItems;
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    cartItems = JsonSerializer.Deserialize<List<CartItem>>(model.CartJson, options);

                    if (cartItems == null || !cartItems.Any())
                    {
                        ModelState.AddModelError("", "Sepetiniz boş.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Sepet verisi işlenemedi: " + ex.Message);
                    return View(model);
                }

                // Sipariş tutarını hesaplıyoruz
                decimal totalAmount = cartItems.Sum(item => item.price * item.quantity);

                // Yeni sipariş oluşturuyoruz
                var order = new Order
                {
                    TableId = model.TableId,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Received,
                    TotalAmount = totalAmount,
                    CustomerName = model.CustomerName,
                    CustomerNote = model.CustomerNote
                };

                // Sipariş kalemlerini ekliyoruz
                foreach (var item in cartItems)
                {
                    // ID string olarak geliyorsa int'e çeviriyoruz
                    if (!int.TryParse(item.id, out int productId))
                    {
                        // Hatalı ID formatını loglayabilirsiniz
                        continue;
                    }

                    // Ürünü veritabanından kontrol ediyoruz
                    var product = await _context.Products.FindAsync(productId);
                    if (product == null)
                    {
                        // Ürün bulunamadı hatası verilebilir
                        continue;
                    }

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = productId,
                        Quantity = item.quantity,
                        UnitPrice = item.price
                    });
                }

                // Siparişte öğe kalmadıysa hata dönüyoruz
                if (!order.OrderItems.Any())
                {
                    ModelState.AddModelError("", "Geçerli ürün bulunamadı.");
                    return View(model);
                }

                // Siparişi veritabanına kaydediyoruz
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Admin'e bildirim gönderiyoruz
                await NotifyOrderToAdmin(order.Id);

                // Başarılı sayfasına yönlendiriyoruz
                return RedirectToAction(nameof(Success), new { id = order.Id });
            }
            catch (Exception ex)
            {
                // Hatayı loglayabilirsiniz
                ModelState.AddModelError("", "Sipariş işlenirken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        // GET: /Order/Success/5
        public async Task<IActionResult> Success(int id)
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

        // Admin'e bildirim gönderme
        private async Task NotifyOrderToAdmin(int orderId)
        {
            try
            {
                // Sipariş ve masa bilgilerini alıyoruz
                var order = await _context.Orders
                    .Include(o => o.Table)
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order != null)
                {
                    // OrderHub üzerinden admin'e bildirim gönderiyoruz
                    await _hubContext.Clients.Group("Admins").SendAsync("ReceiveNewOrder",
                        new
                        {
                            OrderId = order.Id,
                            TableId = order.TableId,
                            
                            TableName = order.Table.Name,
                            CustomerName = order.CustomerName ?? "Misafir",
                            TotalAmount = order.TotalAmount,
                            ItemCount = order.OrderItems.Sum(i => i.Quantity),
                            OrderDate = order.OrderDate
                        });
                }
            }
            catch (Exception ex)
            {
                // Hatayı loglayabilirsiniz
                // Bildirim gönderme hatası sipariş sürecini etkilememelidir
            }
        }
    }

    // DTO sınıfları
    public class CartItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
    }
}