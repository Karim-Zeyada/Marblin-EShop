using Marblin.Core.Enums;
using Marblin.Core.Interfaces;
using Marblin.Core.Entities;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Marblin.Web.Controllers
{
    public class CustomRequestController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ILogger<CustomRequestController> _logger;

        public CustomRequestController(
            IUnitOfWork unitOfWork, 
            IFileService fileService,
            ILogger<CustomRequestController> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomRequest model, List<IFormFile> inspirationImages)
        {
            _logger.LogInformation("Receiving custom request inquiry from {CustomerName} ({Email})", 
                model.CustomerName, model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid custom request submission from {Email}. Errors: {ErrorCount}", 
                    model.Email, ModelState.ErrorCount);
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            model.Images = new List<CustomRequestImage>();

            if (inspirationImages != null && inspirationImages.Any())
            {
                foreach (var file in inspirationImages)
                {
                    if (file.Length > 0)
                    {
                        using var stream = file.OpenReadStream();
                        var relativePath = await _fileService.SaveFileAsync(stream, file.FileName, FileCategory.PrivateDocument);
                        model.Images.Add(new CustomRequestImage
                        {
                            ImageUrl = relativePath,
                            UploadedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            try
            {
                _unitOfWork.Repository<CustomRequest>().Add(model);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Custom request saved successfully for {Email}. ID: {RequestId}", 
                    model.Email, model.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save custom request for {Email}", model.Email);
                throw;
            }

            return View("Success");
        }
    }
}
