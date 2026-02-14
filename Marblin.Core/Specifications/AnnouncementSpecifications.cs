namespace Marblin.Core.Specifications
{
    using Marblin.Core.Entities;

    /// <summary>
    /// Specification to fetch active announcements for a specific page.
    /// </summary>
    public class ActiveAnnouncementsSpecification : BaseSpecification<Announcement>
    {
        public ActiveAnnouncementsSpecification(string pageName) : base(a =>
            a.IsActive
            && a.StartDate <= DateTime.Now
            && a.EndDate > DateTime.Now
            && (
                (pageName == "Homepage" && a.ShowOnHomepage) ||
                (pageName == "Catalog" && a.ShowOnCatalog) ||
                (pageName == "Checkout" && a.ShowOnCheckout)
            ))
        {
            ApplyOrderBy(a => a.Priority);
        }
    }
    
    /// <summary>
    /// Specification to fetch all announcements (for admin listing).
    /// </summary>
    public class AllAnnouncementsSpecification : BaseSpecification<Announcement>
    {
        public AllAnnouncementsSpecification() : base()
        {
            ApplyOrderByDescending(a => a.CreatedAt);
        }
    }
}

