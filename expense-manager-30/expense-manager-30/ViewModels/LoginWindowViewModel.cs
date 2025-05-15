using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Services;
using expense_manager_30.Views;

namespace expense_manager_30.ViewModels;

public partial class LoginWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _loginMessage = string.Empty;

    [ObservableProperty] private string _password = string.Empty;

    [ObservableProperty] private string _username = string.Empty;

    public LoginWindowViewModel()
    {
        LoginCommand = new RelayCommand(Login);
        RegisterCommand = new RelayCommand(OpenRegister);
    }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }

    private void Login()
    {
        if (!DbService.LoginUser(Username, Password))
        {
            LoginMessage = "Invalid username or password.";
            return;
        }

        var userId = DbService.GetUserIdByUsername(Username);
        if (userId == null)
        {
            LoginMessage = "Unexpected error: user not found.";
            return;
        }

        Session.SetUser(userId.Value, Username);
        LoginMessage = "Login successful.";

        WindowManagement.ReplaceWindow(new MainWindow());
    }

    private static void OpenRegister()
    {
        WindowManagement.ReplaceWindow(new RegisterWindowView());
    }
}