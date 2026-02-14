using Marblin.Core.Entities;
using Marblin.Core.Interfaces;
using Marblin.Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace Marblin.Web.Areas.Admin.Controllers
{
    public class CouponsController : AdminBaseController
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
            // Validate exactly one discount type
            if (!coupon.DiscountPercentage.HasValue && !coupon.DiscountAmount.HasValue)
            {
                ModelState.AddModelError("", "Either discount percentage or discount amount is required.");
            }
            else if (coupon.DiscountPercentage.HasValue && coupon.DiscountAmount.HasValue)
            {
                ModelState.AddModelError("", "Set either discount percentage or discount amount, not both.");
            }

            if (ModelState.IsValid)
            {
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

            // Validate exactly one discount type
            if (!coupon.DiscountPercentage.HasValue && !coupon.DiscountAmount.HasValue)
            {
                ModelState.AddModelError("", "Either discount percentage or discount amount is required.");
            }
            else if (coupon.DiscountPercentage.HasValue && coupon.DiscountAmount.HasValue)
            {
                ModelState.AddModelError("", "Set either discount percentage or discount amount, not both.");
            }

            if (ModelState.IsValid)
            {
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
            if (coupon == null) return NotFound();

            _unitOfWork.Repository<Coupon>().Remove(coupon);
            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Coupon deleted!";
            return RedirectToAction(nameof(Index));
        }
    }
}
