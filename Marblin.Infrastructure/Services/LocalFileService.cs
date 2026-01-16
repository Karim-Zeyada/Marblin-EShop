using Marblin.Core.Enums;
using Marblin.Core.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Marblin.Infrastructure.Services
{
    public class LocalFileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        // Magic Numbers for file signature validation
        private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new Dictionary<string, List<byte[]>>
        {
            { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }
        };

        public LocalFileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, FileCategory category)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            
            // 1. Signature Validation
            ValidateFileSignature(fileStream, ext);

            // 2. Get safe base path based on category (no user input in paths)
            var (basePath, isPublic, urlPrefix) = GetStorageInfo(category);

            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);

            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(basePath, uniqueFileName);

            using (var output = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(output);
            }

            if (isPublic)
            {
                return $"{urlPrefix}/{uniqueFileName}";
            }
            else
            {
                // Return just the filename for private files
                return uniqueFileName;
            }
        }

        public void DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            // Delete from WebRoot (public files)
            var filePath = Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public Stream GetFileStream(string fileName, FileCategory category)
        {
            // Sanitize filename to prevent path traversal
            var sanitizedFileName = Path.GetFileName(fileName);
            
            var (basePath, _, _) = GetStorageInfo(category);
            var filePath = Path.Combine(basePath, sanitizedFileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {sanitizedFileName}");
            }

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Gets storage information based on file category.
        /// Returns hardcoded safe paths - no user input flows into these paths.
        /// </summary>
        private (string basePath, bool isPublic, string urlPrefix) GetStorageInfo(FileCategory category)
        {
            return category switch
            {
                FileCategory.ProductImage => (
                    Path.Combine(_environment.WebRootPath, "uploads", "products"),
                    true,
                    "/uploads/products"
                ),
                FileCategory.CategoryImage => (
                    Path.Combine(_environment.WebRootPath, "uploads", "categories"),
                    true,
                    "/uploads/categories"
                ),
                FileCategory.SiteAsset => (
                    Path.Combine(_environment.WebRootPath, "uploads", "assets"),
                    true,
                    "/uploads/assets"
                ),
                FileCategory.ReceiptImage => (
                    Path.Combine(_environment.ContentRootPath, "PrivateUploads", "receipts"),
                    false,
                    string.Empty
                ),
                FileCategory.PrivateDocument => (
                    Path.Combine(_environment.ContentRootPath, "PrivateUploads", "documents"),
                    false,
                    string.Empty
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(category), $"Unknown file category: {category}")
            };
        }

        private void ValidateFileSignature(Stream fileStream, string extension)
        {
            if (!_fileSignatures.ContainsKey(extension))
            {
                throw new InvalidOperationException($"File type {extension} is not allowed.");
            }

            fileStream.Position = 0;
            using (var reader = new BinaryReader(fileStream, System.Text.Encoding.Default, true))
            {
                var signatures = _fileSignatures[extension];
                var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));
                
                bool match = signatures.Any(signature => 
                    headerBytes.Take(signature.Length).SequenceEqual(signature)
                );

                if (!match)
                {
                    throw new InvalidOperationException($"Invalid file signature for {extension}. Potential malicious file.");
                }
            }
            fileStream.Position = 0; // Reset position
        }
    }
}
