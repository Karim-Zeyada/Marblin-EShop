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

        // GET: Admin/Receipts/GetFile?fileName=abc.jpg
        [HttpGet]
        public IActionResult GetFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest();

            try
            {
                // Receipts are stored in "receipts" folder in PrivateUploads
                var stream = _fileService.GetFileStream(fileName, FileCategory.ReceiptImage);
                
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
