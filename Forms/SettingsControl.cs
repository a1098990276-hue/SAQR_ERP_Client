using SAQR_ERP_Client.Data;
using SAQR_ERP_Client.Models;

namespace SAQR_ERP_Client.Forms
{
    /// <summary>
    /// إعدادات النظام
    /// </summary>
    public class SettingsControl : UserControl
    {
        private readonly DatabaseService _db = DatabaseService.Instance;
        private SystemSettings _settings = null!;

        private TextBox txtCompanyName = null!;
        private TextBox txtCompanyNameEn = null!;
        private TextBox txtAddress = null!;
        private TextBox txtCity = null!;
        private TextBox txtCountry = null!;
        private TextBox txtPhone = null!;
        private TextBox txtMobile = null!;
        private TextBox txtEmail = null!;
        private TextBox txtWebsite = null!;
        private TextBox txtTaxNumber = null!;
        private TextBox txtCommercialRegister = null!;
        private TextBox txtCurrency = null!;
        private TextBox txtCurrencyCode = null!;
        private NumericUpDown nudDefaultTaxRate = null!;

        public SettingsControl()
        {
            _settings = _db.GetSettings();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.FromArgb(245, 245, 250);
            this.Padding = new Padding(10);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30),
                AutoScroll = true
            };
            this.Controls.Add(mainPanel);

