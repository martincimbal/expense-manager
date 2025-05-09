using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Models;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;


public partial class TransactionListViewModel : ViewModelBase
{
    private readonly DbService _dbService;

    [ObservableProperty]
    private ObservableCollection<Transaction> _transactions = new();

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public TransactionListViewModel()
    {
        _dbService = new DbService();
        LoadTransactions();
    }

    [RelayCommand]
    private void LoadTransactions()
    {
        if (!Session.IsLoggedIn)
        {
            StatusMessage = "User not logged in.";
            Transactions.Clear();
            return;
        }

        var userTransactions = _dbService.GetTransactions(Session.CurrentUserId);
        var categories = _dbService.GetCategories(Session.CurrentUserId);
        
        foreach (var transaction in userTransactions)
        {
            var category = categories.FirstOrDefault(c => c.Id == transaction.CategoryId);
            transaction.CategoryName = category?.Name ?? "Unknown";
        }

        Transactions = new ObservableCollection<Transaction>(userTransactions);
    }
}