namespace Marblin.Core.Enums
{
    /// <summary>
    /// Order status workflow as per SRS.
    /// </summary>
    public enum OrderStatus
    {
        PendingPayment,
        InProduction,
        AwaitingBalance,
        Shipped,
        Cancelled
    }
}
