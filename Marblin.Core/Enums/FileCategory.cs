namespace Marblin.Core.Enums
{
    /// <summary>
    /// Defines the categories for file storage, eliminating magic strings
    /// and preventing path traversal vulnerabilities.
    /// </summary>
    public enum FileCategory
    {
        /// <summary>
        /// Product images stored in wwwroot/uploads/products
        /// </summary>
        ProductImage,

        /// <summary>
        /// Category images stored in wwwroot/uploads/categories
        /// </summary>
        CategoryImage,

        /// <summary>
        /// Site assets (hero, feature images) stored in wwwroot/uploads/assets
        /// </summary>
        SiteAsset,

        /// <summary>
        /// Payment receipt images stored in PrivateUploads/receipts
        /// </summary>
        ReceiptImage,

        /// <summary>
        /// Private documents (custom requests) stored in PrivateUploads/documents
        /// </summary>
        PrivateDocument
    }
}
