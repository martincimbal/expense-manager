using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using expense_manager_30.ViewModels;

namespace expense_manager_30.Views;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
        DataContext = new LoginViewModel();
    }
}