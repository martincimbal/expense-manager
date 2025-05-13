using Avalonia.Controls;
using expense_manager_30.ViewModels;

namespace expense_manager_30.Views;

public partial class RegisterWindowView : Window
{
    public RegisterWindowView()
    {
        InitializeComponent();
        DataContext = new RegisterWindowViewModel();
    }
}