using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Models;
using expense_manager_30.Services;
using expense_manager_30.Views;

namespace expense_manager_30.ViewModels;

public partial class TransactionListViewModel : ViewModelBase
{
    private readonly DbService _database;

    [ObservableProperty] private ObservableCollection<Category> _categories = [];

    [ObservableProperty] private DateTimeOffset? _endDateFilter;

    [ObservableProperty] private bool _isIncomeFilter;

    [ObservableProperty] private string? _noteFilter;

    [ObservableProperty] private Category? _selectedFilterCategory;

    [ObservableProperty] private DateTimeOffset? _startDateFilter;

    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private ObservableCollection<Transaction> _transactions = [];

    public TransactionListViewModel()
    {
        _database = new DbService();
        ApplyFilterCommand = new RelayCommand(ApplyFilter);
        ClearFilterCommand = new RelayCommand(ClearFilter);
        DeleteTransactionCommand = new RelayCommand<Transaction>(DeleteTransaction);
        EditTransactionCommand = new RelayCommand<Transaction>(EditTransaction);
        LoadTransactions();
    }

    public ICommand ApplyFilterCommand { get; }
    public ICommand ClearFilterCommand { get; }
    public ICommand DeleteTransactionCommand { get; }
    public ICommand EditTransactionCommand { get; }

    [RelayCommand]
    private void LoadTransactions()
    {
        if (!Session.IsLoggedIn)
        {
            StatusMessage = "User not logged in.";
            Transactions.Clear();
            return;
        }

        var userTransactions = _database.GetTransactions(Session.CurrentUserId);
        var categories = _database.GetCategories(Session.CurrentUserId);

        foreach (var transaction in userTransactions)
        {
            var category = categories.FirstOrDefault(c => c.Id == transaction.CategoryId);
            transaction.CategoryName = category?.Name ?? "Unknown";
        }

        Categories = new ObservableCollection<Category>(categories);
        Transactions = new ObservableCollection<Transaction>(userTransactions);
    }

    private void ApplyFilter()
    {
        if (!Session.IsLoggedIn) return;

        var categoryId = SelectedFilterCategory?.Id;
        var startDate = StartDateFilter?.DateTime.Date;
        var endDate = EndDateFilter?.DateTime.Date.AddDays(1).AddSeconds(-1);
        bool? incomeFilter = IsIncomeFilter ? true : null;

        var filtered = _database.GetFilteredTransactions(
            Session.CurrentUserId,
            categoryId,
            startDate,
            endDate,
            incomeFilter,
            string.IsNullOrWhiteSpace(NoteFilter) ? null : NoteFilter
        );

        foreach (var t in filtered)
        {
            var category = Categories.FirstOrDefault(c => c.Id == t.CategoryId);
            t.CategoryName = category?.Name ?? "Unknown";
        }

        Transactions = new ObservableCollection<Transaction>(filtered);
        StatusMessage = $"{Transactions.Count} filtered transaction(s) loaded.";
    }

    private void ClearFilter()
    {
        SelectedFilterCategory = null;
        StartDateFilter = null;
        EndDateFilter = null;
        IsIncomeFilter = false;
        NoteFilter = null;
        LoadTransactions();
    }

    private void DeleteTransaction(Transaction? transaction)
    {
        if (transaction == null) return;

        _database.DeleteTransaction(transaction.Id);
        LoadTransactions();
    }

    private async void EditTransaction(Transaction? transaction)
    {
        if (transaction == null) return;

        var categories = _database.GetCategories(Session.CurrentUserId);

        var editWindow = new EditTransactionWindow();
        var viewModel = new EditTransactionWindowViewModel(transaction, new ObservableCollection<Category>(categories))
            {
                CloseAction = () => editWindow.Close()
            };

        editWindow.DataContext = viewModel;

        var lifetime = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var ownerWindow = lifetime?.Windows.FirstOrDefault(w => w.IsActive);

        if (ownerWindow == null) return;
        await editWindow.ShowDialog(ownerWindow);

        if (viewModel.IsSaved || viewModel.IsDeleted) LoadTransactions();
    }
}