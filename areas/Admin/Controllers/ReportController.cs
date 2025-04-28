using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models;
using RestaurantQRSystem.Models.Enums;
using RestaurantQRSystem.ViewModels;
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

        // GET: Admin/Report
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            // Varsayılan tarih aralığı: Bu ay
            startDate ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            endDate ??= DateTime.Today.AddDays(1);

            // Tüm siparişleri getir
            var orders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Giderleri getir
            var expenses = await _context.Expenses
                .Where(e => e.Date >= startDate && e.Date < endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            // Rapor verilerini hesapla
            var totalSales = orders.Sum(o => o.TotalAmount);
            var totalExpenses = expenses.Sum(e => e.Amount);
            var profit = totalSales - totalExpenses;

            // Durumlara göre sipariş sayıları
            var receivedCount = orders.Count(o => o.Status == OrderStatus.Received);
            var preparingCount = orders.Count(o => o.Status == OrderStatus.Preparing);
            var readyCount = orders.Count(o => o.Status == OrderStatus.Ready);
            var deliveredCount = orders.Count(o => o.Status == OrderStatus.Delivered);
            var cancelledCount = orders.Count(o => o.Status == OrderStatus.Cancelled);

            // Kategori bazında satışlar
            var salesByCategory = orders
                .Where(o => o.Status != OrderStatus.Cancelled) // İptal edilmemiş siparişler
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Product.CategoryId)
                .Select(g => new CategorySalesViewModel
                {
                    CategoryId = g.Key,
                    CategoryName = _context.Categories.Find(g.Key)?.Name ?? "Bilinmeyen",
                    TotalSales = g.Sum(oi => oi.UnitPrice * oi.Quantity),
                    ItemCount = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(c => c.TotalSales)
                .ToList();

            // En çok satan ürünler
            var topProducts = orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new TopProductViewModel
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product.Name,
                    Quantity = g.Sum(oi => oi.Quantity),
                    TotalSales = g.Sum(oi => oi.UnitPrice * oi.Quantity)
                })
                .OrderByDescending(p => p.Quantity)
                .Take(10)
                .ToList();

            // ViewModel oluştur
            var viewModel = new ReportViewModel
            {
                StartDate = startDate.Value,
                EndDate = endDate.Value.AddDays(-1),
                Orders = orders,
                Expenses = expenses,
                TotalSales = totalSales,
                TotalExpenses = totalExpenses,
                Profit = profit,
                ReceivedCount = receivedCount,
                PreparingCount = preparingCount,
                ReadyCount = readyCount,
                DeliveredCount = deliveredCount,
                CancelledCount = cancelledCount,
                SalesByCategory = salesByCategory,
                TopProducts = topProducts
            };

            return View(viewModel);
        }

        // GET: Admin/Report/DailySales
        public async Task<IActionResult> DailySales(int month = 0, int year = 0)
        {
            // Varsayılan ay ve yıl şu anki zaman
            if (month == 0) month = DateTime.Today.Month;
            if (year == 0) year = DateTime.Today.Year;

            // Ay başlangıç ve bitiş tarihleri
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            // Günlük satışları getir
            var dailySales = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailySalesViewModel
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    TotalSales = g.Sum(o => o.TotalAmount),
                    CancelledCount = g.Count(o => o.Status == OrderStatus.Cancelled),
                    CancelledAmount = g.Where(o => o.Status == OrderStatus.Cancelled).Sum(o => o.TotalAmount)
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            var viewModel = new MonthlyReportViewModel
            {
                Month = month,
                Year = year,
                DailySales = dailySales,
                TotalSales = dailySales.Sum(d => d.TotalSales),
                TotalOrders = dailySales.Sum(d => d.OrderCount)
            };

            return View(viewModel);
        }

        // GET: Admin/Report/Expenses
        public IActionResult Expenses()
        {
            return View();
        }

        // POST: Admin/Report/AddExpense
        [HttpPost]
        public async Task<IActionResult> AddExpense(Expense expense)
        {
            if (ModelState.IsValid)
            {
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(expense);
        }

        // GET: Admin/Report/Invoice/5
        public async Task<IActionResult> Invoice(int id)
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

        // POST: Admin/Report/GenerateInvoice
        [HttpPost]
        public async Task<IActionResult> GenerateInvoice(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Fatura oluştur
            var invoice = new Invoice
            {
                OrderId = order.Id,
                InvoiceDate = DateTime.Now,
                Amount = order.TotalAmount,
                IsPaid = true,
                PaymentMethod = "Nakit", // Varsayılan değer
                CustomerName = order.CustomerName ?? "Misafir"
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Invoice), new { id = invoice.Id });
        }
    }
}