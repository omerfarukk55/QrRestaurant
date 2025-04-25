using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace RestaurantQRSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SettingsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var info = await _context.RestaurantInfos.FirstOrDefaultAsync() ?? new RestaurantInfo();
            return View(info);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RestaurantInfo model, IFormFile logoFile)
        {
            if (ModelState.IsValid)
            {
                // Logo işle
                if (logoFile != null && logoFile.Length > 0)
                {
                    var fileName = "logo_" + Path.GetFileName(logoFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(stream);
                    }
                    model.LogoUrl = "/uploads/" + fileName;
                }

                var current = await _context.RestaurantInfos.FirstOrDefaultAsync();

                if (current == null)
                {
                    _context.RestaurantInfos.Add(model);
                }
                else
                {
                    current.RestaurantName = model.RestaurantName;
                    current.Address = model.Address;
                    current.Email = model.Email;
                    current.Phone = model.Phone;
                    if (!string.IsNullOrEmpty(model.LogoUrl))
                        current.LogoUrl = model.LogoUrl;
                }
                await _context.SaveChangesAsync();
                TempData["Message"] = "Ayarlar başarıyla kaydedildi.";
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}