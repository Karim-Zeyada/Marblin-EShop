using Marblin.Core.Enums;

namespace Marblin.Core.Entities
{
    /// <summary>
    /// Customer order with guest checkout and deposit-based payment.
    /// </summary>
    public class Order
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Public-facing order number for tracking (e.g., "ORD-2025-0001").
        /// </summary>
        public string OrderNumber { get; set; } = string.Empty;
        
        // Customer Information (Guest Checkout)
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        
        // Shipping Address
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        
        // Order Totals
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Deposit percentage at time of order (captured for historical accuracy).
        /// </summary>
        public decimal DepositPercentage { get; set; }
        
        /// <summary>
        /// Calculated deposit amount.
        /// </summary>
        public decimal DepositAmount { get; set; }
        
        /// <summary>
        /// Remaining balance after deposit.
        /// </summary>
        public decimal RemainingBalance => TotalAmount - DepositAmount;
        
        // Discount
        public string? DiscountCode { get; set; }
        public decimal DiscountAmount { get; set; }
        
        // Payment Method
        /// <summary>
        /// Payment method chosen by customer: COD (deposit only) or Full Payment upfront.
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
        
        /// <summary>
        /// Amount due based on payment method: DepositAmount for COD, TotalAmount for Full Payment.
        /// </summary>
        public decimal AmountDue => PaymentMethod == PaymentMethod.FullPaymentUpfront 
            ? TotalAmount 
            : DepositAmount;
        
        // Status & Workflow
        public OrderStatus Status { get; private set; } = OrderStatus.PendingPayment;
        
        // Payment Proof
        public PaymentProofType PaymentProofType { get; private set; } = PaymentProofType.None;
        public string? TransactionId { get; private set; }
        public string? ReceiptImageUrl { get; private set; }
        public bool IsDepositVerified { get; private set; }
        public DateTime? PaymentProofSubmittedAt { get; private set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DepositVerifiedAt { get; private set; }
        public DateTime? InProductionAt { get; set; }
        public DateTime? AwaitingBalanceAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        
        public void VerifyDeposit()
        {
            IsDepositVerified = true;
            DepositVerifiedAt = DateTime.UtcNow;
            
            if (Status == OrderStatus.PendingPayment)
            {
                Status = OrderStatus.InProduction;
                InProductionAt = DateTime.UtcNow;
            }
        }

        public void SetPaymentProof(string proof, PaymentProofType type)
        {
             if (type == PaymentProofType.ReceiptImage)
             {
                 ReceiptImageUrl = proof;
                 TransactionId = null; // Clear mutually exclusive field if needed, or keep both? logic implies one.
             }
             else if (type == PaymentProofType.TransactionId)
             {
                 TransactionId = proof;
                 ReceiptImageUrl = null;
             }
             
             PaymentProofType = type;
             PaymentProofSubmittedAt = DateTime.UtcNow;
        }

        public void UpdateStatus(OrderStatus newStatus)
        {
            Status = newStatus;
        }

        
        // Navigation
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Balance Payment Proof
        public PaymentProofType BalancePaymentProofType { get; private set; } = PaymentProofType.None;
        public string? BalanceTransactionId { get; private set; }
        public string? BalanceReceiptImageUrl { get; private set; }
        public bool IsBalanceVerified { get; private set; }
        public DateTime? BalancePaymentProofSubmittedAt { get; private set; }
        public DateTime? BalanceVerifiedAt { get; private set; }

        public void SetBalancePaymentProof(string proof, PaymentProofType type)
        {
             if (type == PaymentProofType.ReceiptImage)
             {
                 BalanceReceiptImageUrl = proof;
                 BalanceTransactionId = null; 
             }
             else if (type == PaymentProofType.TransactionId)
             {
                 BalanceTransactionId = proof;
                 BalanceReceiptImageUrl = null;
             }
             
             BalancePaymentProofType = type;
             BalancePaymentProofSubmittedAt = DateTime.UtcNow;
        }

        public void VerifyBalance()
        {
            IsBalanceVerified = true;
            BalanceVerifiedAt = DateTime.UtcNow;
            
            if (Status == OrderStatus.AwaitingBalance)
            {
                // Assuming next step is generic 'Processing' or kept as Shipped manually later?
                // For now, let's just mark verified. Admins usually move to Shipped.
                // Or we could have an optional status 'ReadyToShip'.
                // Keeping status as is, or maybe just purely tracking the verification flag.
            }
        }
    }
}
