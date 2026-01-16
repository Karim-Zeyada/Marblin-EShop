using System.Diagnostics;
using Marblin.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marblin.Core.Entities;
using Marblin.Core.Interfaces;

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
            var settings = await _unitOfWork.Repository<SiteSettings>().Query().FirstOrDefaultAsync();
            
            var viewModel = new HomeViewModel
            {
                Settings = settings ?? new SiteSettings(),
                SignaturePieces = await _unitOfWork.Repository<Product>().Query()
                    .Include(p => p.Images)
                    .Include(p => p.Category)
                    .Where(p => p.IsSignaturePiece && p.IsActive)
                    .Take(6)
                    .ToListAsync(),
                Categories = await _unitOfWork.Repository<Category>().Query()
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ToListAsync()
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
