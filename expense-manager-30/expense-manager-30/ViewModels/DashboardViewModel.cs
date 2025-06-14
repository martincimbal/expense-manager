using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.enums;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindow;

    [ObservableProperty] private decimal _currentBalance;

    [ObservableProperty] private decimal _lastMonthBalance;

    [ObservableProperty] private int _lastMonthTransactions;

    [ObservableProperty] private ObservableCollection<string> _topCategories = [];

    [ObservableProperty] private int _totalTransactions;

    public DashboardViewModel(MainWindowViewModel mainWindow)
    {
        _mainWindow = mainWindow;

        GoToTransactionsCommand = new RelayCommand(GoToTransactions);
        GoToStatisticsCommand = new RelayCommand(GoToStatistics);

        LoadDashboardData();
    }


    public ICommand GoToTransactionsCommand { get; }
    public ICommand GoToStatisticsCommand { get; }

    private void LoadDashboardData()
    {
        if (!Session.IsLoggedIn) return;

        var allTransactions = DbService.GetTransactions(Session.CurrentUserId);
        var categories = DbService.GetCategories(Session.CurrentUserId);

        CurrentBalance = allTransactions.Sum(t => t.IsIncome ? t.Amount : -t.Amount);
        TotalTransactions = allTransactions.Count;

        var oneMonthAgo = DateTime.Now.AddMonths(-1);
        var lastMonthTransactionsList = allTransactions.Where(t => t.Date >= oneMonthAgo).ToList();

        LastMonthBalance = lastMonthTransactionsList.Sum(t => t.IsIncome ? t.Amount : -t.Amount);
        LastMonthTransactions = lastMonthTransactionsList.Count;

        var top = allTransactions
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryName = categories.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Unknown",
                Total = g.Sum(t => t.IsIncome ? t.Amount : -t.Amount)
            })
            .OrderByDescending(x => Math.Abs(x.Total))
            .Take(3)
            .Select(x => $"{x.CategoryName}: {x.Total:C}")
            .ToList();

        TopCategories = new ObservableCollection<string>(top);
    }

    private void GoToTransactions()
    {
        _mainWindow.SelectedPage = PageType.Transactions;
    }

    private void GoToStatistics()
    {
        _mainWindow.SelectedPage = PageType.Statistics;
    }
}