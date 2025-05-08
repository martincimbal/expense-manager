using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly DbService _dbService;

    public RegisterViewModel()
    {
        _dbService = new DbService();
        RegisterCommand = new RelayCommand(RegisterUser);
    }

    [ObservableProperty] 
    private string _username;

    [ObservableProperty] 
    private string _password;

    [ObservableProperty] 
    private string _confirmPassword;

    [ObservableProperty] 
    private string _errorMessage;

    public ICommand RegisterCommand { get; }

    private void RegisterUser()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "All fields are required.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        try
        {
            _dbService.RegisterUser(Username, Password);
            ErrorMessage = "Registration successful!";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
    }
}