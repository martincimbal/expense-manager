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
        if (File.Exists(DbFilePath)) return;
        SQLiteConnection.CreateFile(DbFilePath);
        CreateTables();
    }
    
    private static void CreateTables()
    {
        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        const string createUserTableQuery = @"
                    CREATE TABLE Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        Salt TEXT NOT NULL
                    );
                ";

        const string createCategoryTableQuery = @"
                    CREATE TABLE Categories (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        UserId INTEGER,
                        FOREIGN KEY(UserId) REFERENCES Users(Id)
                    );
                ";

        const string createTransactionTableQuery = @"
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


    private (string passwordHash, string salt) HashPassword(string password)
    {
        var salt = Guid.NewGuid().ToString();

        var saltedPassword = salt + password;
        var passwordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(saltedPassword)));

        return (passwordHash, salt);
    }

    public void RegisterUser(string username, string password)
    {
        var (passwordHash, salt) = HashPassword(password);

        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        const string query = "INSERT INTO Users (Username, PasswordHash, Salt) VALUES (@Username, @PasswordHash, @Salt)";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);
        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
        command.Parameters.AddWithValue("@Salt", salt);

        command.ExecuteNonQuery();
    }
    
    public static bool LoginUser(string username, string password)
    {
        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        const string query = "SELECT PasswordHash, Salt FROM Users WHERE Username = @Username";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return false;
        var storedHash = reader.GetString(0);
        var storedSalt = reader.GetString(1);

        var saltedPassword = storedSalt + password;
        var passwordHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(saltedPassword)));

        return storedHash == passwordHash;
    }

    public static void AddCategory(string categoryName, int? userId)
    {
        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        const string query = "INSERT INTO Categories (Name, UserId) VALUES (@Name, @UserId)";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Name", categoryName);
        command.Parameters.AddWithValue("@UserId", userId ?? (object)DBNull.Value);

        command.ExecuteNonQuery();
    }
    
    public static void AddTransaction(decimal amount, bool isIncome, DateTime date, string? note, int categoryId, int userId)
    {
        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        const string query = "INSERT INTO Transactions (Amount, IsIncome, Date, Note, CategoryId, UserId) " +
                             "VALUES (@Amount, @IsIncome, @Date, @Note, @CategoryId, @UserId)";

        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@IsIncome", isIncome ? 1 : 0);
        command.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
        command.Parameters.AddWithValue("@Note", note ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CategoryId", categoryId);
        command.Parameters.AddWithValue("@UserId", userId);

        command.ExecuteNonQuery();
    }
    
    public static List<Transaction> GetTransactions(int userId)
    {
        var transactions = new List<Transaction>();

        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        const string query = "SELECT * FROM Transactions WHERE UserId = @UserId";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = command.ExecuteReader();
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

        return transactions;
    }

    public static List<Category> GetCategories(int userId)
    {
        var categories = new List<Category>();

        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        const string query = "SELECT * FROM Categories WHERE UserId = @UserId";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                UserId = reader.GetInt32(2)
            });
        }
        
        return categories;
    }

    public static List<Transaction> GetFilteredTransactions(
            int userId,
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool? isIncomeOnly = null,
            string? noteContains = null)
    {
        var transactions = new List<Transaction>();

        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        var query = "SELECT * FROM Transactions WHERE UserId = @UserId";
        var parameters = new List<SQLiteParameter>
        {
            new SQLiteParameter("@UserId", userId)
        };

        if (categoryId.HasValue)
        {
            query += " AND CategoryId = @CategoryId";
            parameters.Add(new SQLiteParameter("@CategoryId", categoryId.Value));
        }

        if (startDate.HasValue)
        {
            query += " AND Date >= @StartDate";
            parameters.Add(new SQLiteParameter("@StartDate", startDate.Value.ToString("yyyy-MM-dd")));
        }

        if (endDate.HasValue)
        {
            query += " AND Date <= @EndDate";
            parameters.Add(new SQLiteParameter("@EndDate", endDate.Value.ToString("yyyy-MM-dd")));
        }

        if (isIncomeOnly.HasValue)
        {
            query += " AND IsIncome = @IsIncome";
            parameters.Add(new SQLiteParameter("@IsIncome", isIncomeOnly.Value ? 1 : 0));
        }

        if (!string.IsNullOrWhiteSpace(noteContains))
        {
            query += " AND Note LIKE @Note";
            parameters.Add(new SQLiteParameter("@Note", $"%{noteContains}%"));
        }

        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddRange(parameters.ToArray());

        using var reader = command.ExecuteReader();
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
        return transactions;
    }



    public (decimal totalIncome, decimal totalExpenses) GetBalance(int userId)
    {
        decimal totalIncome = 0;
        decimal totalExpenses = 0;

        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        const string query = "SELECT Amount, IsIncome FROM Transactions WHERE UserId = @UserId";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", userId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var amount = reader.GetDecimal(0);
            var isIncome = reader.GetInt32(1) == 1;

            if (isIncome)
                totalIncome += amount;
            else
                totalExpenses += amount;
        }

        return (totalIncome, totalExpenses);
    }
    
    public static int? GetUserIdByUsername(string username)
    {
        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();

        const string query = "SELECT Id FROM Users WHERE Username = @Username";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Username", username);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetInt32(0);
        }

        return null;
    }
    public static void DeleteTransaction(int transactionId)
    {
        using var connection = new SQLiteConnection($"Data Source={DbFilePath};");
        connection.Open();
        const string query = "DELETE FROM Transactions WHERE Id = @Id";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@Id", transactionId);
        command.ExecuteNonQuery();
    }

    public static void UpdateTransaction(Transaction transaction)
    {
        using var conn = new SQLiteConnection($"Data Source={DbFilePath};");
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Transactions
            SET Amount = @amount,
                Date = @date,
                Note = @note,
                CategoryId = @categoryId,
                IsIncome = @isIncome
            WHERE Id = @id
        ";
        cmd.Parameters.AddWithValue("@amount", transaction.Amount);
        cmd.Parameters.AddWithValue("@date", transaction.Date);
        cmd.Parameters.AddWithValue("@note", transaction.Note ?? "");
        cmd.Parameters.AddWithValue("@categoryId", transaction.CategoryId);
        cmd.Parameters.AddWithValue("@isIncome", transaction.IsIncome ? 1 : 0);
        cmd.Parameters.AddWithValue("@id", transaction.Id);

        cmd.ExecuteNonQuery();
    }

}