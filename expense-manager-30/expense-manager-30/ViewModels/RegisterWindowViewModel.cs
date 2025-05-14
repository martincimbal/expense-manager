using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Services;
using expense_manager_30.Views;

namespace expense_manager_30.ViewModels;

public partial class RegisterWindowViewModel : ObservableObject
{
    private readonly DbService _database;

    [ObservableProperty] private string _confirmPassword = string.Empty;

    [ObservableProperty] private string _statusMessage = string.Empty;

    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private string _username = string.Empty;

    public RegisterWindowViewModel()
    {
        _database = new DbService();
        RegisterCommand = new RelayCommand(RegisterUser);
        ReturnToLoginCommand = new RelayCommand(ReturnToLogin);
    }

    public ICommand RegisterCommand { get; }
    public ICommand ReturnToLoginCommand { get; }

    private void RegisterUser()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            StatusMessage = "All fields are required.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            StatusMessage = "Passwords do not match.";
            return;
        }

        if (!DbService.RegisterUser(Username, Password))
        {
            StatusMessage = "Username already exists.";
            return;
        }

        AddCategories();
        StatusMessage = "Registration successful!";
    }

    private void AddCategories()
    {
        var id = DbService.GetUserIdByUsername(Username);

        List<string> categories =
        [
            "Food",
            "Salary",
            "Rent",
            "Health",
            "Subscription"
        ];

        categories.ForEach(category => DbService.AddCategory(category, id));
    }

    private static void ReturnToLogin()
    {
        WindowManagement.ReplaceWindow(new LoginWindow());
    }
}