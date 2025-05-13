using Avalonia.Controls;
using expense_manager_30.ViewModels;

namespace expense_manager_30.Views;

public partial class TransactionListView : UserControl
{
    public TransactionListView()
    {
        InitializeComponent();
        DataContext = new TransactionListViewModel();
    }
}