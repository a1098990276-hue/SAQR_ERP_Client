using SAQR_ERP_Client.Data;
using SAQR_ERP_Client.Models;

namespace SAQR_ERP_Client.Forms
{
    /// <summary>
    /// إدارة دليل الحسابات
    /// </summary>
    public class AccountsControl : UserControl
    {
        private readonly DatabaseService _db = DatabaseService.Instance;
        private DataGridView dgvAccounts = null!;
        private List<Account> _accounts = new();

        public AccountsControl()
        {
            InitializeUI();
            LoadData();
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

            var btnAdd = CreateButton("➕ إضافة حساب", Color.FromArgb(46, 204, 113));
            btnAdd.Click += BtnAdd_Click;
            toolbar.Controls.Add(btnAdd);

            var btnEdit = CreateButton("✏️ تعديل", Color.FromArgb(52, 152, 219));
            btnEdit.Location = new Point(160, 10);
            btnEdit.Click += BtnEdit_Click;
            toolbar.Controls.Add(btnEdit);

            var btnDelete = CreateButton("🗑️ حذف", Color.FromArgb(231, 76, 60));
            btnDelete.Location = new Point(300, 10);
            btnDelete.Click += BtnDelete_Click;
            toolbar.Controls.Add(btnDelete);

            var btnRefresh = CreateButton("🔄 تحديث", Color.FromArgb(155, 89, 182));
            btnRefresh.Location = new Point(440, 10);
            btnRefresh.Click += (s, e) => LoadData();
            toolbar.Controls.Add(btnRefresh);

            // جدول الحسابات
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 10, 0, 0),
                Padding = new Padding(10)
            };
            this.Controls.Add(gridPanel);

            dgvAccounts = CreateDataGrid();
            dgvAccounts.Columns.Add("Id", "م");
            dgvAccounts.Columns.Add("AccountCode", "رمز الحساب");
            dgvAccounts.Columns.Add("AccountName", "اسم الحساب");
            dgvAccounts.Columns.Add("AccountNameEn", "الاسم بالإنجليزي");
            dgvAccounts.Columns.Add("Type", "النوع");
            dgvAccounts.Columns.Add("Balance", "الرصيد");
            dgvAccounts.Columns.Add("IsActive", "الحالة");

            dgvAccounts.Columns["Id"]!.Width = 50;
            dgvAccounts.Columns["AccountCode"]!.Width = 120;
            dgvAccounts.Columns["Balance"]!.Width = 120;
            dgvAccounts.Columns["IsActive"]!.Width = 80;

