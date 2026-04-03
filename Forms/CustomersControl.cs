using SAQR_ERP_Client.Data;
using SAQR_ERP_Client.Models;

namespace SAQR_ERP_Client.Forms
{
    /// <summary>
    /// إدارة العملاء
    /// </summary>
    public class CustomersControl : UserControl
    {
        private readonly DatabaseService _db = DatabaseService.Instance;
        private DataGridView dgvCustomers = null!;
        private List<Customer> _customers = new();

        public CustomersControl()
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

            var btnAdd = CreateButton("➕ إضافة عميل", Color.FromArgb(46, 204, 113));
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

            // جدول العملاء
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 10, 0, 0),
                Padding = new Padding(10)
            };
            this.Controls.Add(gridPanel);

            dgvCustomers = CreateDataGrid();
            dgvCustomers.Columns.Add("Id", "م");
            dgvCustomers.Columns.Add("CustomerCode", "كود العميل");
            dgvCustomers.Columns.Add("Name", "اسم العميل");
            dgvCustomers.Columns.Add("Phone", "الهاتف");
            dgvCustomers.Columns.Add("Mobile", "الجوال");
            dgvCustomers.Columns.Add("City", "المدينة");
            dgvCustomers.Columns.Add("Balance", "الرصيد");
            dgvCustomers.Columns.Add("IsActive", "الحالة");

            dgvCustomers.Columns["Id"]!.Width = 50;
            dgvCustomers.Columns["CustomerCode"]!.Width = 100;
            dgvCustomers.Columns["Balance"]!.Width = 100;
            dgvCustomers.Columns["IsActive"]!.Width = 80;

            dgvCustomers.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            gridPanel.Controls.Add(dgvCustomers);
            gridPanel.BringToFront();
        }

        private void LoadData()
        {
            _customers = _db.GetAllCustomers();
            dgvCustomers.Rows.Clear();

            foreach (var customer in _customers)
            {
                dgvCustomers.Rows.Add(
                    customer.Id,
                    customer.CustomerCode,
                    customer.Name,
                    customer.Phone ?? "",
                    customer.Mobile ?? "",
                    customer.City ?? "",
                    $"{customer.Balance:N2}",
                    customer.IsActive ? "نشط" : "معطل"
                );
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var dialog = new CustomerDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _db.SaveCustomer(dialog.Customer);
                LoadData();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["Id"].Value);
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            if (customer == null) return;

            using var dialog = new CustomerDialog(customer);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _db.SaveCustomer(dialog.Customer);
                LoadData();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["Id"].Value);
            var result = MessageBox.Show("هل أنت متأكد من حذف هذا العميل؟", "تأكيد الحذف",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _db.DeleteCustomer(id);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"لا يمكن حذف العميل: {ex.Message}", "خطأ",
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
    /// نافذة إضافة/تعديل عميل
    /// </summary>
    public class CustomerDialog : Form
    {
        public Customer Customer { get; private set; } = new();

        private TextBox txtCode = null!;
        private TextBox txtName = null!;
        private TextBox txtNameEn = null!;
        private TextBox txtPhone = null!;
        private TextBox txtMobile = null!;
        private TextBox txtEmail = null!;
        private TextBox txtAddress = null!;
        private TextBox txtCity = null!;
        private TextBox txtCountry = null!;
        private TextBox txtTaxNumber = null!;
        private NumericUpDown nudCreditLimit = null!;
        private TextBox txtNotes = null!;
        private CheckBox chkActive = null!;

        public CustomerDialog(Customer? customer = null)
        {
            if (customer != null)
            {
                Customer = customer;
            }
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = Customer.Id == 0 ? "إضافة عميل جديد" : "تعديل بيانات العميل";
            this.Size = new Size(600, 600);
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
                ColumnCount = 4,
                RowCount = 10,
                Padding = new Padding(20)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            this.Controls.Add(panel);

            // الصف الأول
            panel.Controls.Add(new Label { Text = "كود العميل:", Anchor = AnchorStyles.Right }, 0, 0);
            txtCode = new TextBox { Dock = DockStyle.Fill };
            txtCode.Text = Customer.CustomerCode;
            panel.Controls.Add(txtCode, 1, 0);

            panel.Controls.Add(new Label { Text = "الاسم:", Anchor = AnchorStyles.Right }, 2, 0);
            txtName = new TextBox { Dock = DockStyle.Fill };
            txtName.Text = Customer.Name;
            panel.Controls.Add(txtName, 3, 0);

            // الصف الثاني
            panel.Controls.Add(new Label { Text = "الاسم بالإنجليزي:", Anchor = AnchorStyles.Right }, 0, 1);
            txtNameEn = new TextBox { Dock = DockStyle.Fill };
            txtNameEn.Text = Customer.NameEn;
            panel.Controls.Add(txtNameEn, 1, 1);

            panel.Controls.Add(new Label { Text = "الهاتف:", Anchor = AnchorStyles.Right }, 2, 1);
            txtPhone = new TextBox { Dock = DockStyle.Fill };
            txtPhone.Text = Customer.Phone;
            panel.Controls.Add(txtPhone, 3, 1);

            // الصف الثالث
            panel.Controls.Add(new Label { Text = "الجوال:", Anchor = AnchorStyles.Right }, 0, 2);
            txtMobile = new TextBox { Dock = DockStyle.Fill };
            txtMobile.Text = Customer.Mobile;
            panel.Controls.Add(txtMobile, 1, 2);

            panel.Controls.Add(new Label { Text = "البريد:", Anchor = AnchorStyles.Right }, 2, 2);
            txtEmail = new TextBox { Dock = DockStyle.Fill };
            txtEmail.Text = Customer.Email;
            panel.Controls.Add(txtEmail, 3, 2);

            // الصف الرابع
            panel.Controls.Add(new Label { Text = "المدينة:", Anchor = AnchorStyles.Right }, 0, 3);
            txtCity = new TextBox { Dock = DockStyle.Fill };
            txtCity.Text = Customer.City;
            panel.Controls.Add(txtCity, 1, 3);

            panel.Controls.Add(new Label { Text = "الدولة:", Anchor = AnchorStyles.Right }, 2, 3);
            txtCountry = new TextBox { Dock = DockStyle.Fill };
            txtCountry.Text = Customer.Country ?? "المملكة العربية السعودية";
            panel.Controls.Add(txtCountry, 3, 3);

            // الصف الخامس
            panel.Controls.Add(new Label { Text = "العنوان:", Anchor = AnchorStyles.Right }, 0, 4);
            txtAddress = new TextBox { Dock = DockStyle.Fill };
            txtAddress.Text = Customer.Address;
            panel.SetColumnSpan(txtAddress, 3);
            panel.Controls.Add(txtAddress, 1, 4);

            // الصف السادس
            panel.Controls.Add(new Label { Text = "الرقم الضريبي:", Anchor = AnchorStyles.Right }, 0, 5);
            txtTaxNumber = new TextBox { Dock = DockStyle.Fill };
            txtTaxNumber.Text = Customer.TaxNumber;
            panel.Controls.Add(txtTaxNumber, 1, 5);

            panel.Controls.Add(new Label { Text = "حد الائتمان:", Anchor = AnchorStyles.Right }, 2, 5);
            nudCreditLimit = new NumericUpDown { Dock = DockStyle.Fill, Maximum = 9999999999, DecimalPlaces = 2 };
            nudCreditLimit.Value = Customer.CreditLimit;
            panel.Controls.Add(nudCreditLimit, 3, 5);

            // الصف السابع
            panel.Controls.Add(new Label { Text = "ملاحظات:", Anchor = AnchorStyles.Right }, 0, 6);
            txtNotes = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 60 };
            txtNotes.Text = Customer.Notes;
            panel.SetColumnSpan(txtNotes, 3);
            panel.Controls.Add(txtNotes, 1, 6);

            // الصف الثامن
            panel.Controls.Add(new Label { Text = "الحالة:", Anchor = AnchorStyles.Right }, 0, 7);
            chkActive = new CheckBox { Text = "نشط", Checked = Customer.IsActive };
            panel.Controls.Add(chkActive, 1, 7);

            // أزرار
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill
            };
            panel.SetColumnSpan(buttonPanel, 4);
            panel.Controls.Add(buttonPanel, 0, 9);

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
                MessageBox.Show("الرجاء إدخال كود العميل واسمه", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            Customer.CustomerCode = txtCode.Text.Trim();
            Customer.Name = txtName.Text.Trim();
            Customer.NameEn = string.IsNullOrWhiteSpace(txtNameEn.Text) ? null : txtNameEn.Text.Trim();
            Customer.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
            Customer.Mobile = string.IsNullOrWhiteSpace(txtMobile.Text) ? null : txtMobile.Text.Trim();
            Customer.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
            Customer.Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim();
            Customer.City = string.IsNullOrWhiteSpace(txtCity.Text) ? null : txtCity.Text.Trim();
            Customer.Country = string.IsNullOrWhiteSpace(txtCountry.Text) ? null : txtCountry.Text.Trim();
            Customer.TaxNumber = string.IsNullOrWhiteSpace(txtTaxNumber.Text) ? null : txtTaxNumber.Text.Trim();
            Customer.CreditLimit = nudCreditLimit.Value;
            Customer.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
            Customer.IsActive = chkActive.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
