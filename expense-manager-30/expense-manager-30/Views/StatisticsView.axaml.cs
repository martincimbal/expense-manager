using Avalonia.Controls;
using expense_manager_30.ViewModels;

namespace expense_manager_30.Views;

public partial class StatisticsView : UserControl
{
    public StatisticsView()
    {
        InitializeComponent();
        DataContext = new StatisticsViewModel();
    }
}