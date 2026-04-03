using SAQR_ERP_Client.Data;
using SAQR_ERP_Client.Models;

namespace SAQR_ERP_Client.Forms
{
    /// <summary>
    /// لوحة المعلومات الرئيسية
    /// </summary>
    public class DashboardControl : UserControl
    {
        private readonly DatabaseService _db = DatabaseService.Instance;

        public DashboardControl()
        {
            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.FromArgb(245, 245, 250);
            this.Padding = new Padding(10);
        }

        private void LoadData()
        {
            var accounts = _db.GetAllAccounts();
            var customers = _db.GetAllCustomers();
            var suppliers = _db.GetAllSuppliers();
            var transactions = _db.GetAllTransactions();
            var invoices = _db.GetAllInvoices();

            // حساب الإحصائيات
            var totalCustomers = customers.Count;
            var totalSuppliers = suppliers.Count;
            var totalSalesInvoices = invoices.Count(i => i.Type == InvoiceType.Sales);
            var totalPurchaseInvoices = invoices.Count(i => i.Type == InvoiceType.Purchase);
            var totalSales = invoices.Where(i => i.Type == InvoiceType.Sales && i.Status != InvoiceStatus.Cancelled).Sum(i => i.Total);
            var totalPurchases = invoices.Where(i => i.Type == InvoiceType.Purchase && i.Status != InvoiceStatus.Cancelled).Sum(i => i.Total);
            var pendingReceivables = invoices.Where(i => i.Type == InvoiceType.Sales && i.RemainingAmount > 0).Sum(i => i.RemainingAmount);
            var pendingPayables = invoices.Where(i => i.Type == InvoiceType.Purchase && i.RemainingAmount > 0).Sum(i => i.RemainingAmount);

            // إنشاء بطاقات الإحصائيات
            var cardsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 150,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(5)
            };
            this.Controls.Add(cardsPanel);

            AddStatCard(cardsPanel, "👥", "العملاء", totalCustomers.ToString(), Color.FromArgb(52, 152, 219));
            AddStatCard(cardsPanel, "🏭", "الموردين", totalSuppliers.ToString(), Color.FromArgb(155, 89, 182));
            AddStatCard(cardsPanel, "💰", "إجمالي المبيعات", $"{totalSales:N2} ريال", Color.FromArgb(46, 204, 113));
            AddStatCard(cardsPanel, "📦", "إجمالي المشتريات", $"{totalPurchases:N2} ريال", Color.FromArgb(230, 126, 34));
            AddStatCard(cardsPanel, "📈", "المستحقات", $"{pendingReceivables:N2} ريال", Color.FromArgb(241, 196, 15));
            AddStatCard(cardsPanel, "📉", "المطلوبات", $"{pendingPayables:N2} ريال", Color.FromArgb(231, 76, 60));

            // لوحة المحتوى السفلية
            var bottomPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 20, 0, 0)
            };
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.Controls.Add(bottomPanel);

            // آخر الفواتير
            var recentInvoicesPanel = CreatePanel("آخر الفواتير", "🧾");
            bottomPanel.Controls.Add(recentInvoicesPanel, 1, 0);

            var invoicesGrid = CreateDataGrid();
            invoicesGrid.Columns.Add("InvoiceNumber", "رقم الفاتورة");
            invoicesGrid.Columns.Add("Date", "التاريخ");
            invoicesGrid.Columns.Add("Type", "النوع");
            invoicesGrid.Columns.Add("Total", "الإجمالي");
            invoicesGrid.Columns.Add("Status", "الحالة");

            foreach (var inv in invoices.Take(10))
            {
                var typeName = inv.Type == InvoiceType.Sales ? "مبيعات" : "مشتريات";
                var statusName = inv.Status switch
                {
                    InvoiceStatus.Draft => "مسودة",
                    InvoiceStatus.Approved => "معتمدة",
                    InvoiceStatus.PartiallyPaid => "مدفوعة جزئياً",
                    InvoiceStatus.FullyPaid => "مدفوعة",
                    InvoiceStatus.Cancelled => "ملغاة",
                    _ => ""
                };
                invoicesGrid.Rows.Add(inv.InvoiceNumber, inv.InvoiceDate.ToString("dd/MM/yyyy"), typeName, $"{inv.Total:N2}", statusName);
            }
            recentInvoicesPanel.Controls.Add(invoicesGrid);

            // آخر القيود
            var recentTransactionsPanel = CreatePanel("آخر القيود", "📝");
            bottomPanel.Controls.Add(recentTransactionsPanel, 0, 0);

            var transactionsGrid = CreateDataGrid();
            transactionsGrid.Columns.Add("Number", "رقم القيد");
            transactionsGrid.Columns.Add("Date", "التاريخ");
            transactionsGrid.Columns.Add("Description", "البيان");
            transactionsGrid.Columns.Add("Amount", "المبلغ");

            foreach (var trans in transactions.Take(10))
            {
                var amount = trans.Lines.Sum(l => l.Debit);
                transactionsGrid.Rows.Add(trans.TransactionNumber, trans.TransactionDate.ToString("dd/MM/yyyy"), trans.Description, $"{amount:N2}");
            }
            recentTransactionsPanel.Controls.Add(transactionsGrid);

            bottomPanel.BringToFront();
        }

        private void AddStatCard(FlowLayoutPanel panel, string icon, string title, string value, Color color)
        {
            var card = new Panel
            {
                Size = new Size(180, 130),
                Margin = new Padding(10),
                BackColor = Color.White
            };

            // شريط ملون علوي
            var colorBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 5,
                BackColor = color
            };
            card.Controls.Add(colorBar);

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 30F),
                Location = new Point(60, 15),
                AutoSize = true
            };
            card.Controls.Add(lblIcon);

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(10, 70),
                Size = new Size(160, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblValue);

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Gray,
                Location = new Point(10, 95),
                Size = new Size(160, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };
            card.Controls.Add(lblTitle);

            panel.Controls.Add(card);
        }

        private Panel CreatePanel(string title, string icon)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(10),
                Padding = new Padding(15)
            };

            var lblTitle = new Label
            {
                Text = $"{icon} {title}",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 114),
                Dock = DockStyle.Top,
                Height = 40
            };
            panel.Controls.Add(lblTitle);

            return panel;
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
                Font = new Font("Segoe UI", 9F),
                ColumnHeadersHeight = 35
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 250);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(30, 60, 114);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            grid.EnableHeadersVisualStyles = false;

            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 240, 255);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.RowsDefaultCellStyle.BackColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 255);

            return grid;
        }
    }
}
