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
        public async Task<IActionResult> GetTableDetails(int id)
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
        [HttpGet]
        public async Task<IActionResult> GetTableDetail(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            // Bu masa için aktif sipariş var mı?
            var currentOrder = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.TableId == id && o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync();

            var orderItems = new List<object>();
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

                    totalAmount += item.UnitPrice * item.Quantity;
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
                                     currentOrder.Status == OrderStatus.Ready,
                    PaymentStatus = currentOrder.PaymentStatus.ToString(),
                    PaymentMethod = currentOrder.PaymentMethod,
                    PaymentDate = currentOrder.PaymentDate
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

        // Clear the table and make it available
        [HttpPost]
        public async Task<IActionResult> ClearTable(int tableId)
        {
            var table = await _context.Tables.FindAsync(tableId);
            if (table == null)
            {
                return NotFound();
            }

            // Check if there are any active orders
            var activeOrder = await _context.Orders
                .Where(o => o.TableId == tableId &&
                      o.Status != OrderStatus.Paid &&
                      o.Status != OrderStatus.Cancelled)
                .FirstOrDefaultAsync();

            if (activeOrder != null)
            {
                // Mark the order as completed if it wasn't already
                activeOrder.Status = OrderStatus.Paid;
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true });
        }
        // GET: Admin/Table/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound();
            }

            // Get active order for this table
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.TableId == id && o.Status != OrderStatus.Paid && o.Status != OrderStatus.Cancelled)
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefaultAsync();

            var viewModel = new TableDetailsViewModel
            {
                Id = table.Id,
                Name = table.Name,
                IsOccupied = table.IsOccupied,
                OccupiedSince = table.OccupiedSince,
                QRCode = table.QrCode,
                CurrentOrder = order
            };

            return View(viewModel);
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

    }
}