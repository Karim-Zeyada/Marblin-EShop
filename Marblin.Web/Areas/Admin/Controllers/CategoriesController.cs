using Marblin.Core.Interfaces;
using Marblin.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marblin.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// CRUD operations for Categories.
    /// </summary>
    public class CategoriesController : AdminBaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _unitOfWork.Repository<Category>().Query()
                .Include(c => c.Products)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();
            return View(categories);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View(new Category());
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.SortOrder = await _unitOfWork.Repository<Category>().Query().CountAsync();
            _unitOfWork.Repository<Category>().Add(model);
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Category created!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Categories/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.Description = model.Description;
            category.ImageUrl = model.ImageUrl;
            category.SortOrder = model.SortOrder;
            category.IsActive = model.IsActive;

            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Category updated!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _unitOfWork.Repository<Category>().Query()
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (category == null) return NotFound();

            if (category.Products.Any())
            {
                TempData["Error"] = "Cannot delete category with products. Move or delete products first.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.Repository<Category>().Remove(category);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Category deleted!";
            return RedirectToAction(nameof(Index));
        }
    }
}
