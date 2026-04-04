using SAQR_ERP_Client.Data;
using SAQR_ERP_Client.Models;

namespace SAQR_ERP_Client.Forms
{
    /// <summary>
    /// إدارة الموردين
    /// </summary>
    public class SuppliersControl : UserControl
    {
        private readonly DatabaseService _db = DatabaseService.Instance;
        private DataGridView dgvSuppliers = null!;
        private List<Supplier> _suppliers = new();

        public SuppliersControl()
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

            var btnAdd = CreateButton("➕ إضافة مورد", Color.FromArgb(46, 204, 113));
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

            // جدول الموردين
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 10, 0, 0),
                Padding = new Padding(10)
            };
            this.Controls.Add(gridPanel);

            dgvSuppliers = CreateDataGrid();
            dgvSuppliers.Columns.Add("Id", "م");
            dgvSuppliers.Columns.Add("SupplierCode", "كود المورد");
            dgvSuppliers.Columns.Add("Name", "اسم المورد");
            dgvSuppliers.Columns.Add("Phone", "الهاتف");
            dgvSuppliers.Columns.Add("Mobile", "الجوال");
            dgvSuppliers.Columns.Add("City", "المدينة");
            dgvSuppliers.Columns.Add("Balance", "الرصيد");
            dgvSuppliers.Columns.Add("IsActive", "الحالة");

            dgvSuppliers.Columns["Id"]!.Width = 50;
            dgvSuppliers.Columns["SupplierCode"]!.Width = 100;
            dgvSuppliers.Columns["Balance"]!.Width = 100;
            dgvSuppliers.Columns["IsActive"]!.Width = 80;

            dgvSuppliers.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            gridPanel.Controls.Add(dgvSuppliers);
            gridPanel.BringToFront();
        }

        private void LoadData()
        {
            _suppliers = _db.GetAllSuppliers();
            dgvSuppliers.Rows.Clear();

            foreach (var supplier in _suppliers)
            {
                dgvSuppliers.Rows.Add(
                    supplier.Id,
                    supplier.SupplierCode,
                    supplier.Name,
                    supplier.Phone ?? "",
                    supplier.Mobile ?? "",
                    supplier.City ?? "",
                    $"{supplier.Balance:N2}",
                    supplier.IsActive ? "نشط" : "معطل"
                );
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var dialog = new SupplierDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _db.SaveSupplier(dialog.Supplier);
                LoadData();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvSuppliers.SelectedRows[0].Cells["Id"].Value);
            var supplier = _suppliers.FirstOrDefault(s => s.Id == id);
            if (supplier == null) return;

            using var dialog = new SupplierDialog(supplier);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _db.SaveSupplier(dialog.Supplier);
                LoadData();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvSuppliers.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvSuppliers.SelectedRows[0].Cells["Id"].Value);
            var result = MessageBox.Show("هل أنت متأكد من حذف هذا المورد؟", "تأكيد الحذف",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _db.DeleteSupplier(id);
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"لا يمكن حذف المورد: {ex.Message}", "خطأ",
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
    /// نافذة إضافة/تعديل مورد
    /// </summary>
    public class SupplierDialog : Form
    {
        public Supplier Supplier { get; private set; } = new();

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

        public SupplierDialog(Supplier? supplier = null)
        {
            if (supplier != null)
            {
                Supplier = supplier;
            }
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = Supplier.Id == 0 ? "إضافة مورد جديد" : "تعديل بيانات المورد";
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
            panel.Controls.Add(new Label { Text = "كود المورد:", Anchor = AnchorStyles.Right }, 0, 0);
            txtCode = new TextBox { Dock = DockStyle.Fill };
            txtCode.Text = Supplier.SupplierCode;
            panel.Controls.Add(txtCode, 1, 0);

            panel.Controls.Add(new Label { Text = "الاسم:", Anchor = AnchorStyles.Right }, 2, 0);
            txtName = new TextBox { Dock = DockStyle.Fill };
            txtName.Text = Supplier.Name;
            panel.Controls.Add(txtName, 3, 0);

            // الصف الثاني
            panel.Controls.Add(new Label { Text = "الاسم بالإنجليزي:", Anchor = AnchorStyles.Right }, 0, 1);
            txtNameEn = new TextBox { Dock = DockStyle.Fill };
            txtNameEn.Text = Supplier.NameEn;
            panel.Controls.Add(txtNameEn, 1, 1);

            panel.Controls.Add(new Label { Text = "الهاتف:", Anchor = AnchorStyles.Right }, 2, 1);
            txtPhone = new TextBox { Dock = DockStyle.Fill };
            txtPhone.Text = Supplier.Phone;
            panel.Controls.Add(txtPhone, 3, 1);

            // الصف الثالث
            panel.Controls.Add(new Label { Text = "الجوال:", Anchor = AnchorStyles.Right }, 0, 2);
            txtMobile = new TextBox { Dock = DockStyle.Fill };
            txtMobile.Text = Supplier.Mobile;
            panel.Controls.Add(txtMobile, 1, 2);

            panel.Controls.Add(new Label { Text = "البريد:", Anchor = AnchorStyles.Right }, 2, 2);
            txtEmail = new TextBox { Dock = DockStyle.Fill };
            txtEmail.Text = Supplier.Email;
            panel.Controls.Add(txtEmail, 3, 2);

            // الصف الرابع
            panel.Controls.Add(new Label { Text = "المدينة:", Anchor = AnchorStyles.Right }, 0, 3);
            txtCity = new TextBox { Dock = DockStyle.Fill };
            txtCity.Text = Supplier.City;
            panel.Controls.Add(txtCity, 1, 3);

            panel.Controls.Add(new Label { Text = "الدولة:", Anchor = AnchorStyles.Right }, 2, 3);
            txtCountry = new TextBox { Dock = DockStyle.Fill };
            txtCountry.Text = Supplier.Country ?? "المملكة العربية السعودية";
            panel.Controls.Add(txtCountry, 3, 3);

            // الصف الخامس
            panel.Controls.Add(new Label { Text = "العنوان:", Anchor = AnchorStyles.Right }, 0, 4);
            txtAddress = new TextBox { Dock = DockStyle.Fill };
            txtAddress.Text = Supplier.Address;
            panel.SetColumnSpan(txtAddress, 3);
            panel.Controls.Add(txtAddress, 1, 4);

            // الصف السادس
            panel.Controls.Add(new Label { Text = "الرقم الضريبي:", Anchor = AnchorStyles.Right }, 0, 5);
            txtTaxNumber = new TextBox { Dock = DockStyle.Fill };
            txtTaxNumber.Text = Supplier.TaxNumber;
            panel.Controls.Add(txtTaxNumber, 1, 5);

            panel.Controls.Add(new Label { Text = "حد الائتمان:", Anchor = AnchorStyles.Right }, 2, 5);
            nudCreditLimit = new NumericUpDown { Dock = DockStyle.Fill, Maximum = 9999999999, DecimalPlaces = 2 };
            nudCreditLimit.Value = Supplier.CreditLimit;
            panel.Controls.Add(nudCreditLimit, 3, 5);

            // الصف السابع
            panel.Controls.Add(new Label { Text = "ملاحظات:", Anchor = AnchorStyles.Right }, 0, 6);
            txtNotes = new TextBox { Dock = DockStyle.Fill, Multiline = true, Height = 60 };
            txtNotes.Text = Supplier.Notes;
            panel.SetColumnSpan(txtNotes, 3);
            panel.Controls.Add(txtNotes, 1, 6);

            // الصف الثامن
            panel.Controls.Add(new Label { Text = "الحالة:", Anchor = AnchorStyles.Right }, 0, 7);
            chkActive = new CheckBox { Text = "نشط", Checked = Supplier.IsActive };
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
                MessageBox.Show("الرجاء إدخال كود المورد واسمه", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            Supplier.SupplierCode = txtCode.Text.Trim();
            Supplier.Name = txtName.Text.Trim();
            Supplier.NameEn = string.IsNullOrWhiteSpace(txtNameEn.Text) ? null : txtNameEn.Text.Trim();
            Supplier.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim();
            Supplier.Mobile = string.IsNullOrWhiteSpace(txtMobile.Text) ? null : txtMobile.Text.Trim();
            Supplier.Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim();
            Supplier.Address = string.IsNullOrWhiteSpace(txtAddress.Text) ? null : txtAddress.Text.Trim();
            Supplier.City = string.IsNullOrWhiteSpace(txtCity.Text) ? null : txtCity.Text.Trim();
            Supplier.Country = string.IsNullOrWhiteSpace(txtCountry.Text) ? null : txtCountry.Text.Trim();
            Supplier.TaxNumber = string.IsNullOrWhiteSpace(txtTaxNumber.Text) ? null : txtTaxNumber.Text.Trim();
            Supplier.CreditLimit = nudCreditLimit.Value;
            Supplier.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();
            Supplier.IsActive = chkActive.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
