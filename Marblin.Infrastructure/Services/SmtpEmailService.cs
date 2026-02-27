using Marblin.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Marblin.Infrastructure.Services
{
    /// <summary>
    /// SMTP implementation of IEmailService using MailKit for standard SMTP delivery.
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _adminEmail;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@marblin.com";
            _senderName = _configuration["EmailSettings:SenderName"] ?? "Marblin";
            _adminEmail = _configuration["EmailSettings:AdminEmail"] ?? "";
            _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "";
            _smtpPort = int.TryParse(_configuration["EmailSettings:SmtpPort"], out var port) ? port : 587;
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrEmpty(_smtpHost))
            {
                _logger.LogWarning("SMTP Host is not configured. Email not sent.");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_senderName, _senderEmail));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Recipient}. Subject: {Subject}", to, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", to);
            }
        }

        public async Task SendContactFormEmailAsync(string customerName, string customerEmail, string message)
        {
            if (string.IsNullOrEmpty(_adminEmail))
            {
                _logger.LogWarning("Admin email is not configured. Contact form email not sent.");
                return;
            }

            var (subject, htmlBody) = EmailTemplateBuilder.BuildContactForm(customerName, customerEmail, message);
            await SendEmailAsync(_adminEmail, subject, htmlBody);
        }

        public async Task SendOrderConfirmationEmailAsync(string customerEmail, string customerName, string orderNumber, 
            decimal totalAmount, decimal depositAmount, string instapay, string vodafoneCash, string proofUrl, 
            Marblin.Core.Enums.PaymentMethod paymentMethod, string city, decimal cairoGizaShippingCost)
        {
            var (subject, htmlBody) = EmailTemplateBuilder.BuildOrderConfirmation(
                customerName, orderNumber, totalAmount, depositAmount, instapay, vodafoneCash,
                proofUrl, paymentMethod, city, cairoGizaShippingCost);
            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendPaymentProofReceivedEmailAsync(string customerEmail, string customerName, string orderNumber, 
            Marblin.Core.Enums.PaymentMethod paymentMethod)
        {
            var (subject, htmlBody) = EmailTemplateBuilder.BuildPaymentProofReceived(customerName, orderNumber, paymentMethod);
            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendDepositVerifiedEmailAsync(string customerEmail, string customerName, string orderNumber, 
            decimal totalAmount, decimal remainingBalance, Marblin.Core.Enums.PaymentMethod paymentMethod)
        {
            var (subject, htmlBody) = EmailTemplateBuilder.BuildDepositVerified(
                customerName, orderNumber, totalAmount, remainingBalance, paymentMethod);
            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendAwaitingBalanceEmailAsync(string customerEmail, string customerName, string orderNumber, 
            decimal totalAmount, decimal depositPaid, decimal remainingBalance, string instapay, string vodafoneCash, 
            Marblin.Core.Enums.PaymentMethod paymentMethod, string city, decimal cairoGizaShippingCost)
        {
            var (subject, htmlBody) = EmailTemplateBuilder.BuildAwaitingBalance(
                customerName, orderNumber, totalAmount, depositPaid, remainingBalance,
                instapay, vodafoneCash, paymentMethod, city, cairoGizaShippingCost);
            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendOrderShippedEmailAsync(string customerEmail, string customerName, string orderNumber)
        {
            var (subject, htmlBody) = EmailTemplateBuilder.BuildOrderShipped(customerName, orderNumber);
            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendOrderCancelledEmailAsync(string customerEmail, string customerName, string orderNumber, string reason)
        {
            var (subject, htmlBody) = EmailTemplateBuilder.BuildOrderCancelled(customerName, orderNumber, reason);
            await SendEmailAsync(customerEmail, subject, htmlBody);
        }
    }
}
