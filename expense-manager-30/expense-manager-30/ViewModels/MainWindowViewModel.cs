using CommunityToolkit.Mvvm.ComponentModel;

namespace expense_manager_30.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private object? _currentPage;

    [ObservableProperty] private int _selectedIndex;

    public MainWindowViewModel()
    {
        SelectedIndex = 0;
        CurrentPage = new DashboardViewModel();
    }

    partial void OnSelectedIndexChanged(int value)
    {
        CurrentPage = value switch
        {
            0 => new DashboardViewModel(),
            1 => new TransactionListViewModel(),
            2 => new AddTransactionViewModel(),
            3 => new AddCategoryViewModel(),
            4 => new StatisticsViewModel(),
            5 => new ImportExportViewModel(),
            6 => new AccountViewModel(),
            _ => CurrentPage
        };
    }
}