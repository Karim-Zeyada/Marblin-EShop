using Marblin.Core.Entities;
using Marblin.Core.Interfaces;
using Marblin.Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marblin.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CouponsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            // Just list all for now
            var coupons = await _unitOfWork.Repository<Coupon>().GetAllAsync();
            return View(coupons);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                // Check uniqueness manually if using generic repo, or just let DB fail?
                // Better to check.
                var spec = new CouponByCodeSpecification(coupon.Code);
                var existing = await _unitOfWork.Repository<Coupon>().GetEntityWithSpec(spec);
                
                if (existing != null)
                {
                    ModelState.AddModelError("Code", "Coupon code already exists.");
                    return View(coupon);
                }

                _unitOfWork.Repository<Coupon>().Add(coupon);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
            if (coupon == null) return NotFound();
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
             if (id != coupon.Id) return NotFound();

             if (ModelState.IsValid)
             {
                 // Ensure code uniqueness if changed?
                 // For simplicity assume code shouldn't change or check logic.
                 // EF Tracked entity update mechanism:
                 // We should fetch tracked, update props, save.
                 
                 var existingCoupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
                 if (existingCoupon == null) return NotFound();
                 
                 // Check if code changed and if new code is taken
                 if (existingCoupon.Code != coupon.Code)
                 {
                      var spec = new CouponByCodeSpecification(coupon.Code);
                      var conflict = await _unitOfWork.Repository<Coupon>().GetEntityWithSpec(spec);
                      if (conflict != null)
                      {
                            ModelState.AddModelError("Code", "Coupon code already exists.");
                            return View(coupon);
                      }
                 }

                 existingCoupon.Code = coupon.Code;
                 existingCoupon.DiscountPercentage = coupon.DiscountPercentage;
                 existingCoupon.DiscountAmount = coupon.DiscountAmount;
                 existingCoupon.ExpiryDate = coupon.ExpiryDate;
                 existingCoupon.IsActive = coupon.IsActive;
                 existingCoupon.UsageLimit = coupon.UsageLimit;
                 
                 await _unitOfWork.SaveChangesAsync();
                 return RedirectToAction(nameof(Index));
             }
             return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _unitOfWork.Repository<Coupon>().GetByIdAsync(id);
            if (coupon != null)
            {
                _unitOfWork.Repository<Coupon>().Remove(coupon);
                await _unitOfWork.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
