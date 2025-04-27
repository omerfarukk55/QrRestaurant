using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging.Abstractions;
using RestaurantQRSystem.Models.DTOs;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace RestaurantQRSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToListAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductDto model, IFormFile imgFile)
        {
            if (ModelState.IsValid)
            {
                string imageUrl = null;

                if (imgFile != null && imgFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imgFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, "uploads", "products", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imgFile.CopyToAsync(stream);
                    }
                    imageUrl = "/uploads/products/" + fileName;
                }

                var product = new Product
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    IsAvailable = model.IsAvailable,
                    CategoryId = model.CategoryId,
                    ImageUrl = imageUrl  // Görsel yolunu ürün nesnesine atıyorum
                };

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model, IFormFile imgFile)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound();

                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.IsAvailable = model.IsAvailable;
                product.CategoryId = model.CategoryId;

                if (imgFile != null && imgFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(imgFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, "uploads", "products", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await imgFile.CopyToAsync(stream);
                    }
                    product.ImageUrl = "/uploads/products/" + fileName;
                }

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}