using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Services;
using expense_manager_30.Views;

namespace expense_manager_30.ViewModels;

public partial class AccountViewModel : ViewModelBase
{
    private readonly DbService _database = new();

    [ObservableProperty] private string _confirmPassword = string.Empty;

    [ObservableProperty] private string _currentPassword = string.Empty;

    [ObservableProperty] private string _newPassword = string.Empty;

    [ObservableProperty] private string _statusMessage = string.Empty;

    public AccountViewModel()
    {
        ChangePasswordCommand = new RelayCommand(ChangePassword);
        LogoutCommand = new RelayCommand(Logout);
    }

    public ICommand ChangePasswordCommand { get; }
    public ICommand LogoutCommand { get; }

    private void ChangePassword()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) ||
            string.IsNullOrWhiteSpace(NewPassword) ||
            string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            StatusMessage = "Please fill in all fields.";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            StatusMessage = "Passwords do not match.";
            return;
        }

        var success = _database.ChangePassword(Session.CurrentUserId, CurrentPassword, NewPassword);
        StatusMessage = success ? "Password changed successfully." : "Current password is incorrect.";
    }

    private void Logout()
    {
        if (Session.CurrentUserId == 0)
        {
            StatusMessage = "User is not logged in.";
            return;
        }

        Session.Clear();
        var nextWindow = new LoginWindow();
        WindowManagement.ReplaceWindow(nextWindow);
    }
}