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
        
        // Valid state transitions map
        private static readonly Dictionary<OrderStatus, OrderStatus[]> _validTransitions = new()
        {
            { OrderStatus.PendingPayment, new[] { OrderStatus.InProduction, OrderStatus.Cancelled } },
            { OrderStatus.InProduction, new[] { OrderStatus.AwaitingBalance, OrderStatus.Shipped, OrderStatus.Cancelled } },
            { OrderStatus.AwaitingBalance, new[] { OrderStatus.Shipped, OrderStatus.Cancelled } },
            { OrderStatus.Shipped, Array.Empty<OrderStatus>() },
            { OrderStatus.Cancelled, Array.Empty<OrderStatus>() }
        };

        public bool IsRefunded { get; private set; }
        public decimal RefundedAmount { get; private set; }
        public DateTime? RefundedAt { get; private set; }

        public void MarkAsRefunded(decimal amount)
        {
            if (IsRefunded)
            {
                throw new InvalidOperationException("Order is already refunded.");
            }
            
            IsRefunded = true;
            RefundedAmount = amount;
            RefundedAt = DateTime.UtcNow;
        }

        public void VerifyDeposit()
        {
            if (Status != OrderStatus.PendingPayment)
            {
                throw new InvalidOperationException(
                    $"Cannot verify deposit: order is '{Status}', expected '{OrderStatus.PendingPayment}'.");
            }

            IsDepositVerified = true;
            DepositVerifiedAt = DateTime.UtcNow;
            
            // Auto-verify balance for Full Payment to unify payment state
            if (PaymentMethod == PaymentMethod.FullPaymentUpfront)
            {
                IsBalanceVerified = true;
                BalanceVerifiedAt = DateTime.UtcNow;
            }

            Status = OrderStatus.InProduction;
            InProductionAt = DateTime.UtcNow;
        }

        public void SetPaymentProof(string proof, PaymentProofType type)
        {
             if (type == PaymentProofType.ReceiptImage)
             {
                 ReceiptImageUrl = proof;
                 TransactionId = null;
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
            if (!_validTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            {
                throw new InvalidOperationException(
                    $"Cannot transition from '{Status}' to '{newStatus}'.");
            }

            // Payment verification guards
            if (newStatus == OrderStatus.InProduction && !IsDepositVerified)
            {
                throw new InvalidOperationException(
                    "Cannot move to 'InProduction': payment has not been verified yet.");
            }

            if (newStatus == OrderStatus.Shipped && PaymentMethod == PaymentMethod.CashOnDelivery && !IsBalanceVerified)
            {
                throw new InvalidOperationException(
                    "Cannot mark as 'Shipped': balance payment has not been verified yet.");
            }

            Status = newStatus;
        }

        
        // Concurrency Token
        [System.ComponentModel.DataAnnotations.Timestamp]
        public byte[]? RowVersion { get; set; }

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
            if (Status != OrderStatus.AwaitingBalance)
            {
                throw new InvalidOperationException(
                    $"Cannot verify balance: order is '{Status}', expected '{OrderStatus.AwaitingBalance}'.");
            }

            IsBalanceVerified = true;
            BalanceVerifiedAt = DateTime.UtcNow;
            Status = OrderStatus.Shipped;
            ShippedAt = DateTime.UtcNow;
        }
    }
}
