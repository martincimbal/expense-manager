using CommunityToolkit.Mvvm.ComponentModel;

namespace expense_manager_30.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private int selectedIndex;

    [ObservableProperty]
    private object? currentPage;

    partial void OnSelectedIndexChanged(int value)
    {
        CurrentPage = value switch
        {
            // 0 => new TransactionListViewModel(),
            1 => new AddTransactionViewModel(),
            2 => new AddCategoryViewModel(),
            // 3 => new CategoryManagementViewModel(),
            // 4 => new ImportExportViewModel(),
            // 5 => new LogoutViewModel(),
            // _ => CurrentPage
            _ => new AddTransactionViewModel()
        };
    }

    public MainWindowViewModel()
    {
        SelectedIndex = 0;
        // CurrentPage = new TransactionListViewModel();
        CurrentPage = new AddCategoryViewModel();
    }
}