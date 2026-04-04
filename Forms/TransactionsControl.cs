using SAQR_ERP_Client.Data;
using SAQR_ERP_Client.Models;

namespace SAQR_ERP_Client.Forms
{
    /// <summary>
    /// إدارة القيود اليومية
    /// </summary>
    public class TransactionsControl : UserControl
    {
        private readonly DatabaseService _db = DatabaseService.Instance;
        private DataGridView dgvTransactions = null!;
        private List<Transaction> _transactions = new();

        public TransactionsControl()
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

            var btnAdd = CreateButton("➕ إضافة قيد", Color.FromArgb(46, 204, 113));
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

            var btnPost = CreateButton("✅ ترحيل", Color.FromArgb(155, 89, 182));
            btnPost.Location = new Point(440, 10);
            btnPost.Click += BtnPost_Click;
            toolbar.Controls.Add(btnPost);

            var btnRefresh = CreateButton("🔄 تحديث", Color.FromArgb(149, 165, 166));
            btnRefresh.Location = new Point(580, 10);
            btnRefresh.Click += (s, e) => LoadData();
            toolbar.Controls.Add(btnRefresh);

            // جدول القيود
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 10, 0, 0),
                Padding = new Padding(10)
            };
            this.Controls.Add(gridPanel);

            dgvTransactions = CreateDataGrid();
            dgvTransactions.Columns.Add("Id", "م");
            dgvTransactions.Columns.Add("TransactionNumber", "رقم القيد");
            dgvTransactions.Columns.Add("TransactionDate", "التاريخ");
            dgvTransactions.Columns.Add("Description", "البيان");
            dgvTransactions.Columns.Add("Type", "النوع");
            dgvTransactions.Columns.Add("Debit", "مدين");
            dgvTransactions.Columns.Add("Credit", "دائن");
            dgvTransactions.Columns.Add("Status", "الحالة");

            dgvTransactions.Columns["Id"]!.Width = 50;
            dgvTransactions.Columns["TransactionNumber"]!.Width = 120;
            dgvTransactions.Columns["TransactionDate"]!.Width = 100;
            dgvTransactions.Columns["Debit"]!.Width = 100;
            dgvTransactions.Columns["Credit"]!.Width = 100;
            dgvTransactions.Columns["Status"]!.Width = 80;

            dgvTransactions.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            gridPanel.Controls.Add(dgvTransactions);
            gridPanel.BringToFront();
        }

        private void LoadData()
        {
            _transactions = _db.GetAllTransactions();
            dgvTransactions.Rows.Clear();

            foreach (var trans in _transactions)
            {
                var typeName = trans.Type switch
                {
                    TransactionType.JournalEntry => "قيد يومية",
                    TransactionType.Receipt => "سند قبض",
                    TransactionType.Payment => "سند صرف",
                    TransactionType.SalesInvoice => "فاتورة مبيعات",
                    TransactionType.PurchaseInvoice => "فاتورة مشتريات",
                    _ => ""
                };

                var statusName = trans.Status switch
                {
                    TransactionStatus.Draft => "مسودة",
                    TransactionStatus.Approved => "معتمد",
                    TransactionStatus.Posted => "مرحل",
                    TransactionStatus.Cancelled => "ملغي",
                    _ => ""
                };

                var totalDebit = trans.Lines.Sum(l => l.Debit);
                var totalCredit = trans.Lines.Sum(l => l.Credit);

                dgvTransactions.Rows.Add(
                    trans.Id,
                    trans.TransactionNumber,
                    trans.TransactionDate.ToString("dd/MM/yyyy"),
                    trans.Description,
                    typeName,
                    $"{totalDebit:N2}",
                    $"{totalCredit:N2}",
                    statusName
                );
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var dialog = new TransactionDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _db.SaveTransaction(dialog.Transaction);
                LoadData();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvTransactions.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvTransactions.SelectedRows[0].Cells["Id"].Value);
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            if (transaction == null) return;

            if (transaction.Status == TransactionStatus.Posted)
            {
                MessageBox.Show("لا يمكن تعديل قيد مرحل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            using var dialog = new TransactionDialog(transaction);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _db.SaveTransaction(dialog.Transaction);
                LoadData();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvTransactions.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvTransactions.SelectedRows[0].Cells["Id"].Value);
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            if (transaction == null) return;

            if (transaction.Status == TransactionStatus.Posted)
            {
                MessageBox.Show("لا يمكن حذف قيد مرحل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            var result = MessageBox.Show("هل أنت متأكد من حذف هذا القيد؟", "تأكيد الحذف",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == DialogResult.Yes)
            {
                _db.DeleteTransaction(id);
                LoadData();
            }
        }

        private void BtnPost_Click(object? sender, EventArgs e)
        {
            if (dgvTransactions.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvTransactions.SelectedRows[0].Cells["Id"].Value);
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            if (transaction == null) return;

            if (transaction.Status == TransactionStatus.Posted)
            {
                MessageBox.Show("هذا القيد مرحل بالفعل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            // التحقق من توازن القيد
            var totalDebit = transaction.Lines.Sum(l => l.Debit);
            var totalCredit = transaction.Lines.Sum(l => l.Credit);

            if (totalDebit != totalCredit)
            {
                MessageBox.Show("القيد غير متوازن. يجب أن يكون إجمالي المدين يساوي إجمالي الدائن", "خطأ",
                    MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            var result = MessageBox.Show("هل أنت متأكد من ترحيل هذا القيد؟", "تأكيد الترحيل",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == DialogResult.Yes)
            {
                transaction.Status = TransactionStatus.Posted;
                transaction.ApprovedAt = DateTime.Now;
                transaction.ApprovedBy = Environment.UserName;
                _db.SaveTransaction(transaction);
                LoadData();
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
    /// نافذة إضافة/تعديل قيد
    /// </summary>
    public class TransactionDialog : Form
    {
        public Transaction Transaction { get; private set; } = new();
        private readonly DatabaseService _db = DatabaseService.Instance;
        private List<Account> _accounts = new();

        private DateTimePicker dtpDate = null!;
        private TextBox txtDescription = null!;
        private TextBox txtReference = null!;
        private DataGridView dgvLines = null!;
        private Label lblTotalDebit = null!;
        private Label lblTotalCredit = null!;
        private Label lblDifference = null!;

        public TransactionDialog(Transaction? transaction = null)
        {
            if (transaction != null)
            {
                Transaction = transaction;
            }
            else
            {
                Transaction.TransactionNumber = _db.GetNextTransactionNumber(TransactionType.JournalEntry);
            }
            _accounts = _db.GetAllAccounts();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = Transaction.Id == 0 ? "إضافة قيد جديد" : "تعديل القيد";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(800, 600);
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            // الجزء العلوي
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 150,
                Padding = new Padding(20)
            };
            this.Controls.Add(topPanel);

            // رقم القيد
            topPanel.Controls.Add(new Label { Text = "رقم القيد:", Location = new Point(20, 20), AutoSize = true });
            var txtNumber = new TextBox { Location = new Point(100, 17), Width = 150, Text = Transaction.TransactionNumber, ReadOnly = true };
            topPanel.Controls.Add(txtNumber);

            // التاريخ
            topPanel.Controls.Add(new Label { Text = "التاريخ:", Location = new Point(280, 20), AutoSize = true });
            dtpDate = new DateTimePicker { Location = new Point(340, 17), Width = 150, Value = Transaction.TransactionDate };
            topPanel.Controls.Add(dtpDate);

            // المرجع
            topPanel.Controls.Add(new Label { Text = "المرجع:", Location = new Point(520, 20), AutoSize = true });
            txtReference = new TextBox { Location = new Point(580, 17), Width = 150, Text = Transaction.Reference ?? "" };
            topPanel.Controls.Add(txtReference);

            // البيان
            topPanel.Controls.Add(new Label { Text = "البيان:", Location = new Point(20, 60), AutoSize = true });
            txtDescription = new TextBox { Location = new Point(100, 57), Width = 630, Text = Transaction.Description };
            topPanel.Controls.Add(txtDescription);

            // أزرار إضافة/حذف سطر
            var btnAddLine = new Button
            {
                Text = "➕ إضافة سطر",
                Location = new Point(20, 100),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddLine.Click += (s, e) => AddLine();
            topPanel.Controls.Add(btnAddLine);

            var btnRemoveLine = new Button
            {
                Text = "➖ حذف سطر",
                Location = new Point(150, 100),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRemoveLine.Click += (s, e) => RemoveLine();
            topPanel.Controls.Add(btnRemoveLine);

            // جدول الأسطر
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 10, 20, 10)
            };
            this.Controls.Add(gridPanel);

            dgvLines = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 10F),
                ColumnHeadersHeight = 40,
                RowTemplate = { Height = 35 }
            };

            // عمود الحساب
            var accountColumn = new DataGridViewComboBoxColumn
            {
                Name = "AccountId",
                HeaderText = "الحساب",
                Width = 300,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
            };
            foreach (var account in _accounts)
            {
                accountColumn.Items.Add($"{account.AccountCode} - {account.AccountName}");
            }
            dgvLines.Columns.Add(accountColumn);

            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Debit", HeaderText = "مدين", Width = 120 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Credit", HeaderText = "دائن", Width = 120 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "البيان", Width = 250 });

            dgvLines.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 60, 114);
            dgvLines.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLines.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            dgvLines.EnableHeadersVisualStyles = false;

            dgvLines.CellValueChanged += DgvLines_CellValueChanged;
            dgvLines.CellEndEdit += (s, e) => UpdateTotals();

            gridPanel.Controls.Add(dgvLines);

            // تحميل الأسطر الموجودة
            foreach (var line in Transaction.Lines)
            {
                var accountIndex = _accounts.FindIndex(a => a.Id == line.AccountId);
                var accountDisplay = accountIndex >= 0 ? $"{_accounts[accountIndex].AccountCode} - {_accounts[accountIndex].AccountName}" : "";
                dgvLines.Rows.Add(accountDisplay, line.Debit, line.Credit, line.Description ?? "");
            }

            // الجزء السفلي
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 245, 250)
            };
            this.Controls.Add(bottomPanel);

            // الإجماليات
            lblTotalDebit = new Label { Text = "إجمالي المدين: 0.00", Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            bottomPanel.Controls.Add(lblTotalDebit);

            lblTotalCredit = new Label { Text = "إجمالي الدائن: 0.00", Location = new Point(250, 15), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            bottomPanel.Controls.Add(lblTotalCredit);

            lblDifference = new Label { Text = "الفرق: 0.00", Location = new Point(480, 15), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.Green };
            bottomPanel.Controls.Add(lblDifference);

            // أزرار الحفظ والإلغاء
            var btnSave = new Button
            {
                Text = "💾 حفظ",
                Location = new Point(20, 60),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;
            bottomPanel.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(150, 60),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(189, 195, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            bottomPanel.Controls.Add(btnCancel);

            gridPanel.BringToFront();
            UpdateTotals();
        }

        private void AddLine()
        {
            dgvLines.Rows.Add("", 0, 0, "");
        }

        private void RemoveLine()
        {
            if (dgvLines.SelectedRows.Count > 0)
            {
                dgvLines.Rows.Remove(dgvLines.SelectedRows[0]);
                UpdateTotals();
            }
        }

        private void DgvLines_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            decimal totalDebit = 0, totalCredit = 0;

            foreach (DataGridViewRow row in dgvLines.Rows)
            {
                if (decimal.TryParse(row.Cells["Debit"].Value?.ToString(), out var debit))
                    totalDebit += debit;
                if (decimal.TryParse(row.Cells["Credit"].Value?.ToString(), out var credit))
                    totalCredit += credit;
            }

            lblTotalDebit.Text = $"إجمالي المدين: {totalDebit:N2}";
            lblTotalCredit.Text = $"إجمالي الدائن: {totalCredit:N2}";

            var difference = totalDebit - totalCredit;
            lblDifference.Text = $"الفرق: {Math.Abs(difference):N2}";
            lblDifference.ForeColor = difference == 0 ? Color.Green : Color.Red;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("الرجاء إدخال بيان القيد", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            if (dgvLines.Rows.Count == 0)
            {
                MessageBox.Show("الرجاء إضافة أسطر للقيد", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            Transaction.TransactionDate = dtpDate.Value;
            Transaction.Description = txtDescription.Text.Trim();
            Transaction.Reference = string.IsNullOrWhiteSpace(txtReference.Text) ? null : txtReference.Text.Trim();
            Transaction.Type = TransactionType.JournalEntry;
            Transaction.Lines.Clear();

            foreach (DataGridViewRow row in dgvLines.Rows)
            {
                var accountDisplay = row.Cells["AccountId"].Value?.ToString();
                if (string.IsNullOrEmpty(accountDisplay)) continue;

                var accountCode = accountDisplay.Split(" - ")[0];
                var account = _accounts.FirstOrDefault(a => a.AccountCode == accountCode);
                if (account == null) continue;

                decimal.TryParse(row.Cells["Debit"].Value?.ToString(), out var debit);
                decimal.TryParse(row.Cells["Credit"].Value?.ToString(), out var credit);

                if (debit == 0 && credit == 0) continue;

                Transaction.Lines.Add(new TransactionLine
                {
                    AccountId = account.Id,
                    Debit = debit,
                    Credit = credit,
                    Description = row.Cells["Description"].Value?.ToString()
                });
            }

            if (Transaction.Lines.Count == 0)
            {
                MessageBox.Show("الرجاء إضافة أسطر صحيحة للقيد", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
