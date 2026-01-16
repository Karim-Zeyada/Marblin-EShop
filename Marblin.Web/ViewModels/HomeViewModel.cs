using Marblin.Core.Entities;

namespace Marblin.Web.ViewModels
{
    public class HomeViewModel
    {
        public SiteSettings Settings { get; set; } = new();
        public List<Product> SignaturePieces { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }
}
