using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using expense_manager_30.Models;
using expense_manager_30.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace expense_manager_30.ViewModels;

public partial class StatisticsViewModel : ViewModelBase
{
    private readonly DbService _dbService;

    [ObservableProperty]
    private ObservableCollection<ISeries> _pieSeries;

    [ObservableProperty]
    private ObservableCollection<ISeries> _barSeries;

    [ObservableProperty]
    private ObservableCollection<ISeries> _lineSeries;

    [ObservableProperty]
    private Axis[] _barXAxis = [];

    [ObservableProperty]
    private Axis[] _barYAxis = [];

    [ObservableProperty]
    private Axis[] _lineXAxis = [];

    [ObservableProperty]
    private Axis[] _lineYAxis = [];

    [ObservableProperty]
    private bool _isIncome;

    public bool ShowIncomeCheckBox => SelectedChart != "Balance";

    public List<string> AvailableCharts { get; } =
    [
        "Categories",
        "Monthly Totals",
        "Balance"
    ];

    [ObservableProperty]
    private string _selectedChart = "Categories";

    public StatisticsViewModel()
    {
        _dbService = new DbService();
        PieSeries = [];
        BarSeries = [];
        LineSeries = [];
        LoadInitialData();
    }

    partial void OnIsIncomeChanged(bool value) => LoadStatistics();
    partial void OnSelectedChartChanged(string value)
    {
        OnPropertyChanged(nameof(ShowIncomeCheckBox));
        LoadStatistics();
    }

    private void LoadInitialData()
    {
        if (!Session.IsLoggedIn) return;
        LoadStatistics();
        UpdateLineChart();
    }

    private void LoadStatistics()
    {
        if (!Session.IsLoggedIn) return;

        var transactions = DbService.GetTransactions(Session.CurrentUserId)
            .Where(t => t.IsIncome == IsIncome)
            .OrderBy(t => t.Date)
            .ToList();

        UpdatePieChart(transactions);
        UpdateBarChart(transactions);
    }

    private void UpdatePieChart(List<Transaction> transactions)
    {
        var categories = DbService.GetCategories(Session.CurrentUserId);
        
        PieSeries.Clear();
        foreach (var group in transactions
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                Category = categories.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Unknown",
                Sum = g.Sum(t => t.Amount)
            }))
        {
            PieSeries.Add(new PieSeries<decimal>
            {
                Values = [group.Sum],
                Name = group.Category
            });
        }
    }

    private void UpdateBarChart(List<Transaction> transactions)
    {
        BarSeries.Clear();
        var byMonth = transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .ToList();

        BarYAxis =
        [
            new Axis { Name = "Amount" }
        ];

        BarXAxis =
        [
            new Axis 
            { 
                Labels = byMonth
                    .Select(g => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key.Month)} {g.Key.Year}")
                    .ToList(), 
                Name = "Month" 
            }
        ];
    
        BarSeries.Add(new ColumnSeries<decimal>
        {
            Values = byMonth
                .Select(g => g.Sum(t => t.Amount))
                .ToList(),
            Name = "Monthly Total",
            Stroke = null,
            Fill = new SolidColorPaint(SKColors.SteelBlue)
        });
    }
    private void UpdateLineChart()
    {
        LineSeries.Clear();
        var allTransactions = DbService.GetTransactions(Session.CurrentUserId)
            .OrderBy(t => t.Date)
            .ToList();

        decimal runningBalance = 0;
        var balanceValues = new List<decimal>();
        var balanceLabels = new List<string>();

        foreach (var t in allTransactions)
        {
            runningBalance += t.IsIncome ? t.Amount : -t.Amount;
            balanceValues.Add(runningBalance);
            balanceLabels.Add(t.Date.ToString("d.M."));
        }

        LineYAxis =
        [
            new Axis { Name = "Balance" }
        ];

        LineSeries.Add(new LineSeries<decimal>
        {
            Values = balanceValues,
            GeometrySize = 6,
            Name = "Account Balance",
            Fill = null,
            Stroke = new SolidColorPaint(SKColors.DarkOliveGreen, 2)
        });

        LineXAxis = [new Axis { Labels = balanceLabels, Name = "Date", LabelsRotation = 45 }];
    }}