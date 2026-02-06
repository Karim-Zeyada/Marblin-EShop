namespace Marblin.Core.Enums
{
    /// <summary>
    /// Payment method chosen by customer at checkout.
    /// </summary>
    public enum PaymentMethod
    {
        /// <summary>
        /// Cash on Delivery - Customer pays deposit now, remaining balance on delivery.
        /// </summary>
        CashOnDelivery = 0,
        
        /// <summary>
        /// Full Payment Upfront - Customer pays entire order amount now.
        /// </summary>
        FullPaymentUpfront = 1
    }
}
