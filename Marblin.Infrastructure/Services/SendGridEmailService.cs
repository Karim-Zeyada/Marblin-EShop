using Marblin.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Marblin.Infrastructure.Services
{
    /// <summary>
    /// SendGrid implementation of IEmailService for professional email delivery.
    /// </summary>
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SendGridEmailService> _logger;
        private readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _adminEmail;

        public SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            _apiKey = _configuration["EmailSettings:SendGridApiKey"] ?? "";
            _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@marblin.com";
            _senderName = _configuration["EmailSettings:SenderName"] ?? "Marblin";
            _adminEmail = _configuration["EmailSettings:AdminEmail"] ?? "";
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("SendGrid API Key is not configured. Email not sent.");
                return;
            }

            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_senderEmail, _senderName);
            var toAddress = new EmailAddress(to);
            var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, null, htmlBody);

            try
            {
                var response = await client.SendEmailAsync(msg);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {Recipient}. Subject: {Subject}", to, subject);
                }
                else
                {
                    var body = await response.Body.ReadAsStringAsync();
                    _logger.LogWarning("Email sending failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, body);
                }
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

            var subject = $"New Contact Form Message from {customerName}";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center;'>
                        <h1 style='margin: 0; font-family: Georgia, serif; letter-spacing: 2px;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff;'>
                        <h2 style='color: #1a1a1a; border-bottom: 1px solid #c9a962; padding-bottom: 10px;'>New Inquiry</h2>
                        <p><strong>From:</strong> {customerName}</p>
                        <p><strong>Email:</strong> <a href='mailto:{customerEmail}'>{customerEmail}</a></p>
                        <div style='background: #fdfaf4; padding: 20px; border-left: 4px solid #c9a962; margin-top: 20px;'>
                            <p style='margin: 0; font-style: italic;'>""{message}""</p>
                        </div>
                    </div>
                </div>";

            await SendEmailAsync(_adminEmail, subject, htmlBody);
        }

        public async Task SendOrderConfirmationEmailAsync(string customerEmail, string customerName, string orderNumber, 
            decimal totalAmount, decimal depositAmount, string instapay, string vodafoneCash, string proofUrl)
        {
            var subject = $"Order Confirmation - {orderNumber} | MARBLIN";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center; border-bottom: 3px solid #c9a962;'>
                        <h1 style='margin: 0; font-family: Georgia, serif; letter-spacing: 4px;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff;'>
                        <h2 style='color: #1a1a1a;'>Thank you, {customerName}.</h2>
                        <p>We have received your order. It is currently <strong>Pending Deposit</strong>.</p>
                        
                        <div style='background: #f9f9f9; padding: 20px; border: 1px solid #eee; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>Order Number:</strong> {orderNumber}</p>
                            <p style='margin: 10px 0 0;'><strong>Total Amount:</strong> {totalAmount:C2}</p>
                            <p style='margin: 5px 0 0; color: #c9a962;'><strong>Required Deposit:</strong> {depositAmount:C2}</p>
                        </div>

                        <h3 style='color: #1a1a1a; border-bottom: 1px solid #eee; padding-bottom: 5px;'>Payment Instructions</h3>
                        <p>Please use one of the following methods to pay your deposit:</p>
                        <p><strong>Instapay Account:</strong> {instapay}</p>
                        <p><strong>Vodafone Cash:</strong> {vodafoneCash}</p>

                        <div style='margin-top: 30px; text-align: center;'>
                            <a href='{proofUrl}' style='background: #1a1a1a; color: #fff; padding: 15px 25px; text-decoration: none; border-radius: 2px; font-weight: bold; border-bottom: 2px solid #c9a962;'>UPLOAD PAYMENT PROOF</a>
                        </div>

                        <p style='margin-top: 30px; font-size: 13px; color: #666;'>Our team will notify you once your payment is verified and production begins.</p>
                    </div>
                    <div style='background: #1a1a1a; color: #888; padding: 15px; text-align: center; font-size: 11px;'>
                        © MARBLIN - Luxury Marble & Stone
                    </div>
                </div>";

            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendPaymentProofReceivedEmailAsync(string customerEmail, string customerName, string orderNumber)
        {
            var subject = $"Payment Proof Received - {orderNumber} | MARBLIN";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center; border-bottom: 3px solid #c9a962;'>
                        <h1 style='margin: 0; font-family: Georgia, serif;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff;'>
                        <h2 style='color: #1a1a1a;'>Proof Received</h2>
                        <p>Dear {customerName},</p>
                        <p>We have received your payment proof for order <strong>{orderNumber}</strong>.</p>
                        <p>Our team is currently reviewing the details. You will receive another update once the deposit is verified and your piece moves into production.</p>
                        <p style='margin-top: 30px;'>Best regards,<br/>The Marblin Team</p>
                    </div>
                    <div style='background: #1a1a1a; color: #888; padding: 15px; text-align: center; font-size: 11px;'>
                        © MARBLIN - Luxury Marble & Stone
                    </div>
                </div>";

            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendDepositVerifiedEmailAsync(string customerEmail, string customerName, string orderNumber, 
            decimal totalAmount, decimal remainingBalance)
        {
            var subject = $"Deposit Verified - Order {orderNumber} in Production | MARBLIN";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center; border-bottom: 3px solid #c9a962;'>
                        <h1 style='margin: 0; font-family: Georgia, serif;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff;'>
                        <h2 style='color: #1a1a1a;'>Deposit Verified</h2>
                        <p>Dear {customerName},</p>
                        <p>Great news! Your deposit for order <strong>{orderNumber}</strong> has been verified. Your piece has officially moved into <strong>Production</strong>.</p>
                        
                        <div style='background: #fdfaf4; padding: 20px; border: 1px solid #eee; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>Total Amount:</strong> {totalAmount:C2}</p>
                            <p style='margin: 10px 0 0; color: #c9a962;'><strong>Remaining Balance:</strong> {remainingBalance:C2}</p>
                        </div>

                        <p>We will update you as soon as your order is ready for shipment.</p>
                        <p style='margin-top: 30px;'>Best regards,<br/>The Marblin Team</p>
                    </div>
                    <div style='background: #1a1a1a; color: #888; padding: 15px; text-align: center; font-size: 11px;'>
                        © MARBLIN - Luxury Marble & Stone
                    </div>
                </div>";

            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendAwaitingBalanceEmailAsync(string customerEmail, string customerName, string orderNumber, 
            decimal totalAmount, decimal depositPaid, decimal remainingBalance, string instapay, string vodafoneCash)
        {
            var subject = $"Order Ready - Final Payment Required {orderNumber} | MARBLIN";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center; border-bottom: 3px solid #c9a962;'>
                        <h1 style='margin: 0; font-family: Georgia, serif;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff;'>
                        <h2 style='color: #1a1a1a;'>Your order is ready.</h2>
                        <p>Dear {customerName},</p>
                        <p>We are pleased to inform you that your piece is complete and ready for shipment. We are now awaiting the remaining balance.</p>
                        
                        <div style='background: #f9f9f9; padding: 20px; border: 1px solid #eee; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>Total Amount:</strong> {totalAmount:C2}</p>
                            <p style='margin: 5px 0 0;'><strong>Deposit Paid:</strong> {depositPaid:C2}</p>
                            <p style='margin: 10px 0 0; color: #c9a962; font-size: 18px;'><strong>Balance Due: {remainingBalance:C2}</strong></p>
                        </div>

                        <h3 style='color: #1a1a1a; border-bottom: 1px solid #eee; padding-bottom: 5px;'>Payment Instructions</h3>
                        <p><strong>Instapay Account:</strong> {instapay}</p>
                        <p><strong>Vodafone Cash:</strong> {vodafoneCash}</p>

                        <div style='background: #fff8e1; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                            <p style='margin: 0; font-size: 13px;'><strong>Note:</strong> One of our team members will contact you shortly to verify everything and coordinate the shipment details.</p>
                        </div>

                        <p style='margin-top: 30px;'>Best regards,<br/>The Marblin Team</p>
                    </div>
                    <div style='background: #1a1a1a; color: #888; padding: 15px; text-align: center; font-size: 11px;'>
                        © MARBLIN - Luxury Marble & Stone
                    </div>
                </div>";

            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendOrderShippedEmailAsync(string customerEmail, string customerName, string orderNumber)
        {
            var subject = $"Your Order has Shipped! - {orderNumber} | MARBLIN";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center; border-bottom: 3px solid #c9a962;'>
                        <h1 style='margin: 0; font-family: Georgia, serif;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff; text-align: center;'>
                        <h2 style='color: #1a1a1a;'>It's on its way.</h2>
                        <p>Dear {customerName},</p>
                        <p>Your order <strong>{orderNumber}</strong> has been shipped and is heading to you.</p>
                        
                        <div style='margin: 40px 0;'>
                            <p style='font-size: 24px; color: #c9a962; font-family: Georgia, serif;'>Thank you for choosing MARBLIN.</p>
                            <p style='color: #666;'>We hope you enjoy your new piece as much as we enjoyed creating it for you.</p>
                        </div>

                        <p style='margin-top: 50px; border-top: 1px solid #eee; padding-top: 20px;'>Best regards,<br/>The Marblin Team</p>
                    </div>
                    <div style='background: #1a1a1a; color: #888; padding: 15px; text-align: center; font-size: 11px;'>
                        © MARBLIN - Luxury Marble & Stone
                    </div>
                </div>";

            await SendEmailAsync(customerEmail, subject, htmlBody);
        }
    }
}
