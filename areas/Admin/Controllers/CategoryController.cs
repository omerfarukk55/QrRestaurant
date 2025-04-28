using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantQRSystem.Controllers;
using RestaurantQRSystem.Data;
using RestaurantQRSystem.Models;
using RestaurantQRSystem.Models.DTOs;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantQRSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.OrderBy(c => c.DisplayOrder).ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto model)
        {
            if (ModelState.IsValid)
            {
                 var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {       
                System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
                }

                var Category = new Category
                {
                    Name = model.Name,
                    Description = model.Description,
                    DisplayOrder = model.DisplayOrder
                };

                await _context.Categories.AddAsync(Category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryDto model)
        {
            //if (id != model.Id)
            //  return NotFound();
            var category = await _context.Categories.FindAsync(id);
            if (ModelState.IsValid)
            {
                category.Name = model.Name;
                category.Description = model.Description;
                category.DisplayOrder = model.DisplayOrder;
                category.IsActive = model.IsActive;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
