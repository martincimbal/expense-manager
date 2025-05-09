using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using expense_manager_30.Models;

namespace expense_manager_30.Services;

public class DbService
{
    private const string DbFilePath = "expense_manager.db";

    public DbService()
    {
        if (!File.Exists(DbFilePath))
        {
            SQLiteConnection.CreateFile(DbFilePath);
            CreateTables();
        }
    }
    
    private void CreateTables()
    {
        using (var connection = new SQLiteConnection($"Data Source={DbFilePath};"))
        {
            connection.Open();

            string createUserTableQuery = @"
                    CREATE TABLE Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        Salt TEXT NOT NULL
                    );
                ";

            string createCategoryTableQuery = @"
                    CREATE TABLE Categories (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        UserId INTEGER,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    );
                ";

            string createTransactionTableQuery = @"
                    CREATE TABLE Transactions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Amount REAL NOT NULL,
                        IsIncome INTEGER NOT NULL,
                        Date TEXT NOT NULL,
                        Note TEXT,
                        CategoryId INTEGER,
                        UserId INTEGER,
                        FOREIGN KEY(CategoryId) REFERENCES Categories(Id),
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    );
                ";

            using (var command = new SQLiteCommand(createUserTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createCategoryTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            using (var command = new SQLiteCommand(createTransactionTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }


    private (string passwordHash, string salt) HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var salt = Guid.NewGuid().ToString();
            
            var saltedPassword = salt + password;
            var passwordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword)));

            return (passwordHash, salt);
        }
    }
    
    public void RegisterUser(string username, string password)
    {
        var (passwordHash, salt) = HashPassword(password);

        using (var connection = new SQLiteConnection($"Data Source={DbFilePath};"))
        {
            connection.Open();

            string query = "INSERT INTO Users (Username, PasswordHash, Salt) VALUES (@Username, @PasswordHash, @Salt)";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@Salt", salt);

                command.ExecuteNonQuery();
            }
        }
    }
    
    public bool LoginUser(string username, string password)
    {
        using (var connection = new SQLiteConnection($"Data Source={DbFilePath};"))
        {
            connection.Open();

            string query = "SELECT PasswordHash, Salt FROM Users WHERE Username = @Username";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Username", username);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var storedHash = reader.GetString(0);
                        var storedSalt = reader.GetString(1);

                        // Použít stejný způsob hashování jako při registraci
                        using (var sha256 = SHA256.Create())
                        {
                            var saltedPassword = storedSalt + password;
                            var passwordHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword)));

                            return storedHash == passwordHash;
                        }
                    }
                }
            }
        }

        return false;
    }
    
    public void AddCategory(string categoryName, int? userId)
    {
        using (var connection = new SQLiteConnection($"Data Source={DbFilePath};"))
        {
            connection.Open();

            string query = "INSERT INTO Categories (Name, UserId) VALUES (@Name, @UserId)";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", categoryName);
                command.Parameters.AddWithValue("@UserId", userId ?? (object)DBNull.Value);

                command.ExecuteNonQuery();
            }
        }
    }
    
    public void AddTransaction(decimal amount, bool isIncome, DateTime date, string? note, int categoryId, int userId)
    {
        using (var connection = new SQLiteConnection($"Data Source={DbFilePath};"))
        {
            connection.Open();

            string query = "INSERT INTO Transactions (Amount, IsIncome, Date, Note, CategoryId, UserId) " +
                           "VALUES (@Amount, @IsIncome, @Date, @Note, @CategoryId, @UserId)";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@IsIncome", isIncome ? 1 : 0);
                command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@Note", note ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CategoryId", categoryId);
                command.Parameters.AddWithValue("@UserId", userId);

                command.ExecuteNonQuery();
            }
        }
    }
    
    public List<Transaction> GetTransactions(int userId)
    {
        var transactions = new List<Transaction>();

        using (var connection = new SQLiteConnection($"Data Source={DbFilePath};"))
        {
            connection.Open();

            string query = "SELECT * FROM Transactions WHERE UserId = @UserId";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = reader.GetInt32(0),
                            Amount = reader.GetDecimal(1),
                            IsIncome = reader.GetInt32(2) == 1,
                            Date = DateTime.Parse(reader.GetString(3)),
                            Note = reader.IsDBNull(4) ? null : reader.GetString(4),
                            CategoryId = reader.GetInt32(5),
                            UserId = reader.GetInt32(6)
                        });
                    }
                }
            }
        }

        return transactions;
    }

    public List<Category> GetCategories(int userId)
    {
        var categories = new List<Category>();

        using (var connection = new SQLiteConnection($"Data Source={DbFilePath};"))
        {
            connection.Open();

            string query = "SELECT * FROM Categories WHERE UserId = @UserId";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            UserId = reader.GetInt32(2)
                        });
                    }
                }
            }
        }
        return categories;
    }
    
    public List<Transaction> GetFilteredTransactions(int userId, int? categoryId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var transactions = new List<Transaction>();

        using (var connection = new SQLiteConnection($"Data Source={DbFilePath};"))
        {
            connection.Open();

            string query = "SELECT * FROM Transactions WHERE UserId = @UserId";
            
            if (categoryId.HasValue)
                query += " AND CategoryId = @CategoryId";

            if (startDate.HasValue)
                query += " AND Date >= @StartDate";

            if (endDate.HasValue)
                query += " AND Date <= @EndDate";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                if (categoryId.HasValue)
                    command.Parameters.AddWithValue("@CategoryId", categoryId.Value);

                if (startDate.HasValue)
                    command.Parameters.AddWithValue("@StartDate", startDate.Value.ToString("yyyy-MM-dd"));

                if (endDate.HasValue)
                    command.Parameters.AddWithValue("@EndDate", endDate.Value.ToString("yyyy-MM-dd"));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transactions.Add(new Transaction
                        {
                            Id = reader.GetInt32(0),
                            Amount = reader.GetDecimal(1),
                            IsIncome = reader.GetInt32(2) == 1,
                            Date = DateTime.Parse(reader.GetString(3)),
                            Note = reader.IsDBNull(4) ? null : reader.GetString(4),
                            CategoryId = reader.GetInt32(5),
                            UserId = reader.GetInt32(6)
                        });
                    }
                }
            }
        }

        return transactions;
    }
    
    public (decimal totalIncome, decimal totalExpenses) GetBalance(int userId)
    {
        decimal totalIncome = 0;
        decimal totalExpenses = 0;

        using (var connection = new SQLiteConnection($"Data Source={DbFilePath};"))
        {
            connection.Open();

            string query = "SELECT Amount, IsIncome FROM Transactions WHERE UserId = @UserId";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        decimal amount = reader.GetDecimal(0);
                        bool isIncome = reader.GetInt32(1) == 1;

                        if (isIncome)
                            totalIncome += amount;
                        else
                            totalExpenses += amount;
                    }
                }
            }
        }

        return (totalIncome, totalExpenses);
    }
    
    public int? GetUserIdByUsername(string username)
    {
        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        string query = "SELECT Id FROM Users WHERE Username = @Username";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetInt32(0);
        }

        return null;
    }


}