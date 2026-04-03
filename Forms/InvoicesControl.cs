using SAQR_ERP_Client.Data;
using SAQR_ERP_Client.Models;

namespace SAQR_ERP_Client.Forms
{
    /// <summary>
    /// إدارة الفواتير
    /// </summary>
    public class InvoicesControl : UserControl
    {
        private readonly DatabaseService _db = DatabaseService.Instance;
        private readonly InvoiceType _invoiceType;
        private DataGridView dgvInvoices = null!;
        private List<Invoice> _invoices = new();

        public InvoicesControl(InvoiceType type)
        {
            _invoiceType = type;
            InitializeUI();
            LoadData();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.FromArgb(245, 245, 250);
            this.Padding = new Padding(10);

            var typeName = _invoiceType == InvoiceType.Sales ? "فاتورة مبيعات" : "فاتورة مشتريات";

            // شريط الأدوات
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            this.Controls.Add(toolbar);

            var btnAdd = CreateButton($"➕ {typeName} جديدة", Color.FromArgb(46, 204, 113));
            btnAdd.Width = 180;
            btnAdd.Click += BtnAdd_Click;
            toolbar.Controls.Add(btnAdd);

            var btnEdit = CreateButton("✏️ تعديل", Color.FromArgb(52, 152, 219));
            btnEdit.Location = new Point(200, 10);
            btnEdit.Click += BtnEdit_Click;
            toolbar.Controls.Add(btnEdit);

            var btnDelete = CreateButton("🗑️ حذف", Color.FromArgb(231, 76, 60));
            btnDelete.Location = new Point(340, 10);
            btnDelete.Click += BtnDelete_Click;
            toolbar.Controls.Add(btnDelete);

            var btnApprove = CreateButton("✅ اعتماد", Color.FromArgb(155, 89, 182));
            btnApprove.Location = new Point(480, 10);
            btnApprove.Click += BtnApprove_Click;
            toolbar.Controls.Add(btnApprove);

            var btnRefresh = CreateButton("🔄 تحديث", Color.FromArgb(149, 165, 166));
            btnRefresh.Location = new Point(620, 10);
            btnRefresh.Click += (s, e) => LoadData();
            toolbar.Controls.Add(btnRefresh);

            // جدول الفواتير
            var gridPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = new Padding(0, 10, 0, 0),
                Padding = new Padding(10)
            };
            this.Controls.Add(gridPanel);

            dgvInvoices = CreateDataGrid();
            dgvInvoices.Columns.Add("Id", "م");
            dgvInvoices.Columns.Add("InvoiceNumber", "رقم الفاتورة");
            dgvInvoices.Columns.Add("InvoiceDate", "التاريخ");
            dgvInvoices.Columns.Add("PartyName", _invoiceType == InvoiceType.Sales ? "العميل" : "المورد");
            dgvInvoices.Columns.Add("SubTotal", "المبلغ قبل الضريبة");
            dgvInvoices.Columns.Add("TaxAmount", "الضريبة");
            dgvInvoices.Columns.Add("Total", "الإجمالي");
            dgvInvoices.Columns.Add("PaidAmount", "المدفوع");
            dgvInvoices.Columns.Add("Remaining", "المتبقي");
            dgvInvoices.Columns.Add("Status", "الحالة");

            dgvInvoices.Columns["Id"]!.Width = 50;
            dgvInvoices.Columns["InvoiceNumber"]!.Width = 120;
            dgvInvoices.Columns["InvoiceDate"]!.Width = 100;

            dgvInvoices.DoubleClick += (s, e) => BtnEdit_Click(s, e);

            gridPanel.Controls.Add(dgvInvoices);
            gridPanel.BringToFront();
        }

