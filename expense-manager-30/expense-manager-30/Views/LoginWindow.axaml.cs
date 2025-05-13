using Avalonia.Controls;
using expense_manager_30.ViewModels;

namespace expense_manager_30.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = new LoginWindowViewModel();
    }
}