            dgvAccounts.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            gridPanel.Controls.Add(dgvAccounts);
            gridPanel.BringToFront();
        }

        private void LoadData()
        {
            _accounts = _db.GetAllAccounts();
            dgvAccounts.Rows.Clear();

            foreach (var account in _accounts)
            {
                var typeName = account.Type switch
                {
                    AccountType.Assets => "أصول",
                    AccountType.Liabilities => "خصوم",
                    AccountType.Equity => "حقوق ملكية",
                    AccountType.Revenue => "إيرادات",
                    AccountType.Expenses => "مصروفات",
                    _ => ""
                };

                dgvAccounts.Rows.Add(
                    account.Id,
                    account.AccountCode,
                    account.AccountName,
                    account.AccountNameEn ?? "",
                    typeName,
                    $"{account.Balance:N2}",
                    account.IsActive ? "نشط" : "معطل"
                );
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var dialog = new AccountDialog(_accounts);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _db.SaveAccount(dialog.Account);
                LoadData();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvAccounts.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvAccounts.SelectedRows[0].Cells["Id"].Value);
            var account = _accounts.FirstOrDefault(a => a.Id == id);
            if (account == null) return;

            using var dialog = new AccountDialog(_accounts, account);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _db.SaveAccount(dialog.Account);
                LoadData();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvAccounts.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvAccounts.SelectedRows[0].Cells["Id"].Value);
            var result = MessageBox.Show("هل أنت متأكد من حذف هذا الحساب؟", "تأكيد الحذف",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _db.DeleteAccount(id);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"لا يمكن حذف الحساب: {ex.Message}", "خطأ",
                        MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                }
            }
        }

        private Button CreateButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Size = new Size(130, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand,
                Location = new Point(10, 10)
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

    /// <summary>
    /// نافذة إضافة/تعديل حساب
    /// </summary>
    public class AccountDialog : Form
    {
        public Account Account { get; private set; } = new();
        private readonly List<Account> _allAccounts;
        private TextBox txtCode = null!;
        private TextBox txtName = null!;
        private TextBox txtNameEn = null!;
        private ComboBox cmbType = null!;
        private ComboBox cmbParent = null!;
        private TextBox txtDescription = null!;
        private CheckBox chkActive = null!;

        public AccountDialog(List<Account> allAccounts, Account? account = null)
        {
            _allAccounts = allAccounts;
            if (account != null)
            {
                Account = account;
            }
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = Account.Id == 0 ? "إضافة حساب جديد" : "تعديل الحساب";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 8,
                Padding = new Padding(20)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            this.Controls.Add(panel);

            // رمز الحساب
            panel.Controls.Add(new Label { Text = "رمز الحساب:", Anchor = AnchorStyles.Right }, 0, 0);
            txtCode = new TextBox { Dock = DockStyle.Fill };
            txtCode.Text = Account.AccountCode;
            panel.Controls.Add(txtCode, 1, 0);

            // اسم الحساب
            panel.Controls.Add(new Label { Text = "اسم الحساب:", Anchor = AnchorStyles.Right }, 0, 1);
            txtName = new TextBox { Dock = DockStyle.Fill };
            txtName.Text = Account.AccountName;
            panel.Controls.Add(txtName, 1, 1);

            // الاسم بالإنجليزي
            panel.Controls.Add(new Label { Text = "الاسم بالإنجليزي:", Anchor = AnchorStyles.Right }, 0, 2);
            txtNameEn = new TextBox { Dock = DockStyle.Fill };
            txtNameEn.Text = Account.AccountNameEn;
            panel.Controls.Add(txtNameEn, 1, 2);

            // نوع الحساب
            panel.Controls.Add(new Label { Text = "نوع الحساب:", Anchor = AnchorStyles.Right }, 0, 3);
            cmbType = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new object[] { "أصول", "خصوم", "حقوق ملكية", "إيرادات", "مصروفات" });
            cmbType.SelectedIndex = (int)Account.Type - 1;
            panel.Controls.Add(cmbType, 1, 3);

            // الحساب الأب
            panel.Controls.Add(new Label { Text = "الحساب الأب:", Anchor = AnchorStyles.Right }, 0, 4);
            cmbParent = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbParent.Items.Add("-- بدون --");
            foreach (var acc in _allAccounts.Where(a => a.Id != Account.Id))
            {
                cmbParent.Items.Add($"{acc.AccountCode} - {acc.AccountName}");
            }
            cmbParent.SelectedIndex = Account.ParentAccountId.HasValue
                ? _allAccounts.FindIndex(a => a.Id == Account.ParentAccountId) + 1
                : 0;
            panel.Controls.Add(cmbParent, 1, 4);

            // الوصف
            panel.Controls.Add(new Label { Text = "الوصف:", Anchor = AnchorStyles.Right }, 0, 5);
            txtDescription = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 60 };
            txtDescription.Text = Account.Description;
            panel.Controls.Add(txtDescription, 1, 5);

            // نشط
            panel.Controls.Add(new Label { Text = "الحالة:", Anchor = AnchorStyles.Right }, 0, 6);
            chkActive = new CheckBox { Text = "نشط", Checked = Account.IsActive };
            panel.Controls.Add(chkActive, 1, 6);

            // أزرار
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill
            };
            panel.Controls.Add(buttonPanel, 1, 7);

            var btnSave = new Button
            {
                Text = "💾 حفظ",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            buttonPanel.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "إلغاء",
                Size = new Size(100, 40),
                BackColor = Color.FromArgb(189, 195, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            buttonPanel.Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("الرجاء إدخال رمز الحساب واسمه", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            Account.AccountCode = txtCode.Text.Trim();
            Account.AccountName = txtName.Text.Trim();
            Account.AccountNameEn = txtNameEn.Text.Trim();
            Account.Type = (AccountType)(cmbType.SelectedIndex + 1);
            Account.ParentAccountId = cmbParent.SelectedIndex > 0
                ? _allAccounts[cmbParent.SelectedIndex - 1].Id
                : null;
            Account.Description = txtDescription.Text.Trim();
            Account.IsActive = chkActive.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
