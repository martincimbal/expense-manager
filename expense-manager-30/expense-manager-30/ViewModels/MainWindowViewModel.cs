using CommunityToolkit.Mvvm.ComponentModel;
using expense_manager_30.enums;

namespace expense_manager_30.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private object? _currentPage;

    [ObservableProperty] private PageType _selectedPage;

    public MainWindowViewModel()
    {
        OnSelectedPageChanged(PageType.Dashboard);
    }

    partial void OnSelectedPageChanged(PageType value)
    {
        CurrentPage = value switch
        {
            PageType.Dashboard => new DashboardViewModel(this),
            PageType.Transactions => new TransactionListViewModel(),
            PageType.AddTransaction => new AddTransactionViewModel(),
            PageType.Categories => new CategoriesViewModel(),
            PageType.Statistics => new StatisticsViewModel(),
            PageType.ImportExport => new ImportExportViewModel(),
            PageType.Account => new AccountViewModel(),
            _ => CurrentPage
        };
    }
}