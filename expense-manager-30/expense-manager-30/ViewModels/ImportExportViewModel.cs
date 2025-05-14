using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using expense_manager_30.Models;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;

public partial class ImportExportViewModel : ViewModelBase
{
    private readonly DbService _database = new();

    [ObservableProperty] private string _statusMessage = string.Empty;

    public async Task ImportFromFileAsync(IStorageFile file)
    {
        try
        {
            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            var transactionsToImport = JsonSerializer.Deserialize<List<Transaction>>(content);
            if (transactionsToImport is null)
            {
                StatusMessage = "Invalid format";
                return;
            }

            var existingTransactions = _database.GetTransactions(Session.CurrentUserId);

            var importedCount = 0;
            foreach (var t in from t in transactionsToImport
                     let alreadyExists = existingTransactions.Exists(e =>
                         e.Amount == t.Amount &&
                         e.Date == t.Date &&
                         e.Note == t.Note &&
                         e.CategoryId == t.CategoryId &&
                         e.IsIncome == t.IsIncome)
                     where !alreadyExists
                     select t)
            {
                _database.AddTransaction(
                    t.Amount,
                    t.IsIncome,
                    t.Date,
                    t.Note,
                    t.CategoryId,
                    Session.CurrentUserId);
                importedCount++;
            }

            StatusMessage = $"Imported {importedCount} new transaction(s).";
        }
        catch (JsonException ex)
        {
            StatusMessage =
                $"Error on line {ex.LineNumber}, more specifically: {ex.Message[..Math.Min(ex.Message.Length, 100)]}";
        }
    }

    public async Task ExportToFileAsync(IStorageFile file)
    {
        try
        {
            var transactions = _database.GetTransactions(Session.CurrentUserId);
            var json = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });

            await using var stream = await file.OpenWriteAsync();
            await using var writer = new StreamWriter(stream);
            await writer.WriteAsync(json);

            StatusMessage = "Successfully exported transactions.";
        }
        catch (JsonException ex)
        {
            StatusMessage = $"Error exporting, more specifically: {ex.Message[..Math.Min(ex.Message.Length, 100)]}";
        }
    }
}