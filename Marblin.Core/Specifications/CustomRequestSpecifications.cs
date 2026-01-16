using Marblin.Core.Entities;
using Marblin.Core.Enums;
using System.Linq.Expressions;

namespace Marblin.Core.Specifications
{
    public class CustomRequestWithImagesSpecification : BaseSpecification<CustomRequest>
    {
        public CustomRequestWithImagesSpecification(int id) 
            : base(x => x.Id == id)
        {
            AddInclude(x => x.Images);
        }

        public CustomRequestWithImagesSpecification(bool? reviewed) 
            : base(x => !reviewed.HasValue || x.IsReviewed == reviewed.Value)
        {
            ApplyOrderByDescending(x => x.CreatedAt);
        }
    }
}
