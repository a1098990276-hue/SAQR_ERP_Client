using Microsoft.Data.Sqlite;
using SAQR_ERP_Client.Models;

namespace SAQR_ERP_Client.Data
{
    /// <summary>
    /// خدمة قاعدة البيانات - SQLite
    /// </summary>
    public class DatabaseService
    {
        private readonly string _connectionString;
        private static DatabaseService? _instance;

        public static DatabaseService Instance => _instance ??= new DatabaseService();

        private DatabaseService()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaqrAccounting.db");
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// تهيئة قاعدة البيانات وإنشاء الجداول
        /// </summary>
        private void InitializeDatabase()
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();

            // جدول الحسابات
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Accounts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AccountCode TEXT NOT NULL UNIQUE,
                    AccountName TEXT NOT NULL,
                    AccountNameEn TEXT,
                    Type INTEGER NOT NULL,
                    ParentAccountId INTEGER,
                    Balance REAL DEFAULT 0,
                    IsActive INTEGER DEFAULT 1,
                    Description TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    FOREIGN KEY (ParentAccountId) REFERENCES Accounts(Id)
                );
            ";
            command.ExecuteNonQuery();

            // جدول القيود
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TransactionNumber TEXT NOT NULL UNIQUE,
                    TransactionDate TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    Type INTEGER NOT NULL,
                    Status INTEGER DEFAULT 1,
                    Reference TEXT,
                    CustomerId INTEGER,
                    SupplierId INTEGER,
                    CreatedAt TEXT NOT NULL,
                    CreatedBy TEXT,
                    ApprovedAt TEXT,
                    ApprovedBy TEXT,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
                    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
                );
            ";
            command.ExecuteNonQuery();

            // جدول أسطر القيود
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS TransactionLines (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TransactionId INTEGER NOT NULL,
                    AccountId INTEGER NOT NULL,
                    Debit REAL DEFAULT 0,
                    Credit REAL DEFAULT 0,
                    Description TEXT,
                    FOREIGN KEY (TransactionId) REFERENCES Transactions(Id) ON DELETE CASCADE,
                    FOREIGN KEY (AccountId) REFERENCES Accounts(Id)
                );
            ";
            command.ExecuteNonQuery();

            // جدول العملاء
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Customers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CustomerCode TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    NameEn TEXT,
                    Phone TEXT,
                    Mobile TEXT,
                    Email TEXT,
                    Address TEXT,
                    City TEXT,
                    Country TEXT,
                    TaxNumber TEXT,
                    CommercialRegister TEXT,
                    CreditLimit REAL DEFAULT 0,
                    Balance REAL DEFAULT 0,
                    AccountId INTEGER,
                    IsActive INTEGER DEFAULT 1,
                    Notes TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    FOREIGN KEY (AccountId) REFERENCES Accounts(Id)
                );
            ";
            command.ExecuteNonQuery();

            // جدول الموردين
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Suppliers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SupplierCode TEXT NOT NULL UNIQUE,
                    Name TEXT NOT NULL,
                    NameEn TEXT,
                    Phone TEXT,
                    Mobile TEXT,
                    Email TEXT,
                    Address TEXT,
                    City TEXT,
                    Country TEXT,
                    TaxNumber TEXT,
                    CommercialRegister TEXT,
                    CreditLimit REAL DEFAULT 0,
                    Balance REAL DEFAULT 0,
                    AccountId INTEGER,
                    IsActive INTEGER DEFAULT 1,
                    Notes TEXT,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT,
                    FOREIGN KEY (AccountId) REFERENCES Accounts(Id)
                );
            ";
            command.ExecuteNonQuery();

            // جدول الفواتير
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Invoices (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceNumber TEXT NOT NULL UNIQUE,
                    Type INTEGER NOT NULL,
                    InvoiceDate TEXT NOT NULL,
                    DueDate TEXT,
                    CustomerId INTEGER,
                    SupplierId INTEGER,
                    SubTotal REAL DEFAULT 0,
                    DiscountPercent REAL DEFAULT 0,
                    DiscountAmount REAL DEFAULT 0,
                    TaxPercent REAL DEFAULT 15,
                    TaxAmount REAL DEFAULT 0,
                    Total REAL DEFAULT 0,
                    PaidAmount REAL DEFAULT 0,
                    Status INTEGER DEFAULT 1,
                    Notes TEXT,
                    TransactionId INTEGER,
                    CreatedAt TEXT NOT NULL,
                    CreatedBy TEXT,
                    UpdatedAt TEXT,
                    FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
                    FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id),
                    FOREIGN KEY (TransactionId) REFERENCES Transactions(Id)
                );
            ";
            command.ExecuteNonQuery();

            // جدول أسطر الفواتير
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS InvoiceLines (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InvoiceId INTEGER NOT NULL,
                    LineNumber INTEGER NOT NULL,
                    Description TEXT NOT NULL,
                    ItemCode TEXT,
                    Quantity REAL DEFAULT 1,
                    Unit TEXT DEFAULT 'وحدة',
                    UnitPrice REAL DEFAULT 0,
                    DiscountPercent REAL DEFAULT 0,
                    DiscountAmount REAL DEFAULT 0,
                    TaxPercent REAL DEFAULT 15,
                    TaxAmount REAL DEFAULT 0,
                    Total REAL DEFAULT 0,
                    AccountId INTEGER,
                    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
                    FOREIGN KEY (AccountId) REFERENCES Accounts(Id)
                );
            ";
            command.ExecuteNonQuery();

            // جدول الإعدادات
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Settings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    CompanyName TEXT NOT NULL,
                    CompanyNameEn TEXT,
                    CompanyLogo TEXT,
                    Address TEXT,
                    City TEXT,
                    Country TEXT,
                    Phone TEXT,
                    Mobile TEXT,
                    Email TEXT,
                    Website TEXT,
                    TaxNumber TEXT,
                    CommercialRegister TEXT,
                    Currency TEXT DEFAULT 'ريال',
                    CurrencyCode TEXT DEFAULT 'SAR',
                    DefaultTaxRate REAL DEFAULT 15,
                    FiscalYearStart INTEGER DEFAULT 1,
                    DateFormat TEXT DEFAULT 'dd/MM/yyyy',
                    UseHijriDate INTEGER DEFAULT 0,
                    LastBackup TEXT
                );
            ";
            command.ExecuteNonQuery();

            // جدول السنوات المالية
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS FiscalYears (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    StartDate TEXT NOT NULL,
                    EndDate TEXT NOT NULL,
                    IsCurrent INTEGER DEFAULT 0,
                    IsClosed INTEGER DEFAULT 0,
                    ClosedAt TEXT,
                    ClosedBy TEXT
                );
            ";
            command.ExecuteNonQuery();

            // إدراج الإعدادات الافتراضية إذا لم تكن موجودة
            command.CommandText = "SELECT COUNT(*) FROM Settings";
            var count = Convert.ToInt32(command.ExecuteScalar());
            if (count == 0)
            {
                command.CommandText = @"
                    INSERT INTO Settings (CompanyName, CompanyNameEn, Country, Currency, CurrencyCode, DefaultTaxRate)
                    VALUES ('الصقر لأنظمة المحاسبة', 'Al-Saqr Accounting Systems', 'المملكة العربية السعودية', 'ريال', 'SAR', 15)
                ";
                command.ExecuteNonQuery();
            }

            // إدراج الحسابات الافتراضية إذا لم تكن موجودة
            command.CommandText = "SELECT COUNT(*) FROM Accounts";
            count = Convert.ToInt32(command.ExecuteScalar());
            if (count == 0)
            {
                InsertDefaultAccounts(connection);
            }
        }

        /// <summary>
        /// إدراج الحسابات الافتراضية
        /// </summary>
        private void InsertDefaultAccounts(SqliteConnection connection)
        {
            var command = connection.CreateCommand();
            var now = DateTime.Now.ToString("o");

            var accounts = new[]
            {
                // الأصول
                ("1", "الأصول", "Assets", AccountType.Assets, (int?)null),
                ("11", "الأصول المتداولة", "Current Assets", AccountType.Assets, 1),
                ("111", "النقدية والبنوك", "Cash and Banks", AccountType.Assets, 2),
                ("1111", "الصندوق", "Cash", AccountType.Assets, 3),
                ("1112", "البنك", "Bank", AccountType.Assets, 3),
                ("112", "ذمم العملاء", "Accounts Receivable", AccountType.Assets, 2),
                ("12", "الأصول الثابتة", "Fixed Assets", AccountType.Assets, 1),
                
                // الخصوم
                ("2", "الخصوم", "Liabilities", AccountType.Liabilities, (int?)null),
                ("21", "الخصوم المتداولة", "Current Liabilities", AccountType.Liabilities, 8),
                ("211", "ذمم الموردين", "Accounts Payable", AccountType.Liabilities, 9),
                ("212", "ضريبة القيمة المضافة المستحقة", "VAT Payable", AccountType.Liabilities, 9),
                
                // حقوق الملكية
                ("3", "حقوق الملكية", "Equity", AccountType.Equity, (int?)null),
                ("31", "رأس المال", "Capital", AccountType.Equity, 12),
                ("32", "الأرباح المحتجزة", "Retained Earnings", AccountType.Equity, 12),
                
                // الإيرادات
                ("4", "الإيرادات", "Revenue", AccountType.Revenue, (int?)null),
                ("41", "إيرادات المبيعات", "Sales Revenue", AccountType.Revenue, 15),
                ("42", "إيرادات أخرى", "Other Revenue", AccountType.Revenue, 15),
                
                // المصروفات
                ("5", "المصروفات", "Expenses", AccountType.Expenses, (int?)null),
                ("51", "تكلفة المبيعات", "Cost of Sales", AccountType.Expenses, 18),
                ("52", "مصاريف إدارية", "Administrative Expenses", AccountType.Expenses, 18),
                ("53", "مصاريف تشغيلية", "Operating Expenses", AccountType.Expenses, 18),
            };

            foreach (var (code, name, nameEn, type, parentId) in accounts)
            {
                command.CommandText = @"
                    INSERT INTO Accounts (AccountCode, AccountName, AccountNameEn, Type, ParentAccountId, CreatedAt)
                    VALUES (@code, @name, @nameEn, @type, @parentId, @createdAt)
                ";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@code", code);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@nameEn", nameEn);
                command.Parameters.AddWithValue("@type", (int)type);
                command.Parameters.AddWithValue("@parentId", parentId.HasValue ? parentId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@createdAt", now);
                command.ExecuteNonQuery();
            }
        }

        #region Account Operations

        public List<Account> GetAllAccounts()
        {
            var accounts = new List<Account>();
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Accounts ORDER BY AccountCode";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                accounts.Add(MapAccount(reader));
            }
            return accounts;
        }

        public Account? GetAccountById(int id)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Accounts WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapAccount(reader) : null;
        }

        public int SaveAccount(Account account)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();

            if (account.Id == 0)
            {
                command.CommandText = @"
                    INSERT INTO Accounts (AccountCode, AccountName, AccountNameEn, Type, ParentAccountId, Balance, IsActive, Description, CreatedAt)
                    VALUES (@code, @name, @nameEn, @type, @parentId, @balance, @isActive, @description, @createdAt);
                    SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("o"));
            }
            else
            {
                command.CommandText = @"
                    UPDATE Accounts SET 
                        AccountCode = @code, AccountName = @name, AccountNameEn = @nameEn, Type = @type,
                        ParentAccountId = @parentId, Balance = @balance, IsActive = @isActive, 
                        Description = @description, UpdatedAt = @updatedAt
                    WHERE Id = @id
                ";
                command.Parameters.AddWithValue("@id", account.Id);
                command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
            }

            command.Parameters.AddWithValue("@code", account.AccountCode);
            command.Parameters.AddWithValue("@name", account.AccountName);
            command.Parameters.AddWithValue("@nameEn", account.AccountNameEn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@type", (int)account.Type);
            command.Parameters.AddWithValue("@parentId", account.ParentAccountId.HasValue ? account.ParentAccountId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@balance", account.Balance);
            command.Parameters.AddWithValue("@isActive", account.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("@description", account.Description ?? (object)DBNull.Value);

            if (account.Id == 0)
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
            else
            {
                command.ExecuteNonQuery();
                return account.Id;
            }
        }

        public void DeleteAccount(int id)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Accounts WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        private Account MapAccount(SqliteDataReader reader)
        {
            return new Account
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                AccountNameEn = reader.IsDBNull(reader.GetOrdinal("AccountNameEn")) ? null : reader.GetString(reader.GetOrdinal("AccountNameEn")),
                Type = (AccountType)reader.GetInt32(reader.GetOrdinal("Type")),
                ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? null : reader.GetInt32(reader.GetOrdinal("ParentAccountId")),
                Balance = (decimal)reader.GetDouble(reader.GetOrdinal("Balance")),
                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
            };
        }

        #endregion

        #region Customer Operations

        public List<Customer> GetAllCustomers()
        {
            var customers = new List<Customer>();
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Customers ORDER BY Name";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                customers.Add(MapCustomer(reader));
            }
            return customers;
        }

        public Customer? GetCustomerById(int id)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Customers WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapCustomer(reader) : null;
        }

        public int SaveCustomer(Customer customer)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();

            if (customer.Id == 0)
            {
                command.CommandText = @"
                    INSERT INTO Customers (CustomerCode, Name, NameEn, Phone, Mobile, Email, Address, City, Country, 
                        TaxNumber, CommercialRegister, CreditLimit, Balance, AccountId, IsActive, Notes, CreatedAt)
                    VALUES (@code, @name, @nameEn, @phone, @mobile, @email, @address, @city, @country,
                        @taxNumber, @commercialRegister, @creditLimit, @balance, @accountId, @isActive, @notes, @createdAt);
                    SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("o"));
            }
            else
            {
                command.CommandText = @"
                    UPDATE Customers SET 
                        CustomerCode = @code, Name = @name, NameEn = @nameEn, Phone = @phone, Mobile = @mobile,
                        Email = @email, Address = @address, City = @city, Country = @country,
                        TaxNumber = @taxNumber, CommercialRegister = @commercialRegister, CreditLimit = @creditLimit,
                        Balance = @balance, AccountId = @accountId, IsActive = @isActive, Notes = @notes, UpdatedAt = @updatedAt
                    WHERE Id = @id
                ";
                command.Parameters.AddWithValue("@id", customer.Id);
                command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
            }

            command.Parameters.AddWithValue("@code", customer.CustomerCode);
            command.Parameters.AddWithValue("@name", customer.Name);
            command.Parameters.AddWithValue("@nameEn", customer.NameEn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@phone", customer.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@mobile", customer.Mobile ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@email", customer.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@address", customer.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@city", customer.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@country", customer.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@taxNumber", customer.TaxNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@commercialRegister", customer.CommercialRegister ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@creditLimit", customer.CreditLimit);
            command.Parameters.AddWithValue("@balance", customer.Balance);
            command.Parameters.AddWithValue("@accountId", customer.AccountId.HasValue ? customer.AccountId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@isActive", customer.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("@notes", customer.Notes ?? (object)DBNull.Value);

            if (customer.Id == 0)
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
            else
            {
                command.ExecuteNonQuery();
                return customer.Id;
            }
        }

        public void DeleteCustomer(int id)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Customers WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        private Customer MapCustomer(SqliteDataReader reader)
        {
            return new Customer
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CustomerCode = reader.GetString(reader.GetOrdinal("CustomerCode")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                NameEn = reader.IsDBNull(reader.GetOrdinal("NameEn")) ? null : reader.GetString(reader.GetOrdinal("NameEn")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Mobile = reader.IsDBNull(reader.GetOrdinal("Mobile")) ? null : reader.GetString(reader.GetOrdinal("Mobile")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                Country = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString(reader.GetOrdinal("Country")),
                TaxNumber = reader.IsDBNull(reader.GetOrdinal("TaxNumber")) ? null : reader.GetString(reader.GetOrdinal("TaxNumber")),
                CommercialRegister = reader.IsDBNull(reader.GetOrdinal("CommercialRegister")) ? null : reader.GetString(reader.GetOrdinal("CommercialRegister")),
                CreditLimit = (decimal)reader.GetDouble(reader.GetOrdinal("CreditLimit")),
                Balance = (decimal)reader.GetDouble(reader.GetOrdinal("Balance")),
                AccountId = reader.IsDBNull(reader.GetOrdinal("AccountId")) ? null : reader.GetInt32(reader.GetOrdinal("AccountId")),
                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
            };
        }

        #endregion

        #region Supplier Operations

        public List<Supplier> GetAllSuppliers()
        {
            var suppliers = new List<Supplier>();
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Suppliers ORDER BY Name";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                suppliers.Add(MapSupplier(reader));
            }
            return suppliers;
        }

        public Supplier? GetSupplierById(int id)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Suppliers WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapSupplier(reader) : null;
        }

        public int SaveSupplier(Supplier supplier)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();

            if (supplier.Id == 0)
            {
                command.CommandText = @"
                    INSERT INTO Suppliers (SupplierCode, Name, NameEn, Phone, Mobile, Email, Address, City, Country, 
                        TaxNumber, CommercialRegister, CreditLimit, Balance, AccountId, IsActive, Notes, CreatedAt)
                    VALUES (@code, @name, @nameEn, @phone, @mobile, @email, @address, @city, @country,
                        @taxNumber, @commercialRegister, @creditLimit, @balance, @accountId, @isActive, @notes, @createdAt);
                    SELECT last_insert_rowid();
                ";
                command.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("o"));
            }
            else
            {
                command.CommandText = @"
                    UPDATE Suppliers SET 
                        SupplierCode = @code, Name = @name, NameEn = @nameEn, Phone = @phone, Mobile = @mobile,
                        Email = @email, Address = @address, City = @city, Country = @country,
                        TaxNumber = @taxNumber, CommercialRegister = @commercialRegister, CreditLimit = @creditLimit,
                        Balance = @balance, AccountId = @accountId, IsActive = @isActive, Notes = @notes, UpdatedAt = @updatedAt
                    WHERE Id = @id
                ";
                command.Parameters.AddWithValue("@id", supplier.Id);
                command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
            }

            command.Parameters.AddWithValue("@code", supplier.SupplierCode);
            command.Parameters.AddWithValue("@name", supplier.Name);
            command.Parameters.AddWithValue("@nameEn", supplier.NameEn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@phone", supplier.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@mobile", supplier.Mobile ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@email", supplier.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@address", supplier.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@city", supplier.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@country", supplier.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@taxNumber", supplier.TaxNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@commercialRegister", supplier.CommercialRegister ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@creditLimit", supplier.CreditLimit);
            command.Parameters.AddWithValue("@balance", supplier.Balance);
            command.Parameters.AddWithValue("@accountId", supplier.AccountId.HasValue ? supplier.AccountId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@isActive", supplier.IsActive ? 1 : 0);
            command.Parameters.AddWithValue("@notes", supplier.Notes ?? (object)DBNull.Value);

            if (supplier.Id == 0)
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
            else
            {
                command.ExecuteNonQuery();
                return supplier.Id;
            }
        }

        public void DeleteSupplier(int id)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Suppliers WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        private Supplier MapSupplier(SqliteDataReader reader)
        {
            return new Supplier
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SupplierCode = reader.GetString(reader.GetOrdinal("SupplierCode")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                NameEn = reader.IsDBNull(reader.GetOrdinal("NameEn")) ? null : reader.GetString(reader.GetOrdinal("NameEn")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Mobile = reader.IsDBNull(reader.GetOrdinal("Mobile")) ? null : reader.GetString(reader.GetOrdinal("Mobile")),
                Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                Country = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString(reader.GetOrdinal("Country")),
                TaxNumber = reader.IsDBNull(reader.GetOrdinal("TaxNumber")) ? null : reader.GetString(reader.GetOrdinal("TaxNumber")),
                CommercialRegister = reader.IsDBNull(reader.GetOrdinal("CommercialRegister")) ? null : reader.GetString(reader.GetOrdinal("CommercialRegister")),
                CreditLimit = (decimal)reader.GetDouble(reader.GetOrdinal("CreditLimit")),
                Balance = (decimal)reader.GetDouble(reader.GetOrdinal("Balance")),
                AccountId = reader.IsDBNull(reader.GetOrdinal("AccountId")) ? null : reader.GetInt32(reader.GetOrdinal("AccountId")),
                IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
            };
        }

        #endregion

        #region Transaction Operations

        public List<Transaction> GetAllTransactions()
        {
            var transactions = new List<Transaction>();
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Transactions ORDER BY TransactionDate DESC, Id DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var transaction = MapTransaction(reader);
                transaction.Lines = GetTransactionLines(transaction.Id);
                transactions.Add(transaction);
            }
            return transactions;
        }

        public Transaction? GetTransactionById(int id)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Transactions WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var transaction = MapTransaction(reader);
                transaction.Lines = GetTransactionLines(id);
                return transaction;
            }
            return null;
        }

        public List<TransactionLine> GetTransactionLines(int transactionId)
        {
            var lines = new List<TransactionLine>();
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT tl.*, a.AccountName 
                FROM TransactionLines tl
                LEFT JOIN Accounts a ON tl.AccountId = a.Id
                WHERE tl.TransactionId = @transactionId
                ORDER BY tl.Id
            ";
            command.Parameters.AddWithValue("@transactionId", transactionId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lines.Add(new TransactionLine
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    TransactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
                    AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                    AccountName = reader.IsDBNull(reader.GetOrdinal("AccountName")) ? null : reader.GetString(reader.GetOrdinal("AccountName")),
                    Debit = (decimal)reader.GetDouble(reader.GetOrdinal("Debit")),
                    Credit = (decimal)reader.GetDouble(reader.GetOrdinal("Credit")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description"))
                });
            }
            return lines;
        }

        public int SaveTransaction(Transaction transaction)
        {
            using var connection = GetConnection();
            using var dbTransaction = connection.BeginTransaction();
            
            try
            {
                var command = connection.CreateCommand();
                command.Transaction = dbTransaction;

                if (transaction.Id == 0)
                {
                    // Generate transaction number
                    command.CommandText = "SELECT COALESCE(MAX(CAST(SUBSTR(TransactionNumber, 4) AS INTEGER)), 0) + 1 FROM Transactions";
                    var nextNum = Convert.ToInt32(command.ExecuteScalar());
                    transaction.TransactionNumber = $"JV-{nextNum:D6}";

                    command.CommandText = @"
                        INSERT INTO Transactions (TransactionNumber, TransactionDate, Description, Type, Status, Reference, 
                            CustomerId, SupplierId, CreatedAt, CreatedBy)
                        VALUES (@number, @date, @description, @type, @status, @reference, @customerId, @supplierId, @createdAt, @createdBy);
                        SELECT last_insert_rowid();
                    ";
                    command.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("o"));
                    command.Parameters.AddWithValue("@createdBy", Environment.UserName);
                }
                else
                {
                    command.CommandText = @"
                        UPDATE Transactions SET 
                            TransactionDate = @date, Description = @description, Type = @type, Status = @status,
                            Reference = @reference, CustomerId = @customerId, SupplierId = @supplierId,
                            ApprovedAt = @approvedAt, ApprovedBy = @approvedBy
                        WHERE Id = @id
                    ";
                    command.Parameters.AddWithValue("@id", transaction.Id);
                    command.Parameters.AddWithValue("@approvedAt", transaction.ApprovedAt.HasValue ? transaction.ApprovedAt.Value.ToString("o") : DBNull.Value);
                    command.Parameters.AddWithValue("@approvedBy", transaction.ApprovedBy ?? (object)DBNull.Value);
                }

                command.Parameters.AddWithValue("@number", transaction.TransactionNumber);
                command.Parameters.AddWithValue("@date", transaction.TransactionDate.ToString("o"));
                command.Parameters.AddWithValue("@description", transaction.Description);
                command.Parameters.AddWithValue("@type", (int)transaction.Type);
                command.Parameters.AddWithValue("@status", (int)transaction.Status);
                command.Parameters.AddWithValue("@reference", transaction.Reference ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@customerId", transaction.CustomerId.HasValue ? transaction.CustomerId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@supplierId", transaction.SupplierId.HasValue ? transaction.SupplierId.Value : DBNull.Value);

                int transactionId;
                if (transaction.Id == 0)
                {
                    transactionId = Convert.ToInt32(command.ExecuteScalar());
                }
                else
                {
                    command.ExecuteNonQuery();
                    transactionId = transaction.Id;

                    // Delete existing lines
                    command.CommandText = "DELETE FROM TransactionLines WHERE TransactionId = @transactionId";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@transactionId", transactionId);
                    command.ExecuteNonQuery();
                }

                // Insert lines
                foreach (var line in transaction.Lines)
                {
                    command.CommandText = @"
                        INSERT INTO TransactionLines (TransactionId, AccountId, Debit, Credit, Description)
                        VALUES (@transactionId, @accountId, @debit, @credit, @description)
                    ";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@transactionId", transactionId);
                    command.Parameters.AddWithValue("@accountId", line.AccountId);
                    command.Parameters.AddWithValue("@debit", line.Debit);
                    command.Parameters.AddWithValue("@credit", line.Credit);
                    command.Parameters.AddWithValue("@description", line.Description ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }

                dbTransaction.Commit();
                return transactionId;
            }
            catch
            {
                dbTransaction.Rollback();
                throw;
            }
        }

        public void DeleteTransaction(int id)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Transactions WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        public string GetNextTransactionNumber(TransactionType type)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            
            string prefix = type switch
            {
                TransactionType.JournalEntry => "JV",
                TransactionType.Receipt => "RV",
                TransactionType.Payment => "PV",
                TransactionType.SalesInvoice => "SI",
                TransactionType.PurchaseInvoice => "PI",
                _ => "TX"
            };

            command.CommandText = $"SELECT COALESCE(MAX(CAST(SUBSTR(TransactionNumber, {prefix.Length + 2}) AS INTEGER)), 0) + 1 FROM Transactions WHERE TransactionNumber LIKE @prefix || '%'";
            command.Parameters.AddWithValue("@prefix", prefix);
            var nextNum = Convert.ToInt32(command.ExecuteScalar());
            return $"{prefix}-{nextNum:D6}";
        }

        private Transaction MapTransaction(SqliteDataReader reader)
        {
            return new Transaction
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                TransactionNumber = reader.GetString(reader.GetOrdinal("TransactionNumber")),
                TransactionDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("TransactionDate"))),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Type = (TransactionType)reader.GetInt32(reader.GetOrdinal("Type")),
                Status = (TransactionStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                Reference = reader.IsDBNull(reader.GetOrdinal("Reference")) ? null : reader.GetString(reader.GetOrdinal("Reference")),
                CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId")) ? null : reader.GetInt32(reader.GetOrdinal("CustomerId")),
                SupplierId = reader.IsDBNull(reader.GetOrdinal("SupplierId")) ? null : reader.GetInt32(reader.GetOrdinal("SupplierId")),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                ApprovedAt = reader.IsDBNull(reader.GetOrdinal("ApprovedAt")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("ApprovedAt"))),
                ApprovedBy = reader.IsDBNull(reader.GetOrdinal("ApprovedBy")) ? null : reader.GetString(reader.GetOrdinal("ApprovedBy"))
            };
        }

        #endregion

        #region Invoice Operations

        public List<Invoice> GetAllInvoices(InvoiceType? type = null)
        {
            var invoices = new List<Invoice>();
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            
            var sql = @"
                SELECT i.*, c.Name as CustomerName, s.Name as SupplierName 
                FROM Invoices i
                LEFT JOIN Customers c ON i.CustomerId = c.Id
                LEFT JOIN Suppliers s ON i.SupplierId = s.Id
            ";
            
            if (type.HasValue)
            {
                sql += " WHERE i.Type = @type";
                command.Parameters.AddWithValue("@type", (int)type.Value);
            }
            
            sql += " ORDER BY i.InvoiceDate DESC, i.Id DESC";
            command.CommandText = sql;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var invoice = MapInvoice(reader);
                invoice.Lines = GetInvoiceLines(invoice.Id);
                invoices.Add(invoice);
            }
            return invoices;
        }

        public Invoice? GetInvoiceById(int id)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT i.*, c.Name as CustomerName, s.Name as SupplierName 
                FROM Invoices i
                LEFT JOIN Customers c ON i.CustomerId = c.Id
                LEFT JOIN Suppliers s ON i.SupplierId = s.Id
                WHERE i.Id = @id
            ";
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var invoice = MapInvoice(reader);
                invoice.Lines = GetInvoiceLines(id);
                return invoice;
            }
            return null;
        }

        public List<InvoiceLine> GetInvoiceLines(int invoiceId)
        {
            var lines = new List<InvoiceLine>();
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM InvoiceLines WHERE InvoiceId = @invoiceId ORDER BY LineNumber";
            command.Parameters.AddWithValue("@invoiceId", invoiceId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lines.Add(new InvoiceLine
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    InvoiceId = reader.GetInt32(reader.GetOrdinal("InvoiceId")),
                    LineNumber = reader.GetInt32(reader.GetOrdinal("LineNumber")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    ItemCode = reader.IsDBNull(reader.GetOrdinal("ItemCode")) ? null : reader.GetString(reader.GetOrdinal("ItemCode")),
                    Quantity = (decimal)reader.GetDouble(reader.GetOrdinal("Quantity")),
                    Unit = reader.GetString(reader.GetOrdinal("Unit")),
                    UnitPrice = (decimal)reader.GetDouble(reader.GetOrdinal("UnitPrice")),
                    DiscountPercent = (decimal)reader.GetDouble(reader.GetOrdinal("DiscountPercent")),
                    DiscountAmount = (decimal)reader.GetDouble(reader.GetOrdinal("DiscountAmount")),
                    TaxPercent = (decimal)reader.GetDouble(reader.GetOrdinal("TaxPercent")),
                    TaxAmount = (decimal)reader.GetDouble(reader.GetOrdinal("TaxAmount")),
                    Total = (decimal)reader.GetDouble(reader.GetOrdinal("Total")),
                    AccountId = reader.IsDBNull(reader.GetOrdinal("AccountId")) ? null : reader.GetInt32(reader.GetOrdinal("AccountId"))
                });
            }
            return lines;
        }

        public int SaveInvoice(Invoice invoice)
        {
            using var connection = GetConnection();
            using var dbTransaction = connection.BeginTransaction();
            
            try
            {
                var command = connection.CreateCommand();
                command.Transaction = dbTransaction;

                if (invoice.Id == 0)
                {
                    // Generate invoice number
                    string prefix = invoice.Type switch
                    {
                        InvoiceType.Sales => "SI",
                        InvoiceType.Purchase => "PI",
                        InvoiceType.SalesReturn => "SR",
                        InvoiceType.PurchaseReturn => "PR",
                        _ => "INV"
                    };

                    command.CommandText = $"SELECT COALESCE(MAX(CAST(SUBSTR(InvoiceNumber, {prefix.Length + 2}) AS INTEGER)), 0) + 1 FROM Invoices WHERE InvoiceNumber LIKE @prefix || '%'";
                    command.Parameters.AddWithValue("@prefix", prefix);
                    var nextNum = Convert.ToInt32(command.ExecuteScalar());
                    invoice.InvoiceNumber = $"{prefix}-{nextNum:D6}";

                    command.CommandText = @"
                        INSERT INTO Invoices (InvoiceNumber, Type, InvoiceDate, DueDate, CustomerId, SupplierId,
                            SubTotal, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, Total, PaidAmount,
                            Status, Notes, TransactionId, CreatedAt, CreatedBy)
                        VALUES (@number, @type, @date, @dueDate, @customerId, @supplierId,
                            @subTotal, @discountPercent, @discountAmount, @taxPercent, @taxAmount, @total, @paidAmount,
                            @status, @notes, @transactionId, @createdAt, @createdBy);
                        SELECT last_insert_rowid();
                    ";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@createdAt", DateTime.Now.ToString("o"));
                    command.Parameters.AddWithValue("@createdBy", Environment.UserName);
                }
                else
                {
                    command.CommandText = @"
                        UPDATE Invoices SET 
                            InvoiceDate = @date, DueDate = @dueDate, CustomerId = @customerId, SupplierId = @supplierId,
                            SubTotal = @subTotal, DiscountPercent = @discountPercent, DiscountAmount = @discountAmount,
                            TaxPercent = @taxPercent, TaxAmount = @taxAmount, Total = @total, PaidAmount = @paidAmount,
                            Status = @status, Notes = @notes, TransactionId = @transactionId, UpdatedAt = @updatedAt
                        WHERE Id = @id
                    ";
                    command.Parameters.AddWithValue("@id", invoice.Id);
                    command.Parameters.AddWithValue("@updatedAt", DateTime.Now.ToString("o"));
                }

                command.Parameters.AddWithValue("@number", invoice.InvoiceNumber);
                command.Parameters.AddWithValue("@type", (int)invoice.Type);
                command.Parameters.AddWithValue("@date", invoice.InvoiceDate.ToString("o"));
                command.Parameters.AddWithValue("@dueDate", invoice.DueDate.HasValue ? invoice.DueDate.Value.ToString("o") : DBNull.Value);
                command.Parameters.AddWithValue("@customerId", invoice.CustomerId.HasValue ? invoice.CustomerId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@supplierId", invoice.SupplierId.HasValue ? invoice.SupplierId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@subTotal", invoice.SubTotal);
                command.Parameters.AddWithValue("@discountPercent", invoice.DiscountPercent);
                command.Parameters.AddWithValue("@discountAmount", invoice.DiscountAmount);
                command.Parameters.AddWithValue("@taxPercent", invoice.TaxPercent);
                command.Parameters.AddWithValue("@taxAmount", invoice.TaxAmount);
                command.Parameters.AddWithValue("@total", invoice.Total);
                command.Parameters.AddWithValue("@paidAmount", invoice.PaidAmount);
                command.Parameters.AddWithValue("@status", (int)invoice.Status);
                command.Parameters.AddWithValue("@notes", invoice.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@transactionId", invoice.TransactionId.HasValue ? invoice.TransactionId.Value : DBNull.Value);

                int invoiceId;
                if (invoice.Id == 0)
                {
                    invoiceId = Convert.ToInt32(command.ExecuteScalar());
                }
                else
                {
                    command.ExecuteNonQuery();
                    invoiceId = invoice.Id;

                    // Delete existing lines
                    command.CommandText = "DELETE FROM InvoiceLines WHERE InvoiceId = @invoiceId";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@invoiceId", invoiceId);
                    command.ExecuteNonQuery();
                }

                // Insert lines
                int lineNum = 1;
                foreach (var line in invoice.Lines)
                {
                    command.CommandText = @"
                        INSERT INTO InvoiceLines (InvoiceId, LineNumber, Description, ItemCode, Quantity, Unit,
                            UnitPrice, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, Total, AccountId)
                        VALUES (@invoiceId, @lineNumber, @description, @itemCode, @quantity, @unit,
                            @unitPrice, @discountPercent, @discountAmount, @taxPercent, @taxAmount, @total, @accountId)
                    ";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@invoiceId", invoiceId);
                    command.Parameters.AddWithValue("@lineNumber", lineNum++);
                    command.Parameters.AddWithValue("@description", line.Description);
                    command.Parameters.AddWithValue("@itemCode", line.ItemCode ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@quantity", line.Quantity);
                    command.Parameters.AddWithValue("@unit", line.Unit);
                    command.Parameters.AddWithValue("@unitPrice", line.UnitPrice);
                    command.Parameters.AddWithValue("@discountPercent", line.DiscountPercent);
                    command.Parameters.AddWithValue("@discountAmount", line.DiscountAmount);
                    command.Parameters.AddWithValue("@taxPercent", line.TaxPercent);
                    command.Parameters.AddWithValue("@taxAmount", line.TaxAmount);
                    command.Parameters.AddWithValue("@total", line.Total);
                    command.Parameters.AddWithValue("@accountId", line.AccountId.HasValue ? line.AccountId.Value : DBNull.Value);
                    command.ExecuteNonQuery();
                }

                dbTransaction.Commit();
                return invoiceId;
            }
            catch
            {
                dbTransaction.Rollback();
                throw;
            }
        }

        public void DeleteInvoice(int id)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Invoices WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        private Invoice MapInvoice(SqliteDataReader reader)
        {
            return new Invoice
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                InvoiceNumber = reader.GetString(reader.GetOrdinal("InvoiceNumber")),
                Type = (InvoiceType)reader.GetInt32(reader.GetOrdinal("Type")),
                InvoiceDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("InvoiceDate"))),
                DueDate = reader.IsDBNull(reader.GetOrdinal("DueDate")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("DueDate"))),
                CustomerId = reader.IsDBNull(reader.GetOrdinal("CustomerId")) ? null : reader.GetInt32(reader.GetOrdinal("CustomerId")),
                CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName")),
                SupplierId = reader.IsDBNull(reader.GetOrdinal("SupplierId")) ? null : reader.GetInt32(reader.GetOrdinal("SupplierId")),
                SupplierName = reader.IsDBNull(reader.GetOrdinal("SupplierName")) ? null : reader.GetString(reader.GetOrdinal("SupplierName")),
                SubTotal = (decimal)reader.GetDouble(reader.GetOrdinal("SubTotal")),
                DiscountPercent = (decimal)reader.GetDouble(reader.GetOrdinal("DiscountPercent")),
                DiscountAmount = (decimal)reader.GetDouble(reader.GetOrdinal("DiscountAmount")),
                TaxPercent = (decimal)reader.GetDouble(reader.GetOrdinal("TaxPercent")),
                TaxAmount = (decimal)reader.GetDouble(reader.GetOrdinal("TaxAmount")),
                Total = (decimal)reader.GetDouble(reader.GetOrdinal("Total")),
                PaidAmount = (decimal)reader.GetDouble(reader.GetOrdinal("PaidAmount")),
                Status = (InvoiceStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                TransactionId = reader.IsDBNull(reader.GetOrdinal("TransactionId")) ? null : reader.GetInt32(reader.GetOrdinal("TransactionId")),
                CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
            };
        }

        #endregion

        #region Settings Operations

        public SystemSettings GetSettings()
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Settings LIMIT 1";

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new SystemSettings
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                    CompanyNameEn = reader.IsDBNull(reader.GetOrdinal("CompanyNameEn")) ? null : reader.GetString(reader.GetOrdinal("CompanyNameEn")),
                    CompanyLogo = reader.IsDBNull(reader.GetOrdinal("CompanyLogo")) ? null : reader.GetString(reader.GetOrdinal("CompanyLogo")),
                    Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                    City = reader.IsDBNull(reader.GetOrdinal("City")) ? null : reader.GetString(reader.GetOrdinal("City")),
                    Country = reader.IsDBNull(reader.GetOrdinal("Country")) ? null : reader.GetString(reader.GetOrdinal("Country")),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                    Mobile = reader.IsDBNull(reader.GetOrdinal("Mobile")) ? null : reader.GetString(reader.GetOrdinal("Mobile")),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    Website = reader.IsDBNull(reader.GetOrdinal("Website")) ? null : reader.GetString(reader.GetOrdinal("Website")),
                    TaxNumber = reader.IsDBNull(reader.GetOrdinal("TaxNumber")) ? null : reader.GetString(reader.GetOrdinal("TaxNumber")),
                    CommercialRegister = reader.IsDBNull(reader.GetOrdinal("CommercialRegister")) ? null : reader.GetString(reader.GetOrdinal("CommercialRegister")),
                    Currency = reader.GetString(reader.GetOrdinal("Currency")),
                    CurrencyCode = reader.GetString(reader.GetOrdinal("CurrencyCode")),
                    DefaultTaxRate = (decimal)reader.GetDouble(reader.GetOrdinal("DefaultTaxRate")),
                    FiscalYearStart = reader.GetInt32(reader.GetOrdinal("FiscalYearStart")),
                    DateFormat = reader.GetString(reader.GetOrdinal("DateFormat")),
                    UseHijriDate = reader.GetInt32(reader.GetOrdinal("UseHijriDate")) == 1,
                    LastBackup = reader.IsDBNull(reader.GetOrdinal("LastBackup")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("LastBackup")))
                };
            }
            return new SystemSettings();
        }

        public void SaveSettings(SystemSettings settings)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Settings SET 
                    CompanyName = @companyName, CompanyNameEn = @companyNameEn, CompanyLogo = @companyLogo,
                    Address = @address, City = @city, Country = @country, Phone = @phone, Mobile = @mobile,
                    Email = @email, Website = @website, TaxNumber = @taxNumber, CommercialRegister = @commercialRegister,
                    Currency = @currency, CurrencyCode = @currencyCode, DefaultTaxRate = @defaultTaxRate,
                    FiscalYearStart = @fiscalYearStart, DateFormat = @dateFormat, UseHijriDate = @useHijriDate,
                    LastBackup = @lastBackup
                WHERE Id = @id
            ";

            command.Parameters.AddWithValue("@id", settings.Id);
            command.Parameters.AddWithValue("@companyName", settings.CompanyName);
            command.Parameters.AddWithValue("@companyNameEn", settings.CompanyNameEn ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@companyLogo", settings.CompanyLogo ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@address", settings.Address ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@city", settings.City ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@country", settings.Country ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@phone", settings.Phone ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@mobile", settings.Mobile ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@email", settings.Email ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@website", settings.Website ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@taxNumber", settings.TaxNumber ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@commercialRegister", settings.CommercialRegister ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@currency", settings.Currency);
            command.Parameters.AddWithValue("@currencyCode", settings.CurrencyCode);
            command.Parameters.AddWithValue("@defaultTaxRate", settings.DefaultTaxRate);
            command.Parameters.AddWithValue("@fiscalYearStart", settings.FiscalYearStart);
            command.Parameters.AddWithValue("@dateFormat", settings.DateFormat);
            command.Parameters.AddWithValue("@useHijriDate", settings.UseHijriDate ? 1 : 0);
            command.Parameters.AddWithValue("@lastBackup", settings.LastBackup.HasValue ? settings.LastBackup.Value.ToString("o") : DBNull.Value);

            command.ExecuteNonQuery();
        }

        #endregion

        #region Report Operations

        /// <summary>
        /// الحصول على ميزان المراجعة
        /// </summary>
        public List<(Account Account, decimal Debit, decimal Credit)> GetTrialBalance(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var result = new List<(Account Account, decimal Debit, decimal Credit)>();
            using var connection = GetConnection();
            var command = connection.CreateCommand();

            var sql = @"
                SELECT a.*, 
                    COALESCE(SUM(tl.Debit), 0) as TotalDebit,
                    COALESCE(SUM(tl.Credit), 0) as TotalCredit
                FROM Accounts a
                LEFT JOIN TransactionLines tl ON a.Id = tl.AccountId
                LEFT JOIN Transactions t ON tl.TransactionId = t.Id
                WHERE (t.Status = @status OR t.Id IS NULL)
            ";

            if (fromDate.HasValue)
            {
                sql += " AND (t.TransactionDate >= @fromDate OR t.Id IS NULL)";
                command.Parameters.AddWithValue("@fromDate", fromDate.Value.ToString("o"));
            }
            if (toDate.HasValue)
            {
                sql += " AND (t.TransactionDate <= @toDate OR t.Id IS NULL)";
                command.Parameters.AddWithValue("@toDate", toDate.Value.ToString("o"));
            }

            sql += " GROUP BY a.Id ORDER BY a.AccountCode";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@status", (int)TransactionStatus.Posted);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var account = MapAccount(reader);
                var debit = (decimal)reader.GetDouble(reader.GetOrdinal("TotalDebit"));
                var credit = (decimal)reader.GetDouble(reader.GetOrdinal("TotalCredit"));
                result.Add((account, debit, credit));
            }
            return result;
        }

        /// <summary>
        /// الحصول على قائمة الدخل
        /// </summary>
        public (decimal Revenue, decimal Expenses, decimal NetIncome) GetIncomeStatement(DateTime? fromDate = null, DateTime? toDate = null)
        {
            using var connection = GetConnection();
            var command = connection.CreateCommand();

            var sql = @"
                SELECT a.Type,
                    COALESCE(SUM(tl.Credit) - SUM(tl.Debit), 0) as Net
                FROM Accounts a
                JOIN TransactionLines tl ON a.Id = tl.AccountId
                JOIN Transactions t ON tl.TransactionId = t.Id
                WHERE t.Status = @status AND a.Type IN (@revenue, @expenses)
            ";

            if (fromDate.HasValue)
            {
                sql += " AND t.TransactionDate >= @fromDate";
                command.Parameters.AddWithValue("@fromDate", fromDate.Value.ToString("o"));
            }
            if (toDate.HasValue)
            {
                sql += " AND t.TransactionDate <= @toDate";
                command.Parameters.AddWithValue("@toDate", toDate.Value.ToString("o"));
            }

            sql += " GROUP BY a.Type";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@status", (int)TransactionStatus.Posted);
            command.Parameters.AddWithValue("@revenue", (int)AccountType.Revenue);
            command.Parameters.AddWithValue("@expenses", (int)AccountType.Expenses);

            decimal revenue = 0, expenses = 0;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var type = (AccountType)reader.GetInt32(reader.GetOrdinal("Type"));
                var net = (decimal)reader.GetDouble(reader.GetOrdinal("Net"));
                
                if (type == AccountType.Revenue)
                    revenue = net;
                else if (type == AccountType.Expenses)
                    expenses = -net;
            }

            return (revenue, expenses, revenue - expenses);
        }

        #endregion
    }
}
