namespace SAQR_ERP_Client.Models
{
    /// <summary>
    /// نموذج الفاتورة
    /// </summary>
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public InvoiceType Type { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercent { get; set; } = 15; // ضريبة القيمة المضافة
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount => Total - PaidAmount;
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        public string? Notes { get; set; }
        public int? TransactionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<InvoiceLine> Lines { get; set; } = new();
    }

    /// <summary>
    /// سطر الفاتورة
    /// </summary>
    public class InvoiceLine
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int LineNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ItemCode { get; set; }
        public decimal Quantity { get; set; } = 1;
        public string Unit { get; set; } = "وحدة";
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercent { get; set; } = 15;
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public int? AccountId { get; set; }
    }

    /// <summary>
    /// نوع الفاتورة
    /// </summary>
    public enum InvoiceType
    {
        /// <summary>فاتورة مبيعات</summary>
        Sales = 1,
        /// <summary>فاتورة مشتريات</summary>
        Purchase = 2,
        /// <summary>مرتجع مبيعات</summary>
        SalesReturn = 3,
        /// <summary>مرتجع مشتريات</summary>
        PurchaseReturn = 4
    }

    /// <summary>
    /// حالة الفاتورة
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>مسودة</summary>
        Draft = 1,
        /// <summary>معتمدة</summary>
        Approved = 2,
        /// <summary>مدفوعة جزئياً</summary>
        PartiallyPaid = 3,
        /// <summary>مدفوعة بالكامل</summary>
        FullyPaid = 4,
        /// <summary>ملغاة</summary>
        Cancelled = 5
    }
}
