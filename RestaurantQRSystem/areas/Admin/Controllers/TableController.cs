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