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
    private readonly DbService _dbService;

    [ObservableProperty]
    private string amount = string.Empty;

    [ObservableProperty]
    private bool isIncome;

    [ObservableProperty]
    private DateTimeOffset date = DateTimeOffset.Now;

    [ObservableProperty]
    private string note = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ICommand AddTransactionCommand { get; }

    public AddTransactionViewModel()
    {
        _dbService = new DbService();
        AddTransactionCommand = new RelayCommand(AddTransaction);
        LoadCategories();
    }

    private void LoadCategories()
    {
        if (!Session.IsLoggedIn)
        {
            StatusMessage = "Error: User not logged in.";
            return;
        }

        var loadedCategories = _dbService.GetCategories(Session.CurrentUserId);
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
        _dbService.AddTransaction(parsedAmount, IsIncome, Date.DateTime, Note, SelectedCategory.Id, Session.CurrentUserId);

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