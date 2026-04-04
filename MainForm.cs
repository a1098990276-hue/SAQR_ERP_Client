using SAQR_ERP_Client.Data;
using SAQR_ERP_Client.Forms;

namespace SAQR_ERP_Client
{
    public partial class MainForm : Form
    {
        private Panel panelSidebar = null!;
        private Panel panelMain = null!;
        private Panel panelHeader = null!;
        private Label lblTitle = null!;
        private Label lblSubtitle = null!;
        private Button? activeButton = null;

        public MainForm()
        {
            InitializeComponent();
            InitializeUI();
            
            // تهيئة قاعدة البيانات
            _ = DatabaseService.Instance;
        }

        private void InitializeUI()
        {
            // إعدادات النموذج الرئيسي
            this.Text = "الصقر لأنظمة المحاسبة";
            this.Size = new Size(1400, 900);
            this.MinimumSize = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.FromArgb(245, 245, 250);
            this.Font = new Font("Segoe UI", 10F);

            // إنشاء الشريط الجانبي
            CreateSidebar();

            // إنشاء الرأس
            CreateHeader();

            // إنشاء اللوحة الرئيسية
            CreateMainPanel();

            // عرض لوحة المعلومات
            ShowDashboard();
        }

        private void CreateSidebar()
        {
            panelSidebar = new Panel
            {
                Dock = DockStyle.Right,
                Width = 250,
                BackColor = Color.FromArgb(30, 60, 114)
            };
            this.Controls.Add(panelSidebar);

            // الشعار
            var panelLogo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.FromArgb(25, 50, 95)
            };
            panelSidebar.Controls.Add(panelLogo);

            var lblLogo = new Label
            {
                Text = "🦅",
                Font = new Font("Segoe UI", 40F),
                ForeColor = Color.Gold,
                Dock = DockStyle.Top,
                Height = 70,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelLogo.Controls.Add(lblLogo);

            var lblAppName = new Label
            {
                Text = "الصقر للمحاسبة",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.TopCenter
            };
            panelLogo.Controls.Add(lblAppName);

            // أزرار القائمة
            var menuPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(5, 20, 5, 10),
                AutoScroll = true
            };
            panelSidebar.Controls.Add(menuPanel);

            // إضافة أزرار القائمة
            AddMenuButton(menuPanel, "🏠", "لوحة المعلومات", ShowDashboard);
            AddMenuButton(menuPanel, "📊", "دليل الحسابات", ShowAccounts);
            AddMenuButton(menuPanel, "📝", "القيود اليومية", ShowTransactions);
            AddMenuButton(menuPanel, "👥", "العملاء", ShowCustomers);
            AddMenuButton(menuPanel, "🏭", "الموردين", ShowSuppliers);
            AddMenuButton(menuPanel, "🧾", "فواتير المبيعات", () => ShowInvoices(Models.InvoiceType.Sales));
            AddMenuButton(menuPanel, "📦", "فواتير المشتريات", () => ShowInvoices(Models.InvoiceType.Purchase));
            AddMenuButton(menuPanel, "📈", "التقارير", ShowReports);
            AddMenuButton(menuPanel, "⚙️", "الإعدادات", ShowSettings);

            // زر الخروج
            var btnExit = CreateMenuButton("🚪", "خروج");
            btnExit.Click += (s, e) => Application.Exit();
            menuPanel.Controls.Add(btnExit);
        }

        private void AddMenuButton(FlowLayoutPanel panel, string icon, string text, Action action)
        {
            var btn = CreateMenuButton(icon, text);
            btn.Click += (s, e) =>
            {
                SetActiveButton(btn);
                action();
            };
            panel.Controls.Add(btn);
        }

