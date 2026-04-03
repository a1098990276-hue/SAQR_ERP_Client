namespace SAQR_ERP_Client.Models
{
    /// <summary>
    /// إعدادات النظام
    /// </summary>
    public class SystemSettings
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "الصقر لأنظمة المحاسبة";
        public string? CompanyNameEn { get; set; } = "Al-Saqr Accounting Systems";
        public string? CompanyLogo { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; } = "المملكة العربية السعودية";
        public string? Phone { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? TaxNumber { get; set; }
        public string? CommercialRegister { get; set; }
        public string Currency { get; set; } = "ريال";
        public string CurrencyCode { get; set; } = "SAR";
        public decimal DefaultTaxRate { get; set; } = 15;
        public int FiscalYearStart { get; set; } = 1; // شهر بداية السنة المالية
        public string DateFormat { get; set; } = "dd/MM/yyyy";
        public bool UseHijriDate { get; set; } = false;
        public DateTime? LastBackup { get; set; }
    }

    /// <summary>
    /// السنة المالية
    /// </summary>
    public class FiscalYear
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsClosed { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string? ClosedBy { get; set; }
    }
}