        private void LoadData()
        {
            _invoices = _db.GetAllInvoices(_invoiceType);
            dgvInvoices.Rows.Clear();

            foreach (var invoice in _invoices)
            {
                var partyName = _invoiceType == InvoiceType.Sales ? invoice.CustomerName : invoice.SupplierName;
                var statusName = invoice.Status switch
                {
                    InvoiceStatus.Draft => "مسودة",
                    InvoiceStatus.Approved => "معتمدة",
                    InvoiceStatus.PartiallyPaid => "مدفوعة جزئياً",
                    InvoiceStatus.FullyPaid => "مدفوعة",
                    InvoiceStatus.Cancelled => "ملغاة",
                    _ => ""
                };

                dgvInvoices.Rows.Add(
                    invoice.Id,
                    invoice.InvoiceNumber,
                    invoice.InvoiceDate.ToString("dd/MM/yyyy"),
                    partyName ?? "",
                    $"{invoice.SubTotal:N2}",
                    $"{invoice.TaxAmount:N2}",
                    $"{invoice.Total:N2}",
                    $"{invoice.PaidAmount:N2}",
                    $"{invoice.RemainingAmount:N2}",
                    statusName
                );
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using var dialog = new InvoiceDialog(_invoiceType);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _db.SaveInvoice(dialog.Invoice);
                LoadData();
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvInvoices.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvInvoices.SelectedRows[0].Cells["Id"].Value);
            var invoice = _invoices.FirstOrDefault(i => i.Id == id);
            if (invoice == null) return;

            if (invoice.Status != InvoiceStatus.Draft)
            {
                MessageBox.Show("لا يمكن تعديل فاتورة معتمدة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            using var dialog = new InvoiceDialog(_invoiceType, invoice);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _db.SaveInvoice(dialog.Invoice);
                LoadData();
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvInvoices.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvInvoices.SelectedRows[0].Cells["Id"].Value);
            var invoice = _invoices.FirstOrDefault(i => i.Id == id);
            if (invoice == null) return;

            if (invoice.Status != InvoiceStatus.Draft)
            {
                MessageBox.Show("لا يمكن حذف فاتورة معتمدة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            var result = MessageBox.Show("هل أنت متأكد من حذف هذه الفاتورة؟", "تأكيد الحذف",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == DialogResult.Yes)
            {
                _db.DeleteInvoice(id);
                LoadData();
            }
        }

        private void BtnApprove_Click(object? sender, EventArgs e)
        {
            if (dgvInvoices.SelectedRows.Count == 0) return;

            var id = Convert.ToInt32(dgvInvoices.SelectedRows[0].Cells["Id"].Value);
            var invoice = _invoices.FirstOrDefault(i => i.Id == id);
            if (invoice == null) return;

            if (invoice.Status != InvoiceStatus.Draft)
            {
                MessageBox.Show("هذه الفاتورة معتمدة بالفعل", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            var result = MessageBox.Show("هل أنت متأكد من اعتماد هذه الفاتورة؟", "تأكيد الاعتماد",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

            if (result == DialogResult.Yes)
            {
                invoice.Status = InvoiceStatus.Approved;
                _db.SaveInvoice(invoice);
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
    /// نافذة إضافة/تعديل فاتورة
    /// </summary>
    public class InvoiceDialog : Form
    {
        public Invoice Invoice { get; private set; } = new();
        private readonly InvoiceType _invoiceType;
        private readonly DatabaseService _db = DatabaseService.Instance;
        private List<Customer> _customers = new();
        private List<Supplier> _suppliers = new();

        private DateTimePicker dtpDate = null!;
        private ComboBox cmbParty = null!;
        private TextBox txtNotes = null!;
        private DataGridView dgvLines = null!;
        private Label lblSubTotal = null!;
        private Label lblTax = null!;
        private Label lblTotal = null!;
        private NumericUpDown nudTaxRate = null!;

        public InvoiceDialog(InvoiceType type, Invoice? invoice = null)
        {
            _invoiceType = type;
            if (invoice != null)
            {
                Invoice = invoice;
            }
            else
            {
                Invoice.Type = type;
            }

            _customers = _db.GetAllCustomers();
            _suppliers = _db.GetAllSuppliers();
            InitializeUI();
        }

        private void InitializeUI()
        {
            var typeName = _invoiceType == InvoiceType.Sales ? "فاتورة مبيعات" : "فاتورة مشتريات";
            this.Text = Invoice.Id == 0 ? $"إنشاء {typeName}" : $"تعديل {typeName}";
            this.Size = new Size(1000, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(900, 650);
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

            // رقم الفاتورة
            topPanel.Controls.Add(new Label { Text = "رقم الفاتورة:", Location = new Point(20, 20), AutoSize = true });
            var txtNumber = new TextBox
            {
                Location = new Point(120, 17),
                Width = 150,
                Text = Invoice.InvoiceNumber,
                ReadOnly = true
            };
            if (Invoice.Id == 0)
            {
                txtNumber.Text = "تلقائي";
            }
            topPanel.Controls.Add(txtNumber);

            // التاريخ
            topPanel.Controls.Add(new Label { Text = "التاريخ:", Location = new Point(300, 20), AutoSize = true });
            dtpDate = new DateTimePicker { Location = new Point(360, 17), Width = 150, Value = Invoice.InvoiceDate };
            topPanel.Controls.Add(dtpDate);

            // نسبة الضريبة
            topPanel.Controls.Add(new Label { Text = "نسبة الضريبة %:", Location = new Point(540, 20), AutoSize = true });
            nudTaxRate = new NumericUpDown { Location = new Point(660, 17), Width = 80, Maximum = 100, DecimalPlaces = 2, Value = Invoice.TaxPercent };
            nudTaxRate.ValueChanged += (s, e) => UpdateTotals();
            topPanel.Controls.Add(nudTaxRate);

            // العميل/المورد
            var partyLabel = _invoiceType == InvoiceType.Sales ? "العميل:" : "المورد:";
            topPanel.Controls.Add(new Label { Text = partyLabel, Location = new Point(20, 60), AutoSize = true });
            cmbParty = new ComboBox { Location = new Point(120, 57), Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };

            cmbParty.Items.Add("-- اختر --");
            if (_invoiceType == InvoiceType.Sales)
            {
                foreach (var customer in _customers)
                {
                    cmbParty.Items.Add($"{customer.CustomerCode} - {customer.Name}");
                }
                if (Invoice.CustomerId.HasValue)
                {
                    var idx = _customers.FindIndex(c => c.Id == Invoice.CustomerId);
                    cmbParty.SelectedIndex = idx >= 0 ? idx + 1 : 0;
                }
                else
                {
                    cmbParty.SelectedIndex = 0;
                }
            }
            else
            {
                foreach (var supplier in _suppliers)
                {
                    cmbParty.Items.Add($"{supplier.SupplierCode} - {supplier.Name}");
                }
                if (Invoice.SupplierId.HasValue)
                {
                    var idx = _suppliers.FindIndex(s => s.Id == Invoice.SupplierId);
                    cmbParty.SelectedIndex = idx >= 0 ? idx + 1 : 0;
                }
                else
                {
                    cmbParty.SelectedIndex = 0;
                }
            }
            topPanel.Controls.Add(cmbParty);

            // ملاحظات
            topPanel.Controls.Add(new Label { Text = "ملاحظات:", Location = new Point(550, 60), AutoSize = true });
            txtNotes = new TextBox { Location = new Point(620, 57), Width = 300, Text = Invoice.Notes ?? "" };
            topPanel.Controls.Add(txtNotes);

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

            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "الوصف", Width = 250 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "الكمية", Width = 80 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "Unit", HeaderText = "الوحدة", Width = 80 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "السعر", Width = 100 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "DiscountPercent", HeaderText = "خصم %", Width = 80 });
            dgvLines.Columns.Add(new DataGridViewTextBoxColumn { Name = "LineTotal", HeaderText = "الإجمالي", Width = 120, ReadOnly = true });

            dgvLines.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 60, 114);
            dgvLines.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLines.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            dgvLines.EnableHeadersVisualStyles = false;

            dgvLines.CellEndEdit += (s, e) => { CalculateLineTotal(e.RowIndex); UpdateTotals(); };

            gridPanel.Controls.Add(dgvLines);

            // تحميل الأسطر الموجودة
            foreach (var line in Invoice.Lines)
            {
                dgvLines.Rows.Add(line.Description, line.Quantity, line.Unit, line.UnitPrice, line.DiscountPercent, $"{line.Total:N2}");
            }

            // الجزء السفلي
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 140,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 245, 250)
            };
            this.Controls.Add(bottomPanel);

            // الإجماليات
            lblSubTotal = new Label { Text = $"المبلغ قبل الضريبة: {Invoice.SubTotal:N2}", Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            bottomPanel.Controls.Add(lblSubTotal);

            lblTax = new Label { Text = $"ضريبة القيمة المضافة ({nudTaxRate.Value}%): {Invoice.TaxAmount:N2}", Location = new Point(280, 15), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            bottomPanel.Controls.Add(lblTax);

            lblTotal = new Label { Text = $"الإجمالي: {Invoice.Total:N2}", Location = new Point(600, 15), AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(46, 204, 113) };
            bottomPanel.Controls.Add(lblTotal);

            // أزرار الحفظ والإلغاء
            var btnSave = new Button
            {
                Text = "💾 حفظ",
                Location = new Point(20, 70),
                Size = new Size(120, 45),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F)
            };
            btnSave.Click += BtnSave_Click;
            bottomPanel.Controls.Add(btnSave);

            var btnCancel = new Button
            {
                Text = "إلغاء",
                Location = new Point(150, 70),
                Size = new Size(120, 45),
                BackColor = Color.FromArgb(189, 195, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F)
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            bottomPanel.Controls.Add(btnCancel);

            gridPanel.BringToFront();
            UpdateTotals();
        }

        private void AddLine()
        {
            dgvLines.Rows.Add("", 1, "وحدة", 0, 0, "0.00");
        }

        private void RemoveLine()
        {
            if (dgvLines.SelectedRows.Count > 0)
            {
                dgvLines.Rows.Remove(dgvLines.SelectedRows[0]);
                UpdateTotals();
            }
        }

        private void CalculateLineTotal(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvLines.Rows.Count) return;

            var row = dgvLines.Rows[rowIndex];
            decimal.TryParse(row.Cells["Quantity"].Value?.ToString(), out var quantity);
            decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out var unitPrice);
            decimal.TryParse(row.Cells["DiscountPercent"].Value?.ToString(), out var discountPercent);

            var lineTotal = quantity * unitPrice;
            var discount = lineTotal * discountPercent / 100;
            lineTotal -= discount;

            row.Cells["LineTotal"].Value = $"{lineTotal:N2}";
        }

        private void UpdateTotals()
        {
            decimal subTotal = 0;

            foreach (DataGridViewRow row in dgvLines.Rows)
            {
                if (decimal.TryParse(row.Cells["LineTotal"].Value?.ToString()?.Replace(",", ""), out var lineTotal))
                {
                    subTotal += lineTotal;
                }
            }

            var taxRate = nudTaxRate.Value;
            var taxAmount = subTotal * taxRate / 100;
            var total = subTotal + taxAmount;

            lblSubTotal.Text = $"المبلغ قبل الضريبة: {subTotal:N2}";
            lblTax.Text = $"ضريبة القيمة المضافة ({taxRate}%): {taxAmount:N2}";
            lblTotal.Text = $"الإجمالي: {total:N2}";
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cmbParty.SelectedIndex <= 0)
            {
                var partyName = _invoiceType == InvoiceType.Sales ? "العميل" : "المورد";
                MessageBox.Show($"الرجاء اختيار {partyName}", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            if (dgvLines.Rows.Count == 0)
            {
                MessageBox.Show("الرجاء إضافة أسطر للفاتورة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            Invoice.InvoiceDate = dtpDate.Value;
            Invoice.TaxPercent = nudTaxRate.Value;
            Invoice.Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim();

            if (_invoiceType == InvoiceType.Sales)
            {
                Invoice.CustomerId = _customers[cmbParty.SelectedIndex - 1].Id;
                Invoice.SupplierId = null;
            }
            else
            {
                Invoice.SupplierId = _suppliers[cmbParty.SelectedIndex - 1].Id;
                Invoice.CustomerId = null;
            }

            Invoice.Lines.Clear();
            decimal subTotal = 0;

            foreach (DataGridViewRow row in dgvLines.Rows)
            {
                var description = row.Cells["Description"].Value?.ToString()?.Trim() ?? "";
                if (string.IsNullOrEmpty(description)) continue;

                decimal.TryParse(row.Cells["Quantity"].Value?.ToString(), out var quantity);
                decimal.TryParse(row.Cells["UnitPrice"].Value?.ToString(), out var unitPrice);
                decimal.TryParse(row.Cells["DiscountPercent"].Value?.ToString(), out var discountPercent);

                var lineTotal = quantity * unitPrice;
                var discountAmount = lineTotal * discountPercent / 100;
                lineTotal -= discountAmount;

                var lineTaxAmount = lineTotal * Invoice.TaxPercent / 100;

                subTotal += lineTotal;

                Invoice.Lines.Add(new InvoiceLine
                {
                    Description = description,
                    Quantity = quantity,
                    Unit = row.Cells["Unit"].Value?.ToString() ?? "وحدة",
                    UnitPrice = unitPrice,
                    DiscountPercent = discountPercent,
                    DiscountAmount = discountAmount,
                    TaxPercent = Invoice.TaxPercent,
                    TaxAmount = lineTaxAmount,
                    Total = lineTotal
                });
            }

            if (Invoice.Lines.Count == 0)
            {
                MessageBox.Show("الرجاء إضافة أسطر صحيحة للفاتورة", "تنبيه",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                return;
            }

            Invoice.SubTotal = subTotal;
            Invoice.TaxAmount = subTotal * Invoice.TaxPercent / 100;
            Invoice.Total = Invoice.SubTotal + Invoice.TaxAmount;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
