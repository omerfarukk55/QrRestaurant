using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models;
using RestaurantQRSystem.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;

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

            // Map entity to view model
            var viewModel = new RestaurantSettingsViewModel
            {
                RestaurantName = info.RestaurantName,
                Description = info.Description ?? "",
                Address = info.Address ?? "",
                PhoneNumber = info.Phone ?? "",
                Email = info.Email ?? "",
                CurrentLogoPath = info.LogoUrl,
                FacebookUrl = info.FacebookUrl ?? "",
                InstagramUrl = info.InstagramUrl ?? "",
                ShowLogo = info.ShowLogo,
                WorkingHours = info.WorkingHours ?? "",
                TaxNumber = info.TaxNumber ?? "",
                Currency = info.Currency ?? "₺"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RestaurantSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Field: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            try
            {
                var current = await _context.RestaurantInfos.FirstOrDefaultAsync();

                if (current == null)
                {
                    current = new RestaurantInfo { Id = 1 };
                    _context.RestaurantInfos.Add(current);
                    Console.WriteLine("Creating new RestaurantInfo entity");
                }
                else
                {
                    Console.WriteLine($"Updating existing RestaurantInfo with ID: {current.Id}");
                    // Detach and re-attach to ensure clean tracking
                    _context.Entry(current).State = EntityState.Detached;
                    _context.Entry(current).State = EntityState.Modified;
                }

           
                    // Logo processing code...
                    if (model.LogoFile != null && model.LogoFile.Length > 0)
                    {
                        // Eski logoyu sil
                        if (!string.IsNullOrEmpty(current.LogoUrl))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, "images", "logos", Path.GetFileName(current.LogoUrl));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Yeni logoyu kaydet
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "logos");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.LogoFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.LogoFile.CopyToAsync(fileStream);
                        }

                        current.LogoUrl = "/images/logos/" + uniqueFileName;
                    }
                

                // Update all properties
                current.RestaurantName = model.RestaurantName;
                current.Description = model.Description;
                current.Address = model.Address;
                current.Phone = model.PhoneNumber;
                current.Email = model.Email;
                current.FacebookUrl = model.FacebookUrl;
                current.InstagramUrl = model.InstagramUrl;
                current.ShowLogo = model.ShowLogo;
                current.WorkingHours = model.WorkingHours;
                current.TaxNumber = model.TaxNumber;
                current.Currency = model.Currency;
                current.LastUpdated = DateTime.Now;

                Console.WriteLine($"About to save - Name: {current.RestaurantName}, Address: {current.Address}");

                var saveResult = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChangesAsync result: {saveResult} records affected");

                // Verify save was successful
                var verification = await _context.RestaurantInfos.AsNoTracking().FirstOrDefaultAsync();
                Console.WriteLine($"Verification - ID: {verification?.Id}, Name: {verification?.RestaurantName}, Updated: {verification?.LastUpdated}");

                TempData["Success"] = "Restoran ayarları başarıyla kaydedildi.";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database update error: {dbEx.Message}");
                Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                ModelState.AddModelError("", "Veritabanı güncelleme hatası: " + dbEx.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", "Ayarlar kaydedilirken bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }
    }
}