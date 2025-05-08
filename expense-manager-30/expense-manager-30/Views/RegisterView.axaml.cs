using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using expense_manager_30.ViewModels;

namespace expense_manager_30.Views;

public partial class RegisterView : Window
{
    public RegisterView()
    {
        InitializeComponent();
        DataContext = new RegisterViewModel();
    }
}