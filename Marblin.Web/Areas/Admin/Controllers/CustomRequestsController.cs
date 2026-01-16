using Marblin.Core.Specifications;
using Marblin.Core.Interfaces;
using Marblin.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marblin.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// View and manage custom product requests.
    /// </summary>
    public class CustomRequestsController : AdminBaseController
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomRequestsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Admin/CustomRequests
        public async Task<IActionResult> Index(bool? reviewed)
        {
            var spec = new CustomRequestWithImagesSpecification(reviewed);
            var requests = await _unitOfWork.Repository<CustomRequest>().ListAsync(spec);

            ViewBag.Reviewed = reviewed;
            return View(requests);
        }

        // GET: Admin/CustomRequests/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var spec = new CustomRequestWithImagesSpecification(id);
            var request = await _unitOfWork.Repository<CustomRequest>().GetEntityWithSpec(spec);

            if (request == null) return NotFound();
            return View(request);
        }

        // POST: Admin/CustomRequests/MarkReviewed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReviewed(int id, string? adminNotes)
        {
            var request = await _unitOfWork.Repository<CustomRequest>().GetByIdAsync(id);
            if (request == null) return NotFound();

            request.IsReviewed = true;
            request.AdminNotes = adminNotes;
            await _unitOfWork.SaveChangesAsync();

            TempData["Success"] = "Request marked as reviewed!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
