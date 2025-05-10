using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models;
using QRCoder;
using RestaurantQRSystem.Models.Enums;
using RestaurantQRSystem.ViewModels;

namespace RestaurantQRSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TableController : Controller
    {
        private readonly ApplicationDbContext _context;

       

        public TableController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tables = await _context.Tables.OrderBy(t => t.Name).ToListAsync();
            return View(tables);
        }
        [HttpGet]
        public async Task<IActionResult> GetTableDetail(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            var currentOrder = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.TableId == id && o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync();

            var orderItems = new List<dynamic>();
            decimal totalAmount = 0;

            if (currentOrder != null)
            {
                foreach (var item in currentOrder.OrderItems)
                {
                    orderItems.Add(new
                    {
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.UnitPrice * item.Quantity
                    });

                    totalAmount +=item.UnitPrice * item.Quantity;
                }
            }

            return Json(new
            {
                table = new
                {
                    Id = table.Id,
                    Name = table.Name,
                    QrCodeUrl = Url.Action("ScanTable", "Order", new { area = "", tableId = table.Id }, Request.Scheme)
                },
                order = currentOrder == null ? null : new
                {
                    Id = currentOrder.Id,
                    Status = currentOrder.Status.ToString(),
                    OrderDate = currentOrder.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                    Items = orderItems,
                    TotalAmount = totalAmount,
                    ReadyForPayment = currentOrder.Status == OrderStatus.Delivered ||
                                     currentOrder.Status == OrderStatus.Ready
                }
            });
        }
        
        // Process payment and mark table as ready to clear
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int tableId, int orderId, string paymentMethod)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.TableId != tableId)
            {
                return NotFound();
            }

            // Create payment record
            var payment = new Order
            {
                Id = orderId,
                TotalAmount = order.TotalAmount,
                PaymentDate = DateTime.Now,
                PaymentMethod = paymentMethod,
                Status = (OrderStatus)PaymentStatus.Completed
            };

            _context.Orders.Add(payment);

            // Update order status
            order.Status = OrderStatus.Paid;
            order.PaymentStatus = PaymentStatus.Completed;

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }


        // GET: Admin/Table/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var table = await _context.Tables
                .Include(t => t.Orders.Where(o => o.Status != OrderStatus.Completed &&
                                                  o.Status != OrderStatus.Cancelled &&
                                                  o.Status != OrderStatus.Paid))
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
                return NotFound();

            var model = new TableDetailsViewModel
            {
                Id = table.Id,
                Name = table.Name,
                IsOccupied = table.IsOccupied,
                OccupiedSince = (DateTime?)table.OccupiedSince,
                OrderItems = new List<TableOrderItemViewModel>(),
                TotalAmount = 0
            };

            // Aktif sipariş var mı?
            var activeOrder = table.Orders.FirstOrDefault();
            if (activeOrder != null)
            {
                model.CurrentOrderId = activeOrder.Id;

                // Sipariş öğelerini ekle
                foreach (var item in activeOrder.OrderItems)
                {
                    model.OrderItems.Add(new TableOrderItemViewModel
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = (int)item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice
                    });

                    model.TotalAmount += item.Quantity * item.UnitPrice;
                }
            }

            return View(model);
        }

        // POST: Admin/Table/ReleaseTable/5
        [HttpPost]
        public async Task<IActionResult> ReleaseTable(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            // Find any active orders and mark them as complete
            var activeOrders = await _context.Orders
                .Where(o => o.TableId == id && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            foreach (var order in activeOrders)
            {
                order.Status = OrderStatus.Paid;
            }

            // Reset table status
            table.IsOccupied = false;
            table.OccupiedSince = null;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Table model)
        {
            if (ModelState.IsValid)
            {
                _context.Tables.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return NotFound();
            return View(table);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Table model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return NotFound();
            return View(table);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return NotFound();

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Qr(int id)
        {
            var table = _context.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null) return NotFound();

            // QR kod içeriği — örneğin masa menüsüne özel link
            string url = $"{Request.Scheme}://{Request.Host}/Menu/Scan/{table.QrCode}";

            using (var qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);

                // Yeni QRCoder kullanımı: BitmapByteQRCode
                BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
                byte[] qrBitmap = qrCode.GetGraphic(20);

                return File(qrBitmap, "image/png");
            }
        }
        [HttpGet]
        public IActionResult QrPrint(int id)
        {
            var table = _context.Tables.FirstOrDefault(t => t.Id == id);
            if (table == null) return NotFound();
            string url = $"{Request.Scheme}://{Request.Host}/Menu/Scan/{table.QrCode}";
            ViewBag.Table = table;
            ViewBag.QrUrl = url;
            return View();
        }
        // TableController.cs
        [HttpGet]
        public async Task<IActionResult> GetTableDetailPartial(int id)
        {
            try
            {
                // Log kaydı ekle
                System.Diagnostics.Debug.WriteLine($"GetTableDetailPartial çağrıldı: Table ID={id}");

                var table = await _context.Tables.FindAsync(id);
                if (table == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Masa bulunamadı: ID={id}");
                    return PartialView("_ErrorPartial", "Masa bulunamadı");
                }

                // Masa için aktif siparişleri direkt sorgula
                var activeOrders = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.TableId == id &&
                               (o.Status == OrderStatus.Received ||
                                o.Status == OrderStatus.Preparing ||
                                o.Status == OrderStatus.Ready ||
                                o.Status == OrderStatus.Delivered))
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"Masa ID={id} için {activeOrders.Count} aktif sipariş bulundu");

                // Model oluştur
                var model = new TableDetailsViewModel
                {
                    Id = table.Id,
                    Name = table.Name,
                    Status = table.Status,
                    Orders = activeOrders.Select(o => new OrderViewModel
                    {
                        Id = o.Id,
                        TableId = o.TableId,
                        CustomerName = o.CustomerName,
                        CustomerNote = o.CustomerNote,
                        OrderDate = o.OrderDate,
                        Status = o.Status,
                        TotalAmount = (int)o.TotalAmount,
                        OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                        {
                            ProductId = oi.ProductId,
                            ProductName = oi.Product.Name,
                            Quantity = (int)oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            TotalPrice = oi.UnitPrice * oi.Quantity
                        }).ToList()
                    }).ToList()
                };

                System.Diagnostics.Debug.WriteLine($"Model oluşturuldu: Orders={model.Orders.Count}");

                return PartialView("_TableDetailPartial", model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HATA: {ex.Message}\n{ex.StackTrace}");
                return PartialView("_ErrorPartial", $"Hata oluştu: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAndClear(int tableId, string paymentMethod = "Nakit")
        {
            try
            {
                // 1. Masayı kontrol et
                var table = await _context.Tables.FindAsync(tableId);
                if (table == null)
                {
                    TempData["Error"] = "Masa bulunamadı.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                // 2. Masadaki tüm aktif siparişleri getir
                var activeOrders = await _context.Orders
                    .Where(o => o.TableId == tableId &&
                          (o.Status == OrderStatus.Received ||
                           o.Status == OrderStatus.Preparing ||
                           o.Status == OrderStatus.Ready ||
                           o.Status == OrderStatus.Delivered))
                    .ToListAsync();

                if (!activeOrders.Any())
                {
                    TempData["Warning"] = "Masada işlenecek aktif sipariş bulunamadı.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                // 3. Aktif sipariş sayısını log kaydet (debug için)
                System.Diagnostics.Debug.WriteLine($"Masa {tableId} için {activeOrders.Count} aktif sipariş tamamlanacak");

                // 4. Her siparişi ödendi olarak işaretle
                foreach (var order in activeOrders)
                {
                    System.Diagnostics.Debug.WriteLine($"Sipariş #{order.Id} ödendi olarak işaretleniyor");

                    order.Status = OrderStatus.Paid;
                    order.PaymentStatus = PaymentStatus.Completed;
                    order.PaymentDate = DateTime.Now;
                    order.PaymentMethod = paymentMethod;
                    order.PaidAmount = order.TotalAmount;

                    _context.Update(order);
                }

                // 5. Masa durumunu güncelle (Status özelliği salt okunur ise IsOccupied'ı kullan)
                table.IsOccupied = false;
                table.OccupiedSince = null;
                _context.Update(table);

                // 6. Değişiklikleri kaydet
                await _context.SaveChangesAsync();

                // 7. SignalR ile güncellemeleri bildir (opsiyonel)
                // await _hubContext.Clients.All.SendAsync("TableStatusChanged", tableId);

                TempData["Success"] = $"Masa başarıyla boşaltıldı, {activeOrders.Count} sipariş ödendi olarak işaretlendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"İşlem sırasında hata oluştu: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"CompleteAndClear Error: {ex}");
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearTable(int tableId)
        {
            try
            {
                var table = await _context.Tables
                    .Include(t => t.Orders.Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Paid))
                    .FirstOrDefaultAsync(t => t.Id == tableId);

                if (table == null)
                {
                    TempData["Error"] = "Masa bulunamadı.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                // Tüm aktif siparişleri tamamlandı olarak işaretle
                foreach (var order in table.Orders)
                {
                    order.Status = OrderStatus.Paid;
                    order.PaymentStatus = PaymentStatus.Completed;
                    order.PaymentDate = DateTime.Now;
                    order.PaymentMethod = "Nakit";
                    order.PaidAmount = order.TotalAmount;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Masa başarıyla boşaltıldı, tüm siparişler tamamlandı.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"İşlem sırasında hata oluştu: {ex.Message}";
                // Hata logla
                System.Diagnostics.Debug.WriteLine($"ClearTable Error: {ex}");
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

    }
    public class CompleteOrderViewModel
    {
        public int TableId { get; set; }
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public decimal? PaidAmount { get; set; }
        public string? Notes { get; set; }
    }
}