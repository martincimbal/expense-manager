using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Models;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;

public partial class AddTransactionViewModel : ViewModelBase
{
    private readonly DbService _database;

    [ObservableProperty] private string _amount = string.Empty;

    [ObservableProperty] private ObservableCollection<Category> _categories = [];

    [ObservableProperty] private DateTimeOffset _date = DateTimeOffset.Now;

    [ObservableProperty] private bool _isIncome;

    [ObservableProperty] private string _note = string.Empty;

    [ObservableProperty] private Category? _selectedCategory;

    [ObservableProperty] private string _statusMessage = string.Empty;

    public AddTransactionViewModel()
    {
        _database = new DbService();
        AddTransactionCommand = new RelayCommand(AddTransaction);
        LoadCategories();
    }

    public ICommand AddTransactionCommand { get; }

    private void LoadCategories()
    {
        if (!Session.IsLoggedIn)
        {
            StatusMessage = "Error: User not logged in.";
            return;
        }

        var loadedCategories = _database.GetCategories(Session.CurrentUserId);
        Categories = new ObservableCollection<Category>(loadedCategories);
    }

    private void AddTransaction()
    {
        if (!decimal.TryParse(Amount, out var parsedAmount) || parsedAmount <= 0)
        {
            StatusMessage = "Please enter a valid amount.";
            return;
        }

        if (SelectedCategory == null)
        {
            StatusMessage = "Please select a category.";
            return;
        }

        if (!Session.IsLoggedIn)
        {
            StatusMessage = "Error: User not logged in.";
            return;
        }

        // Convert DateTimeOffset to DateTime before passing it to the DbService
        _database.AddTransaction(parsedAmount, IsIncome, Date.DateTime, Note, SelectedCategory.Id,
            Session.CurrentUserId);

        StatusMessage = "Transaction added successfully.";
        ClearForm();
    }

    private void ClearForm()
    {
        Amount = string.Empty;
        IsIncome = false;
        Date = DateTime.Today;
        Note = string.Empty;
        SelectedCategory = null;
    }
}