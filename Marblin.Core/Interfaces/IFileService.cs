using Marblin.Core.Enums;

namespace Marblin.Core.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Saves a file stream to the specified category folder.
        /// </summary>
        /// <returns>Relative path to the saved file.</returns>
        Task<string> SaveFileAsync(Stream fileStream, string fileName, FileCategory category);

        /// <summary>
        /// Deletes a file given its relative URL path.
        /// </summary>
        void DeleteFile(string relativePath);

        /// <summary>
        /// Retrieves a file stream from the storage.
        /// </summary>
        /// <param name="category">The file category (determines storage location).</param>
        /// <param name="fileName">The specific file name.</param>
        /// <returns>FileStream of the requested file.</returns>
        Stream GetFileStream(string fileName, FileCategory category);
    }
}
