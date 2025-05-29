using Avalonia.Controls;
using expense_manager_30.ViewModels;

namespace expense_manager_30.Views;

public partial class EditTransactionWindow : Window
{
    public EditTransactionWindow()
    {
        InitializeComponent();

        if (DataContext is EditTransactionWindowViewModel vm) vm.CloseAction = Close;
    }
}