        private Button CreateMenuButton(string icon, string text)
        {
            var btn = new Button
            {
                Text = $"{icon}  {text}",
                Size = new Size(238, 50),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 15, 0),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 3, 0, 3)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 80, 140);

            return btn;
        }

        private void SetActiveButton(Button btn)
        {
            if (activeButton != null)
            {
                activeButton.BackColor = Color.Transparent;
            }
            activeButton = btn;
            activeButton.BackColor = Color.FromArgb(50, 100, 170);
        }

        private void CreateHeader()
        {
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                Padding = new Padding(20, 15, 20, 15)
            };
            this.Controls.Add(panelHeader);

            lblTitle = new Label
            {
                Text = "لوحة المعلومات",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 114),
                AutoSize = true,
                Location = new Point(20, 15)
            };
            panelHeader.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("ar-SA")),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(20, 50)
            };
            panelHeader.Controls.Add(lblSubtitle);

            // خط فاصل
            var separator = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 2,
                BackColor = Color.FromArgb(230, 230, 235)
            };
            panelHeader.Controls.Add(separator);
        }

        private void CreateMainPanel()
        {
            panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 245, 250),
                Padding = new Padding(20)
            };
            this.Controls.Add(panelMain);

            // ترتيب العناصر
            panelSidebar.BringToFront();
            panelHeader.BringToFront();
        }

        private void SetPageTitle(string title, string subtitle = "")
        {
            lblTitle.Text = title;
            lblSubtitle.Text = string.IsNullOrEmpty(subtitle) 
                ? DateTime.Now.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("ar-SA"))
                : subtitle;
        }

        private void ClearMainPanel()
        {
            panelMain.Controls.Clear();
        }

        private void ShowDashboard()
        {
            SetPageTitle("لوحة المعلومات");
            ClearMainPanel();

            var dashboard = new DashboardControl
            {
                Dock = DockStyle.Fill
            };
            panelMain.Controls.Add(dashboard);
        }

        private void ShowAccounts()
        {
            SetPageTitle("دليل الحسابات", "إدارة الحسابات المحاسبية");
            ClearMainPanel();

            var accounts = new AccountsControl
            {
                Dock = DockStyle.Fill
            };
            panelMain.Controls.Add(accounts);
        }

        private void ShowTransactions()
        {
            SetPageTitle("القيود اليومية", "إدارة القيود المحاسبية");
            ClearMainPanel();

            var transactions = new TransactionsControl
            {
                Dock = DockStyle.Fill
            };
            panelMain.Controls.Add(transactions);
        }

        private void ShowCustomers()
        {
            SetPageTitle("العملاء", "إدارة بيانات العملاء");
            ClearMainPanel();

            var customers = new CustomersControl
            {
                Dock = DockStyle.Fill
            };
            panelMain.Controls.Add(customers);
        }

        private void ShowSuppliers()
        {
            SetPageTitle("الموردين", "إدارة بيانات الموردين");
            ClearMainPanel();

            var suppliers = new SuppliersControl
            {
                Dock = DockStyle.Fill
            };
            panelMain.Controls.Add(suppliers);
        }

        private void ShowInvoices(Models.InvoiceType type)
        {
            var title = type == Models.InvoiceType.Sales ? "فواتير المبيعات" : "فواتير المشتريات";
            SetPageTitle(title, "إدارة الفواتير");
            ClearMainPanel();

            var invoices = new InvoicesControl(type)
            {
                Dock = DockStyle.Fill
            };
            panelMain.Controls.Add(invoices);
        }

        private void ShowReports()
        {
            SetPageTitle("التقارير المالية", "عرض وطباعة التقارير");
            ClearMainPanel();

            var reports = new ReportsControl
            {
                Dock = DockStyle.Fill
            };
            panelMain.Controls.Add(reports);
        }

        private void ShowSettings()
        {
            SetPageTitle("الإعدادات", "إعدادات النظام والشركة");
            ClearMainPanel();

            var settings = new SettingsControl
            {
                Dock = DockStyle.Fill
            };
            panelMain.Controls.Add(settings);
        }
    }
}
