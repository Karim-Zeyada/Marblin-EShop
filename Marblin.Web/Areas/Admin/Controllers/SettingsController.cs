using Marblin.Core.Enums;
using Marblin.Core.Specifications;
using Marblin.Core.Interfaces;
using Marblin.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Marblin.Web.Areas.Admin.Models;

namespace Marblin.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Site settings including deposit percentage and CMS content.
    /// </summary>
    public class SettingsController : AdminBaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public SettingsController(IUnitOfWork unitOfWork, IFileService fileService, 
            UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Admin/Settings
        public async Task<IActionResult> Index()
        {
            var spec = new SiteSettingsSpecification();
            var settings = await _unitOfWork.Repository<SiteSettings>().GetEntityWithSpec(spec);
            
            if (settings == null)
            {
                settings = new SiteSettings
                {
                    DepositPercentage = 10m,
                    HeroHeadline = "Luxury Marble & Stone",
                    HeroSubheadline = "Handcrafted Excellence",
                    FeatureHeadline = "Bespoke Creations",
                    FeatureBody = "Have a specific design in mind? We verify your vision and bring it to life through our master craftsmanship.",
                    FeatureButtonText = "REQUEST A QUOTE",
                    FeatureButtonUrl = "/CustomRequest/Create"
                };
                _unitOfWork.Repository<SiteSettings>().Add(settings);
                await _unitOfWork.SaveChangesAsync();
            }
            return View(settings);
        }

        // POST: Admin/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SiteSettings model, IFormFile? heroImage, IFormFile? featureImage)
        {
            if (!ModelState.IsValid) return View(model);

            var spec = new SiteSettingsSpecification();
            var settings = await _unitOfWork.Repository<SiteSettings>().GetEntityWithSpec(spec);

            if (settings == null)
            {
                if (heroImage != null)
                {
                    using var stream = heroImage.OpenReadStream();
                     model.HeroImageUrl = await _fileService.SaveFileAsync(stream, heroImage.FileName, FileCategory.SiteAsset);
                }
                if (featureImage != null)
                {
                    using var stream = featureImage.OpenReadStream();
                     model.FeatureImageUrl = await _fileService.SaveFileAsync(stream, featureImage.FileName, FileCategory.SiteAsset);
                }
                _unitOfWork.Repository<SiteSettings>().Add(model);
            }
            else
            {
                settings.DepositPercentage = model.DepositPercentage;
                settings.HeroHeadline = model.HeroHeadline;
                settings.HeroSubheadline = model.HeroSubheadline;
                
                settings.FeatureHeadline = model.FeatureHeadline;
                settings.FeatureBody = model.FeatureBody;
                settings.FeatureButtonText = model.FeatureButtonText;
                settings.FeatureButtonUrl = model.FeatureButtonUrl;

                settings.ValueStatements = model.ValueStatements;
                settings.ProcessSteps = model.ProcessSteps;
                settings.InstapayAccount = model.InstapayAccount;
                settings.VodafoneCashNumber = model.VodafoneCashNumber;
                settings.CairoGizaShippingCost = model.CairoGizaShippingCost;
                settings.UpdatedAt = DateTime.UtcNow;

                if (heroImage != null)
                {
                    if (!string.IsNullOrEmpty(settings.HeroImageUrl))
                    {
                        try { _fileService.DeleteFile(settings.HeroImageUrl); } catch { /* Ignore */ }
                    }
                    using var stream = heroImage.OpenReadStream();
                    settings.HeroImageUrl = await _fileService.SaveFileAsync(stream, heroImage.FileName, FileCategory.SiteAsset);
                }
                if (featureImage != null)
                {
                    if (!string.IsNullOrEmpty(settings.FeatureImageUrl))
                    {
                        try { _fileService.DeleteFile(settings.FeatureImageUrl); } catch { /* Ignore */ }
                    }
                    using var stream = featureImage.OpenReadStream();
                    settings.FeatureImageUrl = await _fileService.SaveFileAsync(stream, featureImage.FileName, FileCategory.SiteAsset);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            TempData["Success"] = "Settings saved!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Settings/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var modelErrors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["PasswordError"] = $"Validation failed: {modelErrors}";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["PasswordError"] = "User not found. Please ensure you are logged in.";
                return RedirectToAction(nameof(Index));
            }

            // Verify current password first
            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                TempData["PasswordError"] = "Current password is incorrect.";
                return RedirectToAction(nameof(Index));
            }

            // Change the password
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            
            if (changePasswordResult.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["PasswordSuccess"] = $"Password changed successfully for {user.Email}!";
            }
            else
            {
                var errors = string.Join("; ", changePasswordResult.Errors.Select(e => e.Description));
                TempData["PasswordError"] = $"Password change failed: {errors}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
