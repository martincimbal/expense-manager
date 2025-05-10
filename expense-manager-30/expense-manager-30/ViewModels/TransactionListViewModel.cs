using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Models;
using expense_manager_30.Services;
using expense_manager_30.Views;

namespace expense_manager_30.ViewModels;


public partial class TransactionListViewModel : ViewModelBase
{
    private readonly DbService _dbService;

    [ObservableProperty] private ObservableCollection<Transaction> transactions = new();

    [ObservableProperty] private string statusMessage = string.Empty;

    [ObservableProperty] private Category? selectedFilterCategory;

    [ObservableProperty] private ObservableCollection<Category> categories = new();

    [ObservableProperty] private bool isIncomeFilter;

    [ObservableProperty] private string? noteFilter = null;

    [ObservableProperty] private DateTimeOffset? startDateFilter;

    [ObservableProperty] private DateTimeOffset? endDateFilter;

    public ICommand ApplyFilterCommand { get; }
    public ICommand ClearFilterCommand { get; }
    public ICommand DeleteTransactionCommand { get; }
    public ICommand EditTransactionCommand { get; }

    public TransactionListViewModel()
    {
        _dbService = new DbService();
        ApplyFilterCommand = new RelayCommand(ApplyFilter);
        ClearFilterCommand = new RelayCommand(ClearFilter);
        DeleteTransactionCommand = new RelayCommand<Transaction>(DeleteTransaction);
        EditTransactionCommand = new RelayCommand<Transaction>(EditTransaction);
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

        Categories = new ObservableCollection<Category>(categories);
        Transactions = new ObservableCollection<Transaction>(userTransactions);
    }

    private void ApplyFilter()
    {
        if (!Session.IsLoggedIn) return;

        var categoryId = SelectedFilterCategory?.Id;
        DateTime? startDate = StartDateFilter?.DateTime.Date;
        DateTime? endDate = EndDateFilter?.DateTime.Date.AddDays(1).AddSeconds(-1);
        bool? incomeFilter = IsIncomeFilter ? true : null;

        var filtered = _dbService.GetFilteredTransactions(
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
        isIncomeFilter = false;
        NoteFilter = null;
        LoadTransactions();
    }

    private void DeleteTransaction(Transaction transaction)
    {
        if (transaction == null) return;

        _dbService.DeleteTransaction(transaction.Id);
        LoadTransactions();
    }
    
    private void EditTransaction(Transaction transaction)
    {
        if (transaction == null) return;

        // Získej všechny kategorie, které použiješ pro ComboBox v okně pro úpravu
        var categories = _dbService.GetCategories(Session.CurrentUserId);

        // Vytvoření instance nového okna
        var editWindow = new EditTransactionWindow();

        // Vytvoření ViewModelu pro okno
        var viewModel = new EditTransactionViewModel(transaction, new ObservableCollection<Category>(categories));

        // Přiřazení ViewModelu jako DataContext pro okno
        editWindow.DataContext = viewModel;

        // Zobrazení okna
        editWindow.Show();  // Tímto způsobem je okno modální a počká na zavření okna
    }


}