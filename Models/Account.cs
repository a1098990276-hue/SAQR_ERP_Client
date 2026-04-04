namespace SAQR_ERP_Client.Models
{
    /// <summary>
    /// نموذج الحساب المحاسبي
    /// </summary>
    public class Account
    {
        public int Id { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string AccountNameEn { get; set; } = string.Empty;
        public AccountType Type { get; set; }
        public int? ParentAccountId { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// أنواع الحسابات المحاسبية
    /// </summary>
    public enum AccountType
    {
        /// <summary>الأصول</summary>
        Assets = 1,
        /// <summary>الخصوم</summary>
        Liabilities = 2,
        /// <summary>حقوق الملكية</summary>
        Equity = 3,
        /// <summary>الإيرادات</summary>
        Revenue = 4,
        /// <summary>المصروفات</summary>
        Expenses = 5
    }
}
