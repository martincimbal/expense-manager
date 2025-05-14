using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Models;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;

public partial class CategoriesViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<Category> _categories = [];

    [ObservableProperty] private string _categoryName = string.Empty;

    [ObservableProperty] private bool _isDeleteButtonVisible;

    [ObservableProperty] private Category? _selectedCategory;

    [ObservableProperty] private string _statusMessage = string.Empty;

    public CategoriesViewModel()
    {
        LoadCategories();
    }

    private void LoadCategories()
    {
        if (!Session.IsLoggedIn) return;
        var cats = DbService.GetCategories(Session.CurrentUserId);
        Categories = new ObservableCollection<Category>(cats);
    }

    partial void OnSelectedCategoryChanged(Category? value)
    {
        IsDeleteButtonVisible = value != null;
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
        LoadCategories();
    }

    [RelayCommand]
    private void DeleteCategory()
    {
        if (SelectedCategory == null)
        {
            StatusMessage = "Please select a category to delete.";
            return;
        }

        if (!Session.IsLoggedIn)
        {
            StatusMessage = "User not logged in.";
            return;
        }

        var isDeleted = DbService.DeleteCategory(SelectedCategory.Id);

        if (isDeleted)
        {
            StatusMessage = "Category deleted!";
            SelectedCategory = null;
            LoadCategories();
        }
        else
        {
            StatusMessage = "Category cannot be deleted because it is used in transactions.";
        }
    }
}