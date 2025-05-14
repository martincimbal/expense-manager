using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Services;
using expense_manager_30.Views;

namespace expense_manager_30.ViewModels;

public partial class LoginWindowViewModel : ObservableObject
{
    private readonly DbService _database;

    [ObservableProperty] private string _loginMessage = string.Empty;

    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private string _username = string.Empty;

    public LoginWindowViewModel()
    {
        _database = new DbService();
        LoginCommand = new RelayCommand(Login);
        RegisterCommand = new RelayCommand(OpenRegister);
    }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }

    private void Login()
    {
        if (_database.LoginUser(Username, Password))
        {
            var userId = _database.GetUserIdByUsername(Username);
            if (userId != null)
            {
                Session.SetUser(userId.Value, Username);
                LoginMessage = "Login successful.";
                WindowManagement.ReplaceWindow(new MainWindow());
            }
            else
            {
                LoginMessage = "Unexpected error: user not found.";
            }
        }
        else
        {
            LoginMessage = "Invalid username or password.";
        }
    }

    private static void OpenRegister()
    {
        var registerView = new RegisterWindowView
        {
            DataContext = new RegisterWindowViewModel()
        };
        registerView.Show();
    }
}