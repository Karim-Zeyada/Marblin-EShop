using Marblin.Core.Enums;
using Marblin.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Marblin.Web.Areas.Admin.Controllers
{
    /// <summary>
    /// Secure controller for serving receipt files to authenticated admins.
    /// </summary>
    public class ReceiptsController : AdminBaseController
    {
        private readonly IFileService _fileService;

        public ReceiptsController(IFileService fileService)
        {
            _fileService = fileService;
        }

        // GET: Admin/Receipts/GetFile?fileName=abc.jpg&type=receipt
        [HttpGet]
        public IActionResult GetFile(string fileName, string type = "receipt")
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest();

            // Only allow serving private file categories through this endpoint
            var allowedCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                nameof(FileCategory.ReceiptImage),
                nameof(FileCategory.PrivateDocument)
            };

            if (!allowedCategories.Contains(type))
                return BadRequest("Invalid file type.");

            try
            {
                FileCategory category;
                if (Enum.TryParse<FileCategory>(type, true, out var parsed) && allowedCategories.Contains(parsed.ToString()))
                {
                    category = parsed;
                }
                else
                {
                    // Default to receipt for backward compatibility
                    category = FileCategory.ReceiptImage;
                }

                // Receipts/Documents are stored in private folders
                var stream = _fileService.GetFileStream(fileName, category);
                
                var contentType = GetContentType(fileName);
                return File(stream, contentType);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return BadRequest("Error retrieving file.");
            }
        }

        private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
