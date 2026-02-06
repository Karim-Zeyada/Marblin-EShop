using Marblin.Core.Enums;
using Marblin.Core.Specifications;
using Marblin.Core.Interfaces;
using Marblin.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Marblin.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Site settings including deposit percentage and CMS content.
    /// </summary>
    public class SettingsController : AdminBaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public SettingsController(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
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
    }
}
