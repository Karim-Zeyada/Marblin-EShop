using Marblin.Core.Entities;
using System.Linq.Expressions;

namespace Marblin.Core.Specifications
{
    public class CouponByCodeSpecification : BaseSpecification<Coupon>
    {
        public CouponByCodeSpecification(string code) 
            : base(c => c.Code == code)
        {
        }
    }
}
