namespace SAQR_ERP_Client.Models
{
    /// <summary>
    /// نموذج القيد المحاسبي
    /// </summary>
    public class Transaction
    {
        public int Id { get; set; }
        public string TransactionNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Draft;
        public string? Reference { get; set; }
        public int? CustomerId { get; set; }
        public int? SupplierId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        public List<TransactionLine> Lines { get; set; } = new();
    }

    /// <summary>
    /// سطر القيد المحاسبي
    /// </summary>
    public class TransactionLine
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public string? AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// نوع القيد
    /// </summary>
    public enum TransactionType
    {
        /// <summary>قيد يومية عادي</summary>
        JournalEntry = 1,
        /// <summary>سند قبض</summary>
        Receipt = 2,
        /// <summary>سند صرف</summary>
        Payment = 3,
        /// <summary>فاتورة مبيعات</summary>
        SalesInvoice = 4,
        /// <summary>فاتورة مشتريات</summary>
        PurchaseInvoice = 5
    }

    /// <summary>
    /// حالة القيد
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>مسودة</summary>
        Draft = 1,
        /// <summary>معتمد</summary>
        Approved = 2,
        /// <summary>مرحل</summary>
        Posted = 3,
        /// <summary>ملغي</summary>
        Cancelled = 4
    }
}