            var titleLabel = new Label
            {
                Text = "⚙️ إعدادات الشركة",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 114),
                Dock = DockStyle.Top,
                Height = 50
            };
            mainPanel.Controls.Add(titleLabel);

            var formPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 12,
                Padding = new Padding(0, 20, 0, 0)
            };
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            formPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            mainPanel.Controls.Add(formPanel);

            // الصف الأول - اسم الشركة
            formPanel.Controls.Add(new Label { Text = "اسم الشركة:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 0, 0);
            txtCompanyName = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtCompanyName.Text = _settings.CompanyName;
            formPanel.Controls.Add(txtCompanyName, 1, 0);

            formPanel.Controls.Add(new Label { Text = "الاسم بالإنجليزي:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 2, 0);
            txtCompanyNameEn = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtCompanyNameEn.Text = _settings.CompanyNameEn;
            formPanel.Controls.Add(txtCompanyNameEn, 3, 0);

            // الصف الثاني - العنوان
            formPanel.Controls.Add(new Label { Text = "العنوان:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 0, 1);
            txtAddress = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtAddress.Text = _settings.Address;
            formPanel.SetColumnSpan(txtAddress, 3);
            formPanel.Controls.Add(txtAddress, 1, 1);

            // الصف الثالث - المدينة والدولة
            formPanel.Controls.Add(new Label { Text = "المدينة:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 0, 2);
            txtCity = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtCity.Text = _settings.City;
            formPanel.Controls.Add(txtCity, 1, 2);

            formPanel.Controls.Add(new Label { Text = "الدولة:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 2, 2);
            txtCountry = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtCountry.Text = _settings.Country;
            formPanel.Controls.Add(txtCountry, 3, 2);

            // الصف الرابع - الهاتف والجوال
            formPanel.Controls.Add(new Label { Text = "الهاتف:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 0, 3);
            txtPhone = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtPhone.Text = _settings.Phone;
            formPanel.Controls.Add(txtPhone, 1, 3);

            formPanel.Controls.Add(new Label { Text = "الجوال:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 2, 3);
            txtMobile = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtMobile.Text = _settings.Mobile;
            formPanel.Controls.Add(txtMobile, 3, 3);

            // الصف الخامس - البريد والموقع
            formPanel.Controls.Add(new Label { Text = "البريد الإلكتروني:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 0, 4);
            txtEmail = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtEmail.Text = _settings.Email;
            formPanel.Controls.Add(txtEmail, 1, 4);

            formPanel.Controls.Add(new Label { Text = "الموقع الإلكتروني:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 2, 4);
            txtWebsite = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtWebsite.Text = _settings.Website;
            formPanel.Controls.Add(txtWebsite, 3, 4);

            // الصف السادس - الرقم الضريبي والسجل التجاري
            formPanel.Controls.Add(new Label { Text = "الرقم الضريبي:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 0, 5);
            txtTaxNumber = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtTaxNumber.Text = _settings.TaxNumber;
            formPanel.Controls.Add(txtTaxNumber, 1, 5);

            formPanel.Controls.Add(new Label { Text = "السجل التجاري:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 2, 5);
            txtCommercialRegister = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtCommercialRegister.Text = _settings.CommercialRegister;
            formPanel.Controls.Add(txtCommercialRegister, 3, 5);

            // الصف السابع - العملة
            formPanel.Controls.Add(new Label { Text = "العملة:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 0, 6);
            txtCurrency = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtCurrency.Text = _settings.Currency;
            formPanel.Controls.Add(txtCurrency, 1, 6);

            formPanel.Controls.Add(new Label { Text = "رمز العملة:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 2, 6);
            txtCurrencyCode = new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F) };
            txtCurrencyCode.Text = _settings.CurrencyCode;
            formPanel.Controls.Add(txtCurrencyCode, 3, 6);

            // الصف الثامن - نسبة الضريبة
            formPanel.Controls.Add(new Label { Text = "نسبة الضريبة الافتراضية %:", Anchor = AnchorStyles.Right, Font = new Font("Segoe UI", 11F) }, 0, 7);
            nudDefaultTaxRate = new NumericUpDown { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11F), Maximum = 100, DecimalPlaces = 2 };
            nudDefaultTaxRate.Value = _settings.DefaultTaxRate;
            formPanel.Controls.Add(nudDefaultTaxRate, 1, 7);

            // فاصل
            var separator = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 2,
                BackColor = Color.FromArgb(230, 230, 235)
            };
            formPanel.SetColumnSpan(separator, 4);
            formPanel.Controls.Add(separator, 0, 8);

            // معلومات النظام
            var infoPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            formPanel.SetColumnSpan(infoPanel, 4);
            formPanel.Controls.Add(infoPanel, 0, 9);

            var lblSystemInfo = new Label
            {
                Text = "معلومات النظام",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 114),
                Location = new Point(0, 10),
                AutoSize = true
            };
            infoPanel.Controls.Add(lblSystemInfo);

            var lblVersion = new Label
            {
                Text = "الإصدار: 1.0.0",
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, 45),
                AutoSize = true
            };
            infoPanel.Controls.Add(lblVersion);

            var lblDeveloper = new Label
            {
                Text = "تطوير: الصقر لأنظمة المحاسبة",
                Font = new Font("Segoe UI", 11F),
                Location = new Point(200, 45),
                AutoSize = true
            };
            infoPanel.Controls.Add(lblDeveloper);

            // أزرار
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill
            };
            formPanel.SetColumnSpan(buttonPanel, 4);
            formPanel.Controls.Add(buttonPanel, 0, 10);

            var btnSave = new Button
            {
                Text = "💾 حفظ الإعدادات",
                Size = new Size(160, 50),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F)
            };
            btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(btnSave);

            var btnBackup = new Button
            {
                Text = "💿 نسخ احتياطي",
                Size = new Size(160, 50),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F),
                Margin = new Padding(20, 0, 0, 0)
            };
            btnBackup.Click += BtnBackup_Click;
            buttonPanel.Controls.Add(btnBackup);

            formPanel.BringToFront();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                MessageBox.Show("الرجاء إدخال اسم الشركة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            _settings.CompanyName = txtCompanyName.Text.Trim();
            _settings.CompanyNameEn = string.IsNullOrWhiteSpace(txtCompanyNameEn.Text) ? null : txtCompanyNameEn.Text.Trim();
            _settings.Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim();
            _settings.City = string.IsNullOrWhiteSpace(txtCity.Text) ? null : txtCity.Text.Trim();
            _settings.Country = string.IsNullOrWhiteSpace(txtCountry.Text) ? null : txtCountry.Text.Trim();
            _settings.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
            _settings.Mobile = string.IsNullOrWhiteSpace(txtMobile.Text) ? null : txtMobile.Text.Trim();
            _settings.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
            _settings.Website = string.IsNullOrWhiteSpace(txtWebsite.Text) ? null : txtWebsite.Text.Trim();
            _settings.TaxNumber = string.IsNullOrWhiteSpace(txtTaxNumber.Text) ? null : txtTaxNumber.Text.Trim();
            _settings.CommercialRegister = string.IsNullOrWhiteSpace(txtCommercialRegister.Text) ? null : txtCommercialRegister.Text.Trim();
            _settings.Currency = txtCurrency.Text.Trim();
            _settings.CurrencyCode = txtCurrencyCode.Text.Trim();
            _settings.DefaultTaxRate = nudDefaultTaxRate.Value;

            _db.SaveSettings(_settings);

            MessageBox.Show("تم حفظ الإعدادات بنجاح", "نجاح",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
        }

        private void BtnBackup_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Title = "حفظ نسخة احتياطية",
                Filter = "ملف قاعدة البيانات|*.db",
                FileName = $"SaqrAccounting_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaqrAccounting.db");
                    File.Copy(sourcePath, dialog.FileName, true);

                    _settings.LastBackup = DateTime.Now;
                    _db.SaveSettings(_settings);

                    MessageBox.Show("تم إنشاء النسخة الاحتياطية بنجاح", "نجاح",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء إنشاء النسخة الاحتياطية: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                }
            }
        }
    }
}
