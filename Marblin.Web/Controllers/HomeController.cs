using System.Diagnostics;
using Marblin.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marblin.Core.Entities;
using Marblin.Core.Interfaces;
using Marblin.Core.Specifications;

namespace Marblin.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            // Site Settings
            var settingsSpec = new SiteSettingsSpecification();
            var settings = await _unitOfWork.Repository<SiteSettings>().GetEntityWithSpec(settingsSpec);
            
            // Signature Pieces
            var signatureSpec = new SignatureProductsSpecification(6);
            var signaturePieces = await _unitOfWork.Repository<Product>().ListAsync(signatureSpec);

            // Categories
            var categoriesSpec = new CategoryWithProductsSpecification();
            var categories = await _unitOfWork.Repository<Category>().ListAsync(categoriesSpec);

            var viewModel = new HomeViewModel
            {
                Settings = settings ?? new SiteSettings(),
                SignaturePieces = signaturePieces.ToList(),
                Categories = categories.Where(c => c.IsActive).ToList()

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

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
