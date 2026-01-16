namespace Marblin.Core.Enums
{
    /// <summary>
    /// Order status workflow as per SRS.
    /// </summary>
    public enum OrderStatus
    {
        PendingDeposit,
        InProduction,
        AwaitingBalance,
        Shipped,
        Cancelled
    }
}
