using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Services;
using expense_manager_30.Views;

namespace expense_manager_30.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly DbService _dbService;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string loginMessage = string.Empty;

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }

    public LoginViewModel()
    {
        _dbService = new DbService();
        LoginCommand = new RelayCommand(Login);
        RegisterCommand = new RelayCommand(OpenRegister);
    }

    private void Login()
    {
        if (_dbService.LoginUser(Username, Password))
        {
            var userId = _dbService.GetUserIdByUsername(Username);
            if (userId != null)
            {
                Session.SetUser(userId.Value, Username);
                LoginMessage = "Login successful.";
                ReplaceWindow(new MainWindow());
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

    private void OpenRegister()
    {
        var registerView = new RegisterView
        {
            DataContext = new RegisterViewModel()
        };
        registerView.Show();
    }

    private void ReplaceWindow(Window newWindow)
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
            return;
        var currentWindow = lifetime.Windows.FirstOrDefault(w => w.IsActive);
        newWindow.Show();
        currentWindow?.Close();
    }
}