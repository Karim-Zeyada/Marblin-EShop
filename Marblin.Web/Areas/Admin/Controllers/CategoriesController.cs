using Marblin.Core.Specifications;
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
        private readonly IFileService _fileService;

        public CategoriesController(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 15;
            
            var spec = new CategoryWithProductsSpecification();
            var allCategories = (await _unitOfWork.Repository<Category>().ListAsync(spec)).ToList();
            
            var totalCount = allCategories.Count;
            var paginatedCategories = allCategories
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
                
            var paginatedList = new Marblin.Core.Common.PaginatedList<Category>(
                paginatedCategories, totalCount, page, pageSize);
            
            return View(paginatedList);
        }

        // GET: Admin/Categories/Create
        public IActionResult Create()
        {
            return View(new Category());
        }

        // POST: Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model, IFormFile? file)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (file != null)
            {
                model.ImageUrl = await _fileService.SaveFileAsync(file.OpenReadStream(), file.FileName, Marblin.Core.Enums.FileCategory.CategoryImage);
            }

            model.SortOrder = await _unitOfWork.Repository<Category>().CountAsync(c => true);
            _unitOfWork.Repository<Category>().Add(model);
            await _unitOfWork.SaveChangesAsync();
            _unitOfWork.ClearCache("categories_list");

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
        public async Task<IActionResult> Edit(int id, Category model, IFormFile? file)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
            if (category == null) return NotFound();

            if (file != null)
            {
                // Basic cleanup of old image if needed, though strictly optional if we want to keep history or avoid broken links
                // if (!string.IsNullOrEmpty(category.ImageUrl)) _fileService.DeleteFile(category.ImageUrl);
                
                category.ImageUrl = await _fileService.SaveFileAsync(file.OpenReadStream(), file.FileName, Marblin.Core.Enums.FileCategory.CategoryImage);
            }

            category.Name = model.Name;
            category.Description = model.Description;
            // Only update ImageUrl if we uploaded a new one, or keep existing. 
            // If we want to allow clearing image, we'd need a specific flag or logic.
            // For now, if no file is uploaded, we essentially keep the old one (which is what we want).
            
            category.SortOrder = model.SortOrder;
            category.IsActive = model.IsActive;

            await _unitOfWork.SaveChangesAsync();
            _unitOfWork.ClearCache("categories_list");

            TempData["Success"] = "Category updated!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var spec = new CategoryWithProductsSpecification(id);
            var category = await _unitOfWork.Repository<Category>().GetEntityWithSpec(spec);
            
            if (category == null) return NotFound();

            if (category.Products.Any())
            {
                TempData["Error"] = "Cannot delete category with products. Move or delete products first.";
                return RedirectToAction(nameof(Index));
            }

            _unitOfWork.Repository<Category>().Remove(category);
            await _unitOfWork.SaveChangesAsync();
            _unitOfWork.ClearCache("categories_list");

            TempData["Success"] = "Category deleted!";
            return RedirectToAction(nameof(Index));
        }
    }
}
