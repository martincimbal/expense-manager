using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
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
    private ObservableCollection<ISeries> pieSeries = new();

    [ObservableProperty]
    private ObservableCollection<ISeries> barSeries = new();

    [ObservableProperty]
    private ObservableCollection<ISeries> lineSeries = new();

    [ObservableProperty]
    private Axis[] barXAxis = new Axis[0];

    [ObservableProperty]
    private Axis[] barYAxis = new Axis[0];

    [ObservableProperty]
    private Axis[] lineXAxis = new Axis[0];

    [ObservableProperty]
    private Axis[] lineYAxis = new Axis[0];

    [ObservableProperty]
    private string summary = string.Empty;

    [ObservableProperty]
    private bool isIncome = false;

    public StatisticsViewModel()
    {
        _dbService = new DbService();
        LoadStatistics();
    }

    partial void OnIsIncomeChanged(bool value)
    {
        LoadStatistics();
    }

    private void LoadStatistics()
    {
        if (!Session.IsLoggedIn) return;

        var transactions = _dbService
            .GetTransactions(Session.CurrentUserId)
            .Where(t => t.IsIncome == IsIncome)
            .OrderBy(t => t.Date)
            .ToList();

        var categories = _dbService.GetCategories(Session.CurrentUserId);

        // Pie Chart
        var grouped = transactions
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                Category = categories.FirstOrDefault(c => c.Id == g.Key)?.Name ?? "Unknown",
                Sum = g.Sum(t => t.Amount)
            })
            .ToList();

        PieSeries = new ObservableCollection<ISeries>(
            grouped.Select(g => new PieSeries<decimal>
            {
                Values = new[] { g.Sum },
                Name = g.Category
            }));

        // Bar Chart (Monthly Totals)
        var byMonth = transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .ToList();

        var monthLabels = byMonth
            .Select(g => $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key.Month)} {g.Key.Year}")
            .ToList();

        var monthValues = byMonth.Select(g => g.Sum(t => t.Amount)).ToList();

        BarSeries = new ObservableCollection<ISeries>
        {
            new ColumnSeries<decimal>
            {
                Values = monthValues,
                Name = "Monthly Total",
                Stroke = null,
                Fill = new SolidColorPaint(SKColors.SteelBlue)
            }
        };

        BarXAxis = new[]
        {
            new Axis { Labels = monthLabels, Name = "Month" }
        };
        BarYAxis = new[]
        {
            new Axis { Name = "Amount" }
        };

        // Line Chart (Cumulative Balance)
        decimal runningTotal = 0;
        var cumulativeValues = new List<decimal>();
        foreach (var t in transactions)
        {
            runningTotal += t.Amount;
            cumulativeValues.Add(runningTotal);
        }

        var lineLabels = transactions.Select(t => t.Date.ToString("d.M.")).ToList();

        LineSeries = new ObservableCollection<ISeries>
        {
            new LineSeries<decimal>
            {
                Values = cumulativeValues,
                GeometrySize = 6,
                Name = "Cumulative Balance",
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.DarkOliveGreen, 2)
            }
        };

        LineXAxis = new[] { new Axis { Labels = lineLabels, Name = "Date", LabelsRotation = 45 } };
        LineYAxis = new[] { new Axis { Name = "Balance" } };

        var total = transactions.Sum(t => t.Amount);
        var avg = transactions.Any() ? transactions.Average(t => t.Amount) : 0;

        Summary = $"{(IsIncome ? "Incomes" : "Expenses")} total: {total:C}, average: {avg:C}";
    }
    
    public enum ChartType { Pie, Bar, Line }

    [ObservableProperty]
    private ChartType _selectedChart = ChartType.Pie;

    // Seznam dostupných grafů pro ComboBox
    public Array AvailableCharts => Enum.GetValues(typeof(ChartType));
}