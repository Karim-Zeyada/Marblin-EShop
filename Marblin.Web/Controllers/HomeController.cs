using System.Diagnostics;
using Marblin.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marblin.Core.Entities;
using Marblin.Core.Interfaces;
using Marblin.Core.Specifications;
using Microsoft.Extensions.Caching.Memory;

namespace Marblin.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IEmailService emailService, IMemoryCache cache)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            // Site Settings (cached 5 minutes)
            var settings = await _cache.GetOrCreateAsync("SiteSettings", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var spec = new SiteSettingsSpecification();
                return await _unitOfWork.Repository<SiteSettings>().GetEntityWithSpec(spec);
            });

            // Signature Pieces
            var signatureSpec = new SignatureProductsSpecification(6);
            var signaturePieces = await _unitOfWork.Repository<Product>().ListAsync(signatureSpec);

            // Featured Sale Products
            var saleSpec = new FeaturedSaleProductsSpecification(6);
            var saleProducts = await _unitOfWork.Repository<Product>().ListAsync(saleSpec);

            // Categories (cached 5 minutes)
            var categories = await _cache.GetOrCreateAsync("ActiveCategories", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var categoriesSpec = new CategoryWithProductsSpecification();
                var allCategories = await _unitOfWork.Repository<Category>().ListAsync(categoriesSpec);
                return allCategories.Where(c => c.IsActive).ToList();
            });

            var viewModel = new HomeViewModel
            {
                Settings = settings ?? new SiteSettings(),
                SignaturePieces = signaturePieces.ToList(),
                FeaturedSaleProducts = saleProducts.Where(p => p.IsOnSale()).ToList(),
                Categories = categories ?? new List<Category>()

            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _emailService.SendContactFormEmailAsync(model.Name, model.Email, model.Message);
                TempData["Success"] = "Thank you for your message! We'll get back to you soon.";
                return RedirectToAction(nameof(Contact));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact form email from {Email}", model.Email);
                TempData["Error"] = "Sorry, there was a problem sending your message. Please try again.";
                return View(model);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
