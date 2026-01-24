namespace Marblin.Core.Interfaces
{
    /// <summary>
    /// Abstraction for email sending operations.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a generic email.
        /// </summary>
        Task SendEmailAsync(string to, string subject, string htmlBody);

        /// <summary>
        /// Sends a contact form message to the admin.
        /// </summary>
        Task SendContactFormEmailAsync(string customerName, string customerEmail, string message);

        /// <summary>
        /// [FLOW 1] Sends an order confirmation email to the customer (Pending Deposit).
        /// </summary>
        Task SendOrderConfirmationEmailAsync(string customerEmail, string customerName, string orderNumber, 
            decimal totalAmount, decimal depositAmount, string instapay, string vodafoneCash, string proofUrl);

        /// <summary>
        /// [FLOW 2] Sends an email confirming receipt of payment proof.
        /// </summary>
        Task SendPaymentProofReceivedEmailAsync(string customerEmail, string customerName, string orderNumber);

        /// <summary>
        /// [FLOW 3] Sends an email confirming deposit verification and start of production.
        /// </summary>
        Task SendDepositVerifiedEmailAsync(string customerEmail, string customerName, string orderNumber, 
            decimal totalAmount, decimal remainingBalance);

        /// <summary>
        /// [FLOW 4] Sends an email requesting the remaining balance.
        /// </summary>
        Task SendAwaitingBalanceEmailAsync(string customerEmail, string customerName, string orderNumber, 
            decimal totalAmount, decimal depositPaid, decimal remainingBalance, string instapay, string vodafoneCash);

        /// <summary>
        /// [FLOW 5] Sends an email confirming order shipment.
        /// </summary>
        Task SendOrderShippedEmailAsync(string customerEmail, string customerName, string orderNumber);
    }
}
