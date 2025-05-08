using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Services;
using expense_manager_30.Views;

namespace expense_manager_30.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly DbService _dbService;

    public LoginViewModel()
    {
        _dbService = new DbService();
        LoginCommand = new RelayCommand(LoginUser);
    }

    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _errorMessage;

    public ICommand LoginCommand { get; }

    private void LoginUser()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter both username and password.";
            return;
        }

        if (_dbService.LoginUser(Username, Password))
        {
            ErrorMessage = "Login successful!";
            // Navigate to another page or perform any necessary action
        }
        else
        {
            ErrorMessage = "Invalid username or password.";
        }
    }
    
    
    [RelayCommand]
    private void OpenRegister()
    {
        var registerView = new RegisterView
        {
            DataContext = new RegisterViewModel()
        };
        registerView.Show();
    }
}