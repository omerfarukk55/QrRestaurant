using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // /Menu/Scan/{qrCode} QR'dan gelinen masa
        [HttpGet("Menu/Scan/{qrCode}")]
        public async Task<IActionResult> Scan(string qrCode)
        {
            var table = await _context.Tables.FirstOrDefaultAsync(t => t.QrCode == qrCode);
            if (table == null)
                return NotFound("Masa bulunamadı!");

            // Kategorileri ve ilgili ürünleri çek
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .Select(c => new
                {
                    c.Name,
                    Products = c.Products
                        .Where(p => p.IsAvailable)
                        .Select(p => new { p.Id, p.Name, p.Price, p.Description, p.ImageUrl })
                        .ToList()
                })
                .ToListAsync();

            ViewBag.Table = table;
            ViewBag.Categories = categories;
            return View("Menu");
        }
    }
}