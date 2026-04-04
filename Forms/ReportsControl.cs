using SAQR_ERP_Client.Data;
using SAQR_ERP_Client.Models;

namespace SAQR_ERP_Client.Forms
{
    /// <summary>
    /// التقارير المالية
    /// </summary>
    public class ReportsControl : UserControl
    {
        private readonly DatabaseService _db = DatabaseService.Instance;
        private Panel contentPanel = null!;
        private DateTimePicker dtpFrom = null!;
        private DateTimePicker dtpTo = null!;

        public ReportsControl()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.FromArgb(245, 245, 250);
            this.Padding = new Padding(10);

            // شريط الأدوات
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            this.Controls.Add(toolbar);

            // فلتر التاريخ
            toolbar.Controls.Add(new Label { Text = "من:", Location = new Point(20, 18), AutoSize = true });
            dtpFrom = new DateTimePicker { Location = new Point(50, 15), Width = 150, Value = new DateTime(DateTime.Now.Year, 1, 1) };
            toolbar.Controls.Add(dtpFrom);

            toolbar.Controls.Add(new Label { Text = "إلى:", Location = new Point(220, 18), AutoSize = true });
            dtpTo = new DateTimePicker { Location = new Point(250, 15), Width = 150, Value = DateTime.Now };
            toolbar.Controls.Add(dtpTo);

            // أزرار التقارير
            var btnTrialBalance = CreateButton("📊 ميزان المراجعة", Color.FromArgb(52, 152, 219));
            btnTrialBalance.Location = new Point(430, 10);
            btnTrialBalance.Click += (s, e) => ShowTrialBalance();
            toolbar.Controls.Add(btnTrialBalance);

            var btnIncomeStatement = CreateButton("📈 قائمة الدخل", Color.FromArgb(46, 204, 113));
            btnIncomeStatement.Location = new Point(590, 10);
            btnIncomeStatement.Click += (s, e) => ShowIncomeStatement();
            toolbar.Controls.Add(btnIncomeStatement);

            var btnCustomerStatement = CreateButton("👥 كشف عملاء", Color.FromArgb(155, 89, 182));
            btnCustomerStatement.Location = new Point(750, 10);
            btnCustomerStatement.Click += (s, e) => ShowCustomerStatement();
            toolbar.Controls.Add(btnCustomerStatement);

            var btnSupplierStatement = CreateButton("🏭 كشف موردين", Color.FromArgb(230, 126, 34));
            btnSupplierStatement.Location = new Point(910, 10);
            btnSupplierStatement.Click += (s, e) => ShowSupplierStatement();
            toolbar.Controls.Add(btnSupplierStatement);

