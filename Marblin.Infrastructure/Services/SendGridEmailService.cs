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
            decimal totalAmount, decimal depositAmount, string instapay, string vodafoneCash, string proofUrl, 
            Marblin.Core.Enums.PaymentMethod paymentMethod, string city, decimal cairoGizaShippingCost)
        {
            var subject = $"Order Confirmation - {orderNumber} | MARBLIN";
            
            // Conditional content based on payment method
            var amountLabel = paymentMethod == Marblin.Core.Enums.PaymentMethod.FullPaymentUpfront 
                ? "Total Order Amount" 
                : "Required Deposit";
            var amountValue = paymentMethod == Marblin.Core.Enums.PaymentMethod.FullPaymentUpfront 
                ? totalAmount 
                : depositAmount;
            var amountColor = paymentMethod == Marblin.Core.Enums.PaymentMethod.FullPaymentUpfront 
                ? "#28a745" 
                : "#c9a962";
            var paymentType = paymentMethod == Marblin.Core.Enums.PaymentMethod.FullPaymentUpfront 
                ? "full payment for your order" 
                : "deposit";
            
            // Shipping information note - show for both payment methods
            var isCairoGiza = city.Equals("Cairo", StringComparison.OrdinalIgnoreCase) || 
                              city.Equals("Giza", StringComparison.OrdinalIgnoreCase);
            var shippingCostInfo = isCairoGiza && cairoGizaShippingCost > 0
                ? $"Shipping to <strong>{city}</strong> is <strong>{cairoGizaShippingCost:C2}</strong>"
                : "Shipping cost will be calculated based on your location";
            
            var shippingMessage = paymentMethod == Marblin.Core.Enums.PaymentMethod.FullPaymentUpfront
                ? $"This amount covers your <strong>order only</strong>. {shippingCostInfo} and will be collected upon delivery."
                : $"This deposit covers part of your order. The remaining balance and shipping ({shippingCostInfo}) will be collected upon delivery.";
                
            var shippingNote = $@"
                <div style='background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
                    <p style='margin: 0; color: #856404; font-size: 14px;'>
                        <strong>📦 Shipping Information:</strong><br/>
                        {shippingMessage}
                    </p>
                </div>";

            
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center; border-bottom: 3px solid #c9a962;'>
                        <h1 style='margin: 0; font-family: Georgia, serif; letter-spacing: 4px;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff;'>
                        <h2 style='color: #1a1a1a;'>Thank you, {customerName}.</h2>
                        <p>We have received your order. It is currently <strong>Pending Payment</strong>.</p>
                        
                        <div style='background: #f9f9f9; padding: 20px; border: 1px solid #eee; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>Order Number:</strong> {orderNumber}</p>
                            <p style='margin: 10px 0 0;'><strong>Total Order Amount:</strong> {totalAmount:C2}</p>
                            <p style='margin: 5px 0 0; color: {amountColor};'><strong>{amountLabel} (to pay now):</strong> {amountValue:C2}</p>
                        </div>
                        
                        {shippingNote}

                        <h3 style='color: #1a1a1a; border-bottom: 1px solid #eee; padding-bottom: 5px;'>Payment Instructions</h3>
                        <p>Please use one of the following methods to pay your {paymentType}:</p>
                        <p><strong>Instapay Account:</strong> {instapay}</p>
                        <p><strong>Vodafone Cash:</strong> {vodafoneCash}</p>

                        <div style='margin-top: 30px; text-align: center;'>
                            <a href='{proofUrl}' style='background: #1a1a1a; color: #fff; padding: 15px 25px; text-decoration: none; border-radius: 2px; font-weight: bold; border-bottom: 2px solid #c9a962;'>UPLOAD PAYMENT PROOF</a>
                        </div>

                        <div style='background: #e3f2fd; border-left: 4px solid #2196f3; padding: 15px; margin: 30px 0;'>
                            <p style='margin: 0; font-size: 13px; color: #1565c0;'>
                                <strong>📋 Cancellation Policy:</strong> You may cancel your order within 2 days of placing it by contacting us via email or WhatsApp.
                            </p>
                        </div>

                        <p style='margin-top: 30px; font-size: 13px; color: #666;'>Our team will notify you once your payment is verified and production begins.</p>
                    </div>
                    <div style='background: #1a1a1a; color: #888; padding: 15px; text-align: center; font-size: 11px;'>
                        © MARBLIN - Luxury Marble & Stone
                    </div>
                </div>";

            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendPaymentProofReceivedEmailAsync(string customerEmail, string customerName, string orderNumber, 
            Marblin.Core.Enums.PaymentMethod paymentMethod)
        {
            var subject = $"Payment Proof Received - {orderNumber} | MARBLIN";
            var paymentType = paymentMethod == Marblin.Core.Enums.PaymentMethod.FullPaymentUpfront 
                ? "full payment" 
                : "deposit";
            
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center; border-bottom: 3px solid #c9a962;'>
                        <h1 style='margin: 0; font-family: Georgia, serif;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff;'>
                        <h2 style='color: #1a1a1a;'>Proof Received</h2>
                        <p>Dear {customerName},</p>
                        <p>We have received your payment proof for the <strong>{paymentType}</strong> on order <strong>{orderNumber}</strong>.</p>
                        <p>Our team is currently reviewing the details. You will receive another update once the payment is verified and your piece moves into production.</p>
                        <p style='margin-top: 30px;'>Best regards,<br/>The Marblin Team</p>
                    </div>
                    <div style='background: #1a1a1a; color: #888; padding: 15px; text-align: center; font-size: 11px;'>
                        © MARBLIN - Luxury Marble & Stone
                    </div>
                </div>";

            await SendEmailAsync(customerEmail, subject, htmlBody);
        }

        public async Task SendDepositVerifiedEmailAsync(string customerEmail, string customerName, string orderNumber, 
            decimal totalAmount, decimal remainingBalance, Marblin.Core.Enums.PaymentMethod paymentMethod)
        {
            var subject = $"Payment Verified - Order {orderNumber} in Production | MARBLIN";
            
            // Conditional content for remaining balance section
            var balanceSection = "";
            if (paymentMethod == Marblin.Core.Enums.PaymentMethod.CashOnDelivery)
            {
                balanceSection = $@"
                    <div style='background: #fdfaf4; padding: 20px; border: 1px solid #eee; margin: 20px 0;'>
                        <p style='margin: 0;'><strong>Total Amount:</strong> {totalAmount:C2}</p>
                        <p style='margin: 10px 0 0; color: #c9a962;'><strong>Remaining Balance:</strong> {remainingBalance:C2}</p>
                    </div>";
            }
            else
            {
                balanceSection = $@"
                    <div style='background: #d4edda; padding: 20px; border-left: 4px solid #28a745; margin: 20px 0;'>
                        <p style='margin: 0; color: #155724;'><strong>✓ Payment Completed in Full</strong></p>
                        <p style='margin: 10px 0 0; color: #155724;'>No balance due. Your order is fully paid!</p>
                    </div>";
            }
            
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center; border-bottom: 3px solid #c9a962;'>
                        <h1 style='margin: 0; font-family: Georgia, serif;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff;'>
                        <h2 style='color: #1a1a1a;'>Payment Verified</h2>
                        <p>Dear {customerName},</p>
                        <p>Great news! Your payment for order <strong>{orderNumber}</strong> has been verified. Your piece has officially moved into <strong>Production</strong>.</p>
                        
                        {balanceSection}

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
            decimal totalAmount, decimal depositPaid, decimal remainingBalance, string instapay, string vodafoneCash, 
            Marblin.Core.Enums.PaymentMethod paymentMethod, string city, decimal cairoGizaShippingCost)
        {
            // Conditional content based on payment method
            var isCOD = paymentMethod == Marblin.Core.Enums.PaymentMethod.CashOnDelivery;
            var subject = isCOD 
                ? $"Order Ready - Final Payment Required {orderNumber} | MARBLIN"
                : $"Order Ready for Delivery {orderNumber} | MARBLIN";
            
            var heading = isCOD ? "Your order is ready." : "Your order is ready for delivery!";
            var message = isCOD 
                ? "We are pleased to inform you that your piece is complete and ready for shipment. We are awaiting the remaining balance."
                : "We are pleased to inform you that your piece is complete and ready for shipment!";
            
            // Shipping cost information
            var isCairoGiza = city.Equals("Cairo", StringComparison.OrdinalIgnoreCase) || 
                              city.Equals("Giza", StringComparison.OrdinalIgnoreCase);
            var shippingInfo = isCairoGiza && cairoGizaShippingCost > 0
                ? $"Shipping to <strong>{city}</strong> is <strong>{cairoGizaShippingCost:C2}</strong>"
                : "Shipping cost will be calculated based on your location";
            
            // Payment details section
            var paymentDetails = isCOD ? $@"
                <div style='background: #f9f9f9; padding: 20px; border: 1px solid #eee; margin: 20px 0;'>
                    <p style='margin: 0;'><strong>Total Order Amount:</strong> {totalAmount:C2}</p>
                    <p style='margin: 5px 0 0;'><strong>Deposit Paid:</strong> {depositPaid:C2}</p>
                    <p style='margin: 10px 0 0; color: #c9a962; font-size: 18px;'><strong>Balance Due: {remainingBalance:C2}</strong></p>
                </div>

                <h3 style='color: #1a1a1a; border-bottom: 1px solid #eee; padding-bottom: 5px;'>Payment Instructions</h3>
                <p><strong>Instapay Account:</strong> {instapay}</p>
                <p><strong>Vodafone Cash:</strong> {vodafoneCash}</p>
                
                <div style='background: #fff8e1; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                    <p style='margin: 0; font-size: 13px;'><strong>Note:</strong> The remaining balance and shipping cost will be collected upon delivery. One of our team members will contact you to coordinate delivery details.</p>
                </div>" : $@"
                <div style='background: #e8f5e9; padding: 20px; border: 1px solid #4caf50; border-left: 4px solid #4caf50; margin: 20px 0;'>
                    <p style='margin: 0;'><strong>Total Order Amount:</strong> {totalAmount:C2}</p>
                    <p style='margin: 10px 0 0; color: #2e7d32; font-size: 18px;'><strong>✅ Fully Paid</strong></p>
                </div>
                
                <div style='background: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                    <p style='margin: 0; font-size: 13px;'>
                        <strong>📦 Shipping:</strong> {shippingInfo} and will be collected upon delivery.
                    </p>
                </div>
                
                <div style='background: #fff8e1; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                    <p style='margin: 0; font-size: 13px;'><strong>Note:</strong> One of our team members will contact you shortly to coordinate the delivery details.</p>
                </div>";
            
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center; border-bottom: 3px solid #c9a962;'>
                        <h1 style='margin: 0; font-family: Georgia, serif;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff;'>
                        <h2 style='color: #1a1a1a;'>{heading}</h2>
                        <p>Dear {customerName},</p>
                        <p>{message}</p>
                        
                        {paymentDetails}

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

        public async Task SendOrderCancelledEmailAsync(string customerEmail, string customerName, string orderNumber, string reason)
        {
            var subject = $"Order Cancelled - {orderNumber} | MARBLIN";
            
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #eee;'>
                    <div style='background: #1a1a1a; color: #fff; padding: 25px; text-align: center; border-bottom: 3px solid #c9a962;'>
                        <h1 style='margin: 0; font-family: Georgia, serif;'>MARBLIN</h1>
                    </div>
                    <div style='padding: 30px; background: #fff;'>
                        <h2 style='color: #1a1a1a;'>Order Cancelled</h2>
                        <p>Dear {customerName},</p>
                        <p>Your order <strong>{orderNumber}</strong> has been cancelled as requested.</p>
                        
                        <div style='background: #f9f9f9; padding: 20px; border: 1px solid #eee; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>Cancellation Reason:</strong></p>
                            <p style='margin: 10px 0 0;'>{reason}</p>
                        </div>

                        <div style='background: #fff3cd; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0;'>
                            <p style='margin: 0; font-size: 13px;'><strong>Refund Information:</strong> If you have already paid a deposit, we will process your refund within 3-5 business days.</p>
                        </div>

                        <p style='margin-top: 30px;'>If you have any questions or would like to place a new order, please don't hesitate to contact us.</p>
                        
                        <p style='margin-top: 30px;'>Best regards,<br/>The Marblin Team</p>
                    </div>
                    <div style='background: #1a1a1a; color: #888; padding: 15px; text-align: center; font-size: 11px;'>
                        © MARBLIN - Luxury Marble & Stone
                    </div>
                </div>";

            await SendEmailAsync(customerEmail, subject, htmlBody);
        }
    }
}
