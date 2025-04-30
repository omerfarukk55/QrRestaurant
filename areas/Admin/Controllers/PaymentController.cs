using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models;
using RestaurantQRSystem.Models.Enums;
using RestaurantQRSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static RestaurantQRSystem.ViewModels.PaymentViewModel;

namespace RestaurantQRSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Payment
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, PaymentStatus? status)
        {
            // Varsayılan tarih aralığı: Son 30 gün
            startDate ??= DateTime.Today.AddDays(-30);
            endDate ??= DateTime.Today.AddDays(1); // Bugünü de dahil etmek için +1 gün

            // Ödemeleri filtrele
            var query = _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Table)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate < endDate)
                .AsQueryable();

            // Eğer durum filtresi varsa uygula
            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            // Ödemeleri getir ve tarihe göre sırala
            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // İstatistikleri hesapla
            var statistics = new PaymentStatisticsViewModel
            {
                TotalAmount = payments.Sum(p => p.Amount),
                CashAmount = payments.Where(p => p.PaymentMethod == "Nakit").Sum(p => p.Amount),
                CardAmount = payments.Where(p => p.PaymentMethod == "Kredi Kartı" || p.PaymentMethod == "Banka Kartı").Sum(p => p.Amount),
                OtherAmount = payments.Where(p => p.PaymentMethod != "Nakit" && p.PaymentMethod != "Kredi Kartı" && p.PaymentMethod != "Banka Kartı").Sum(p => p.Amount),
                CompletedCount = payments.Count(p => p.Status == PaymentStatus.Completed),
                PendingCount = payments.Count(p => p.Status == PaymentStatus.Pending),
                RefundedCount = payments.Count(p => p.Status == PaymentStatus.Refunded)
            };

            // ViewModel oluştur
            var viewModel = new PaymentIndexViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value.AddDays(-1), // UI için 1 gün çıkar
                Status = status,
                Payments = payments,
                Statistics = statistics
            };

            return View(viewModel);
        }

        // GET: Admin/Payment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Table)
                .Include(p => p.Order.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Admin/Payment/Create
        public async Task<IActionResult> Create(int? orderId)
        {
            // Eğer orderId varsa, sipariş detaylarını doldur
            if (orderId.HasValue)
            {
                var order = await _context.Orders
                    .Include(o => o.Table)
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound();
                }

                var viewModel = new CreatePaymentViewModel
                {
                    OrderId = order.Id,
                    TableName = order.Table.Name,
                    OrderDate = order.OrderDate,
                    Amount = order.TotalAmount,
                    CustomerName = order.CustomerName,
                    OrderItems = order.OrderItems.ToList()
                };

                return View(viewModel);
            }

            // Eğer orderId yoksa, boş form göster
            ViewBag.Orders = await _context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Pending)
                .Select(o => new { o.Id, Text = $"#{o.Id} - Masa {o.Table.Name} - {o.TotalAmount:C2}" })
                .ToListAsync();

            return View(new CreatePaymentViewModel());
        }

        // POST: Admin/Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePaymentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // İlgili siparişi kontrol et
                var order = await _context.Orders.FindAsync(model.OrderId);
                if (order == null)
                {
                    return NotFound();
                }

                // Yeni ödeme oluştur
                var payment = new Payment
                {
                    OrderId = model.OrderId,
                    Amount = model.Amount,
                    PaymentMethod = model.PaymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = PaymentStatus.Completed,
                    Notes = model.Notes
                };

                _context.Payments.Add(payment);

                // Sipariş bilgilerini güncelle
                order.PaymentStatus = PaymentStatus.Completed;
                order.PaymentDate = DateTime.Now;
                order.PaymentMethod = model.PaymentMethod;
                order.PaidAmount = model.Amount;
                order.PaymentNotes = model.Notes;

                // Eğer istenirse, siparişi tamamla
                if (model.CompleteOrder)
                {
                    order.Status = OrderStatus.Paid;

                    // Masayı kontrol et
                    var hasMoreActiveOrders = await _context.Orders
                        .AnyAsync(o => o.TableId == order.TableId
                                && o.Id != order.Id
                                && o.Status != OrderStatus.Paid
                                && o.Status != OrderStatus.Cancelled);

                    if (!hasMoreActiveOrders)
                    {
                        // Masayı boşalt
                        var table = await _context.Tables.FindAsync(order.TableId);
                        if (table != null)
                        {
                            table.IsOccupied = false;
                            table.OccupiedSince = null;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = payment.Id });
            }

            // Form geçersizse sipariş listesini yeniden doldur
            ViewBag.Orders = await _context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Pending)
                .Select(o => new { o.Id, Text = $"#{o.Id} - Masa {o.Table.Name} - {o.TotalAmount:C2}" })
                .ToListAsync();

            return View(model);
        }

        // POST: Admin/Payment/ProcessPayment
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int orderId, string paymentMethod, decimal amount, string notes)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Sipariş bulunamadı" });
            }

            try
            {
                // Yeni ödeme oluştur
                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = amount,
                    PaymentMethod = paymentMethod,
                    PaymentDate = DateTime.Now,
                    Status = PaymentStatus.Completed,
                    Notes = notes
                };

                _context.Payments.Add(payment);

                // Sipariş bilgilerini güncelle
                order.PaymentStatus = PaymentStatus.Completed;
                order.PaymentDate = DateTime.Now;
                order.PaymentMethod = paymentMethod;
                order.PaidAmount = amount;
                order.PaymentNotes = notes;

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    paymentId = payment.Id,
                    paymentDate = payment.PaymentDate,
                    message = "Ödeme başarıyla alındı"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Ödeme işlemi sırasında bir hata oluştu: " + ex.Message });
            }
        }

        // POST: Admin/Payment/RefundPayment
        [HttpPost]
        public async Task<IActionResult> RefundPayment(int paymentId, string refundReason)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return Json(new { success = false, message = "Ödeme bulunamadı" });
            }

            try
            {
                // Ödeme durumunu güncelle
                payment.Status = PaymentStatus.Refunded;
                payment.Notes = (payment.Notes ?? "") + $"\nİade Nedeni: {refundReason} - İade Tarihi: {DateTime.Now}";

                // Sipariş durumunu güncelle
                var order = payment.Order;
                order.PaymentStatus = PaymentStatus.Refunded;
                order.PaymentNotes = (order.PaymentNotes ?? "") + $"\nİade Nedeni: {refundReason} - İade Tarihi: {DateTime.Now}";

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Ödeme başarıyla iade edildi",
                    paymentMethod = payment.PaymentMethod,
                    paymentDate = payment.PaymentDate
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "İade işlemi sırasında bir hata oluştu: " + ex.Message });
            }
        }

        // GET: Admin/Payment/Report
        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
        {
            // Varsayılan tarih aralığı: Bu ay
            startDate ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            endDate ??= DateTime.Today.AddDays(1);

            // Tüm ödemeleri getir
            var payments = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate < endDate && p.Status == PaymentStatus.Completed)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Ödeme yöntemlerine göre grupla
            var paymentsByMethod = payments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodSummary
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(p => p.Amount)
                })
                .OrderByDescending(p => p.Amount)
                .ToList();

            // Günlük ödeme toplamları
            var dailyPayments = payments
                .GroupBy(p => p.PaymentDate.Date)
                .Select(g => new DailyPaymentSummary
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(p => p.Amount)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // İptal edilen ödemeler
            var refundedPayments = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate < endDate && p.Status == PaymentStatus.Refunded)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Tüm ödemelerin toplamı
            var totalAmount = payments.Sum(p => p.Amount);
            var totalRefundedAmount = refundedPayments.Sum(p => p.Amount);
            var netAmount = totalAmount - totalRefundedAmount;

            // ViewModel oluştur
            var viewModel = new PaymentReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value.AddDays(-1),
                TotalPayments = payments.Count,
                TotalAmount = totalAmount,
                RefundedPayments = refundedPayments.Count,
                RefundedAmount = totalRefundedAmount,
                NetAmount = netAmount,
                PaymentsByMethod = paymentsByMethod,
                DailyPayments = dailyPayments,
                RefundedPaymentsList = refundedPayments
            };

            return View(viewModel);
        }

        // GET: Admin/Payment/ExportReport
        public async Task<IActionResult> ExportReport(DateTime? startDate, DateTime? endDate)
        {
            // Varsayılan tarih aralığı: Bu ay
            startDate ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            endDate ??= DateTime.Today.AddDays(1);

            // Tüm ödemeleri getir
            var payments = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Table)
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate < endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // CSV içeriği oluştur
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Ödeme ID,Sipariş ID,Masa,Tarih,Tutar,Ödeme Yöntemi,Durum,Notlar");

            foreach (var payment in payments)
            {
                // CSV satırı oluştur (virgüller ve tırnak işaretleri için kaçış işaretleri ekle)
                csv.AppendLine($"{payment.Id}," +
                    $"{payment.OrderId}," +
                    $"\"{payment.Order.Table?.Name ?? "Bilinmeyen"}\"," +
                    $"{payment.PaymentDate.ToString("dd.MM.yyyy HH:mm:ss")}," +
                    $"{payment.Amount}," +
                    $"\"{payment.PaymentMethod}\"," +
                    $"\"{payment.Status}\"," +
                    $"\"{(payment.Notes?.Replace("\"", "\"\"") ?? "")}\"");
            }

            // CSV dosyasını indir
            var formattedStartDate = startDate.Value.ToString("yyyyMMdd");
            var formattedEndDate = endDate.Value.AddDays(-1).ToString("yyyyMMdd");
            var fileName = $"Odemeler_{formattedStartDate}_{formattedEndDate}.csv";

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        // GET: Admin/Payment/GetOrderDetails
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = new
            {
                id = order.Id,
                tableName = order.Table.Name,
                orderDate = order.OrderDate.ToString("dd.MM.yyyy HH:mm"),
                totalAmount = order.TotalAmount,
                status = order.Status.ToString(),
                paymentStatus = order.PaymentStatus.ToString(),
                customerName = order.CustomerName,
                items = order.OrderItems.Select(oi => new
                {
                    productName = oi.Product.Name,
                    quantity = oi.Quantity,
                    unitPrice = oi.UnitPrice,
                    totalPrice = oi.Quantity * oi.UnitPrice
                }).ToList()
            };

            return Json(orderDetails);
        }

        // GET: Admin/Payment/PrintReceipt/5
        public async Task<IActionResult> PrintReceipt(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Table)
                .Include(p => p.Order.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Admin/Payment/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, PaymentStatus status, string notes)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
            {
                return Json(new { success = false, message = "Ödeme bulunamadı" });
            }

            // Update payment status
            payment.Status = status;
            payment.Notes = (payment.Notes ?? "") + $"\nDurum Güncelleme: {status} - {DateTime.Now} - {notes}";

            // Update order payment status
            payment.Order.PaymentStatus = status;
            payment.Order.PaymentNotes = (payment.Order.PaymentNotes ?? "") + $"\nÖdeme Durumu: {status} - {DateTime.Now} - {notes}";

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Ödeme durumu güncellendi" });
        }

        // GET: Admin/Payment/Daily
        public async Task<IActionResult> Daily(DateTime? date)
        {
            // Varsayılan tarih: Bugün
            date ??= DateTime.Today;

            // Günün başlangıç ve bitiş saatleri
            var startTime = date.Value.Date;
            var endTime = startTime.AddDays(1);

            // O günün ödemelerini getir
            var payments = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Table)
                .Where(p => p.PaymentDate >= startTime && p.PaymentDate < endTime)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            // Ödeme yöntemlerine göre grupla
            var paymentsByMethod = payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodSummary
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(p => p.Amount)
                })
                .OrderByDescending(p => p.Amount)
                .ToList();

            // Saatlik ödeme dağılımı (09:00-23:00 arası)
            var hourlyPayments = new List<HourlyPaymentSummary>();
            for (int hour = 9; hour <= 23; hour++)
            {
                var hourStart = startTime.AddHours(hour);
                var hourEnd = hourStart.AddHours(1);

                var paymentsInHour = payments
                    .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentDate >= hourStart && p.PaymentDate < hourEnd)
                    .ToList();

                hourlyPayments.Add(new HourlyPaymentSummary
                {
                    Hour = hour,
                    TimeSlot = $"{hour:00}:00 - {hour + 1:00}:00",
                    Count = paymentsInHour.Count,
                    Amount = paymentsInHour.Sum(p => p.Amount)
                });
            }

            // Diğer istatistikler
            var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed).ToList();
            var refundedPayments = payments.Where(p => p.Status == PaymentStatus.Refunded).ToList();

            var viewModel = new DailyPaymentViewModel
            {
                Date = date.Value,
                Payments = payments,
                CompletedPaymentCount = completedPayments.Count,
                CompletedPaymentAmount = completedPayments.Sum(p => p.Amount),
                RefundedPaymentCount = refundedPayments.Count,
                RefundedPaymentAmount = refundedPayments.Sum(p => p.Amount),
                NetAmount = completedPayments.Sum(p => p.Amount) - refundedPayments.Sum(p => p.Amount),
                PaymentsByMethod = paymentsByMethod,
                HourlyPayments = hourlyPayments
            };

            return View(viewModel);
        }
    }
}