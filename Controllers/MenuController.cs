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
        public async Task<IActionResult> Menu(int tableId)
        {
            var table = await _context.Tables.FindAsync(tableId);
            if (table == null)
            {
                return NotFound();
            }

            // Fetch categories and their products
            var categories = await _context.Categories
        .Include(c => c.Products)
        .Where(c => c.IsActive)
        .ToListAsync();

            // Fetch restaurant info

            // Set ViewBag data
            ViewBag.Table = table;
            ViewBag.Categories = categories;
            
            ViewBag.RestaurantInfo = await _context.RestaurantInfos.FirstOrDefaultAsync();
            return View();
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

            // Bu satırı ekleyin - RestaurantInfo verisini yükle
            ViewBag.RestaurantInfo = await _context.RestaurantInfos.FirstOrDefaultAsync();

            ViewBag.Table = table;
            ViewBag.Categories = categories;
            return View("Menu");
        }
    }
}