            // منطقة المحتوى
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 10, 0, 0),
                Padding = new Padding(20)
            };
            this.Controls.Add(contentPanel);
            contentPanel.BringToFront();

            // عرض رسالة ترحيب
            ShowWelcome();
        }

        private void ShowWelcome()
        {
            contentPanel.Controls.Clear();

            var welcomeLabel = new Label
            {
                Text = "📊 التقارير المالية\n\nاختر التقرير المطلوب من الأزرار أعلاه",
                Font = new Font("Segoe UI", 16F),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            contentPanel.Controls.Add(welcomeLabel);
        }

        private void ShowTrialBalance()
        {
            contentPanel.Controls.Clear();

            var titleLabel = new Label
            {
                Text = $"📊 ميزان المراجعة\nمن {dtpFrom.Value:dd/MM/yyyy} إلى {dtpTo.Value:dd/MM/yyyy}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 114),
                Dock = DockStyle.Top,
                Height = 60
            };
            contentPanel.Controls.Add(titleLabel);

            var grid = CreateDataGrid();
            grid.Columns.Add("AccountCode", "رمز الحساب");
            grid.Columns.Add("AccountName", "اسم الحساب");
            grid.Columns.Add("Debit", "مدين");
            grid.Columns.Add("Credit", "دائن");
            grid.Columns.Add("Balance", "الرصيد");

            grid.Columns["AccountCode"]!.Width = 120;
            grid.Columns["Debit"]!.Width = 120;
            grid.Columns["Credit"]!.Width = 120;
            grid.Columns["Balance"]!.Width = 120;

            var trialBalance = _db.GetTrialBalance(dtpFrom.Value, dtpTo.Value);
            decimal totalDebit = 0, totalCredit = 0;

            foreach (var (account, debit, credit) in trialBalance)
            {
                var balance = debit - credit;
                grid.Rows.Add(
                    account.AccountCode,
                    account.AccountName,
                    $"{debit:N2}",
                    $"{credit:N2}",
                    $"{Math.Abs(balance):N2} {(balance >= 0 ? "مدين" : "دائن")}"
                );
                totalDebit += debit;
                totalCredit += credit;
            }

            // سطر الإجمالي
            var totalRow = grid.Rows.Add("", "الإجمالي", $"{totalDebit:N2}", $"{totalCredit:N2}", "");
            grid.Rows[totalRow].DefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grid.Rows[totalRow].DefaultCellStyle.BackColor = Color.FromArgb(230, 240, 255);

            contentPanel.Controls.Add(grid);
            grid.BringToFront();
        }

        private void ShowIncomeStatement()
        {
            contentPanel.Controls.Clear();

            var titleLabel = new Label
            {
                Text = $"📈 قائمة الدخل\nمن {dtpFrom.Value:dd/MM/yyyy} إلى {dtpTo.Value:dd/MM/yyyy}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 114),
                Dock = DockStyle.Top,
                Height = 60
            };
            contentPanel.Controls.Add(titleLabel);

            var (revenue, expenses, netIncome) = _db.GetIncomeStatement(dtpFrom.Value, dtpTo.Value);

            var reportPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(50)
            };
            contentPanel.Controls.Add(reportPanel);

            var y = 20;
            var lineHeight = 50;

            // الإيرادات
            var lblRevenueTitle = new Label
            {
                Text = "الإيرادات",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Location = new Point(20, y),
                AutoSize = true
            };
            reportPanel.Controls.Add(lblRevenueTitle);

            y += lineHeight;
            var lblRevenueValue = new Label
            {
                Text = $"{revenue:N2} ريال",
                Font = new Font("Segoe UI", 16F),
                Location = new Point(50, y),
                AutoSize = true
            };
            reportPanel.Controls.Add(lblRevenueValue);

            y += lineHeight + 20;
            // المصروفات
            var lblExpensesTitle = new Label
            {
                Text = "المصروفات",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(231, 76, 60),
                Location = new Point(20, y),
                AutoSize = true
            };
            reportPanel.Controls.Add(lblExpensesTitle);

            y += lineHeight;
            var lblExpensesValue = new Label
            {
                Text = $"{expenses:N2} ريال",
                Font = new Font("Segoe UI", 16F),
                Location = new Point(50, y),
                AutoSize = true
            };
            reportPanel.Controls.Add(lblExpensesValue);

            y += lineHeight + 20;
            // خط فاصل
            var separator = new Panel
            {
                Location = new Point(20, y),
                Size = new Size(400, 3),
                BackColor = Color.Gray
            };
            reportPanel.Controls.Add(separator);

            y += 20;
            // صافي الربح/الخسارة
            var lblNetTitle = new Label
            {
                Text = netIncome >= 0 ? "صافي الربح" : "صافي الخسارة",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = netIncome >= 0 ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60),
                Location = new Point(20, y),
                AutoSize = true
            };
            reportPanel.Controls.Add(lblNetTitle);

            y += lineHeight;
            var lblNetValue = new Label
            {
                Text = $"{Math.Abs(netIncome):N2} ريال",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = netIncome >= 0 ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60),
                Location = new Point(50, y),
                AutoSize = true
            };
            reportPanel.Controls.Add(lblNetValue);

            reportPanel.BringToFront();
        }

        private void ShowCustomerStatement()
        {
            contentPanel.Controls.Clear();

            var titleLabel = new Label
            {
                Text = "👥 كشف حساب العملاء",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 114),
                Dock = DockStyle.Top,
                Height = 50
            };
            contentPanel.Controls.Add(titleLabel);

            var grid = CreateDataGrid();
            grid.Columns.Add("CustomerCode", "كود العميل");
            grid.Columns.Add("Name", "اسم العميل");
            grid.Columns.Add("Phone", "الهاتف");
            grid.Columns.Add("TotalSales", "إجمالي المبيعات");
            grid.Columns.Add("TotalPaid", "المدفوع");
            grid.Columns.Add("Balance", "الرصيد");

            grid.Columns["CustomerCode"]!.Width = 100;
            grid.Columns["TotalSales"]!.Width = 130;
            grid.Columns["TotalPaid"]!.Width = 130;
            grid.Columns["Balance"]!.Width = 130;

            var customers = _db.GetAllCustomers();
            var invoices = _db.GetAllInvoices(InvoiceType.Sales);

            foreach (var customer in customers)
            {
                var customerInvoices = invoices.Where(i => i.CustomerId == customer.Id && i.Status != InvoiceStatus.Cancelled).ToList();
                var totalSales = customerInvoices.Sum(i => i.Total);
                var totalPaid = customerInvoices.Sum(i => i.PaidAmount);
                var balance = totalSales - totalPaid;

                grid.Rows.Add(
                    customer.CustomerCode,
                    customer.Name,
                    customer.Phone ?? customer.Mobile ?? "",
                    $"{totalSales:N2}",
                    $"{totalPaid:N2}",
                    $"{balance:N2}"
                );
            }

            contentPanel.Controls.Add(grid);
            grid.BringToFront();
        }

        private void ShowSupplierStatement()
        {
            contentPanel.Controls.Clear();

            var titleLabel = new Label
            {
                Text = "🏭 كشف حساب الموردين",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 114),
                Dock = DockStyle.Top,
                Height = 50
            };
            contentPanel.Controls.Add(titleLabel);

            var grid = CreateDataGrid();
            grid.Columns.Add("SupplierCode", "كود المورد");
            grid.Columns.Add("Name", "اسم المورد");
            grid.Columns.Add("Phone", "الهاتف");
            grid.Columns.Add("TotalPurchases", "إجمالي المشتريات");
            grid.Columns.Add("TotalPaid", "المدفوع");
            grid.Columns.Add("Balance", "الرصيد");

            grid.Columns["SupplierCode"]!.Width = 100;
            grid.Columns["TotalPurchases"]!.Width = 130;
            grid.Columns["TotalPaid"]!.Width = 130;
            grid.Columns["Balance"]!.Width = 130;

            var suppliers = _db.GetAllSuppliers();
            var invoices = _db.GetAllInvoices(InvoiceType.Purchase);

            foreach (var supplier in suppliers)
            {
                var supplierInvoices = invoices.Where(i => i.SupplierId == supplier.Id && i.Status != InvoiceStatus.Cancelled).ToList();
                var totalPurchases = supplierInvoices.Sum(i => i.Total);
                var totalPaid = supplierInvoices.Sum(i => i.PaidAmount);
                var balance = totalPurchases - totalPaid;

                grid.Rows.Add(
                    supplier.SupplierCode,
                    supplier.Name,
                    supplier.Phone ?? supplier.Mobile ?? "",
                    $"{totalPurchases:N2}",
                    $"{totalPaid:N2}",
                    $"{balance:N2}"
                );
            }

            contentPanel.Controls.Add(grid);
            grid.BringToFront();
        }

        private Button CreateButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Size = new Size(150, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
        }

        private DataGridView CreateDataGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 10F),
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 35 }
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 60, 114);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;

            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.RowsDefaultCellStyle.BackColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 255);

            return grid;
        }
    }
}
