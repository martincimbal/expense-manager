using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;

public partial class AddCategoryViewModel : ViewModelBase
{
    private readonly DbService _dbService;

    [ObservableProperty]
    private string categoryName = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public AddCategoryViewModel()
    {
        _dbService = new DbService();
    }

    [RelayCommand]
    private void AddCategory()
    {
        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            StatusMessage = "Please enter a category name.";
            return;
        }

        if (!Session.IsLoggedIn)
        {
            StatusMessage = "User not logged in.";
            return;
        }

        DbService.AddCategory(CategoryName, Session.CurrentUserId);
        StatusMessage = "Category added!";
        CategoryName = string.Empty;
    }
}
