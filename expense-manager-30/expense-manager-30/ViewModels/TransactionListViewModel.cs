using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Models;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;


public partial class TransactionListViewModel : ViewModelBase{
    private readonly DbService _dbService;

    [ObservableProperty]
    private ObservableCollection<Transaction> _transactions = new();

    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    [ObservableProperty]
    private Category? _selectedFilterCategory;

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();
    
    [ObservableProperty]
    private bool? _isIncomeFilter = null;

    [ObservableProperty]
    private string? _noteFilter = null;

    [ObservableProperty]
    private DateTimeOffset? _startDateFilter;

    [ObservableProperty]
    private DateTimeOffset? _endDateFilter;

    [ObservableProperty]
    private int _selectedIndex;

    public ICommand ApplyFilterCommand { get; }
    public ICommand ClearFilterCommand { get; }

    public TransactionListViewModel()
    {
        _dbService = new DbService();
        LoadTransactions();
        ApplyFilterCommand = new RelayCommand(ApplyFilter);
        ClearFilterCommand = new RelayCommand(ClearFilter);
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

        Categories = new ObservableCollection<Category>(categories);
        Transactions = new ObservableCollection<Transaction>(userTransactions);
    }

    private void ApplyFilter()
    {
        if (!Session.IsLoggedIn) return;

        var categoryId = SelectedFilterCategory?.Id;
        
        DateTime? startDate = _startDateFilter?.DateTime;
        DateTime? endDate = _endDateFilter?.DateTime;

        if (startDate.HasValue)
        {
            startDate = startDate.Value.Date;
        }
    
        if (endDate.HasValue)
        {
            endDate = endDate.Value.Date.AddDays(1).AddSeconds(-1);
        }

        var filtered = _dbService.GetFilteredTransactions(
            Session.CurrentUserId,
            categoryId,
            startDate,
            endDate,
            IsIncomeFilter,
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
        SelectedIndex = 0;
        IsIncomeFilter = null;
        NoteFilter = null;
        LoadTransactions();
    }
}