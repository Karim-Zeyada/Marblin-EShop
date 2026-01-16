
using System;

namespace Marblin.Core.Models
{
    public class OrderFinancials
    {
        public decimal Revenue { get; set; }
        public decimal Deposits { get; set; }
    }

    public class TopProductStat
    {
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }
}
