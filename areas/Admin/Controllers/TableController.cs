using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var table = await _context.Tables.FindAsync(id);
                if (table == null)
                {
                    TempData["Error"] = "Masa bulunamadı.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                // Aktif siparişi bul
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.TableId == id && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled)
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefaultAsync();

                var model = new TableDetailsViewModel
                {
                    Id = table.Id,
                    Name = table.Name,
                    IsOccupied = table.IsOccupied,
                    OccupiedSince = table.OccupiedSince,
                    CurrentOrderId = order?.Id
                };

                if (order != null)
                {
                    // Sipariş içeriğini doldur
                    foreach (var item in order.OrderItems)
                    {
                        model.OrderItems.Add(new OrderItemViewModel
                        {
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
            catch (Exception ex)
            {
                // Hata logla
                Console.WriteLine($"Error in Details: {ex}");
                TempData["Error"] = $"Masa detayları görüntülenirken hata oluştu: {ex.Message}";
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
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
        [HttpGet]
        public async Task<IActionResult> GetTableDetailPartial(int id)
        {
            try
            {
                var table = await _context.Tables.FindAsync(id);
                if (table == null)
                {
                    return PartialView("_ErrorPartial", "Masa bulunamadı.");
                }

                // Aktif siparişi daha kapsamlı bir şekilde kontrol et
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .Where(o => o.TableId == id &&
                              (o.Status == OrderStatus.Received ||
                               o.Status == OrderStatus.Preparing ||
                               o.Status == OrderStatus.Ready ||
                               o.Status == OrderStatus.Delivered))
                    .OrderByDescending(o => o.OrderDate)
                    .FirstOrDefaultAsync();

                // ÖNEMLİ: Masa durumunu ve sipariş durumunu tam olarak kontrolümüze almak için
                bool hasActiveOrder = order != null && order.OrderItems.Any();

                // Masa durumunu logla (debugging için)
                System.Diagnostics.Debug.WriteLine($"Masa #{id} DB'de IsOccupied={table.IsOccupied}, Gerçek durum: HasOrder={hasActiveOrder}");

                // View modeli oluştur - Burada masa durumuna bakılmaksızın gerçek sipariş varlığını kullan!
                var model = new TableDetailsViewModel
                {
                    Id = table.Id,
                    Name = table.Name,
                    IsOccupied = hasActiveOrder,  // Sipariş varlığına göre masa durumu belirleniyor
                    OccupiedSince = table.OccupiedSince,
                    CurrentOrderId = order?.Id,
                    OrderItems = new List<OrderItemViewModel>()
                };

                // Sipariş detaylarını ekle
                if (hasActiveOrder)
                {
                    foreach (var item in order.OrderItems)
                    {
                        model.OrderItems.Add(new OrderItemViewModel
                        {
                            ProductName = item.Product.Name,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            TotalPrice = item.Quantity * item.UnitPrice
                        });
                        model.TotalAmount += item.Quantity * item.UnitPrice;
                    }
                }

                return PartialView("_TableDetailPartial", model);
            }
            catch (Exception ex)
            {
                return PartialView("_ErrorPartial", $"Hata oluştu: {ex.Message}");
            }
        }

        // Siparişi tamamla ve masayı boşalt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAndClear(CompleteOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Geçersiz form bilgisi.";
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            try
            {
                // Masa ve siparişi kontrol et
                var table = await _context.Tables.FindAsync(model.TableId);
                if (table == null)
                {
                    TempData["Error"] = "Masa bulunamadı.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                var order = await _context.Orders.FindAsync(model.OrderId);
                if (order == null || order.TableId != model.TableId)
                {
                    TempData["Error"] = "Sipariş bulunamadı veya bu masaya ait değil.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

                // Siparişi ödendi olarak işaretle
                order.Status = OrderStatus.Paid;
                order.PaymentDate = DateTime.Now;

                // Masayı boşalt
                table.IsOccupied = false;
                table.OccupiedSince = null;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Sipariş tamamlandı ve masa boşaltıldı.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"İşlem sırasında hata oluştu: {ex.Message}";
                // Hata logla
                System.Diagnostics.Debug.WriteLine($"CompleteAndClear Error: {ex}");
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }


    }
}