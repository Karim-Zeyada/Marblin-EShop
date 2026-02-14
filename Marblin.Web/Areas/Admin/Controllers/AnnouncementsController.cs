using Marblin.Core.Entities;
using Marblin.Core.Interfaces;
using Marblin.Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace Marblin.Web.Areas.Admin.Controllers
{
    public class AnnouncementsController : AdminBaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public AnnouncementsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var spec = new AllAnnouncementsSpecification();
            var announcements = await _unitOfWork.Repository<Announcement>().ListAsync(spec);
            return View(announcements);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var announcement = new Announcement
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                IsActive = true,
                ShowOnHomepage = true
            };
            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement announcement)
        {
            if (announcement.EndDate <= announcement.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be later than start date.");
            }

            if (ModelState.IsValid)
            {
                announcement.CreatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Announcement>().Add(announcement);
                await _unitOfWork.SaveChangesAsync();
                TempData["Success"] = "Announcement created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var announcement = await _unitOfWork.Repository<Announcement>().GetByIdAsync(id);
            if (announcement == null) return NotFound();
            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Announcement announcement)
        {
            if (id != announcement.Id) return NotFound();

            if (announcement.EndDate <= announcement.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be later than start date.");
            }

            if (ModelState.IsValid)
            {
                var existing = await _unitOfWork.Repository<Announcement>().GetByIdAsync(id);
                if (existing == null) return NotFound();

                // Update properties
                existing.Title = announcement.Title;
                existing.Message = announcement.Message;
                existing.Style = announcement.Style;
                existing.StartDate = announcement.StartDate;
                existing.EndDate = announcement.EndDate;
                existing.IsActive = announcement.IsActive;
                existing.Priority = announcement.Priority;
                existing.ShowOnHomepage = announcement.ShowOnHomepage;
                existing.ShowOnCatalog = announcement.ShowOnCatalog;
                existing.ShowOnCheckout = announcement.ShowOnCheckout;

                await _unitOfWork.SaveChangesAsync();
                TempData["Success"] = "Announcement updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var announcement = await _unitOfWork.Repository<Announcement>().GetByIdAsync(id);
            if (announcement == null) return NotFound();

            _unitOfWork.Repository<Announcement>().Remove(announcement);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Announcement deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var announcement = await _unitOfWork.Repository<Announcement>().GetByIdAsync(id);
            if (announcement == null) return NotFound();

            announcement.IsActive = !announcement.IsActive;
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
