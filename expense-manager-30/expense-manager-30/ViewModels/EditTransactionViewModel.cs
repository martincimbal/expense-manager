using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Models;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;

public partial class EditTransactionViewModel : ViewModelBase
{
    private readonly DbService _dbService = new();

    public ObservableCollection<Category> Categories { get; }

    [ObservableProperty] private Transaction transaction;

    [ObservableProperty] private Category? selectedCategory;

    public IRelayCommand SaveCommand { get; }

    public EditTransactionViewModel(Transaction transactionToEdit, ObservableCollection<Category> categories)
    {
        Transaction = new Transaction
        {
            Id = transactionToEdit.Id,
            Amount = transactionToEdit.Amount,
            Date = transactionToEdit.Date,
            Note = transactionToEdit.Note,
            CategoryId = transactionToEdit.CategoryId,
            IsIncome = transactionToEdit.IsIncome
        };

        Categories = categories;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == Transaction.CategoryId);
        SaveCommand = new RelayCommand(Save);
    }

    private void Save()
    {
        return;
    }
    // {
    //     if (SelectedCategory == null)
    //     {
    //         MessageBox.Show("Please select a category.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
    //         return;
    //     }
    //
    //     Transaction.CategoryId = SelectedCategory.Id;
    //     _dbService.UpdateTransaction(Transaction);
    //
    //     // Zavřít okno, pokud běží jako dialog
    //     Application.Current.Windows
    //         .OfType<Window>()
    //         .FirstOrDefault(w => w.DataContext == this)?
    //         .Close();
    // }
}