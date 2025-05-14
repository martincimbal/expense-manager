using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Models;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;

public partial class EditTransactionWindowViewModel : ViewModelBase
{
    private readonly int _transactionId;

    [ObservableProperty] private string _amount;

    [ObservableProperty] private ObservableCollection<Category> _categories;

    [ObservableProperty] private DateTimeOffset _date;

    [ObservableProperty] private bool _isIncome;

    [ObservableProperty] private string _note;

    [ObservableProperty] private Category? _selectedCategory;

    [ObservableProperty] private string _statusMessage = string.Empty;


    public EditTransactionWindowViewModel(Transaction transaction, ObservableCollection<Category> categories)
    {
        _transactionId = transaction.Id;
        Amount = transaction.Amount.ToString("0.##");
        IsIncome = transaction.IsIncome;
        Date = transaction.Date;
        Note = transaction.Note ?? string.Empty;
        SelectedCategory = categories.FirstOrDefault(c => c.Id == transaction.CategoryId);

        Categories = categories;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        DeleteCommand = new RelayCommand(Delete);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public Action? CloseAction { get; set; }
    public bool IsSaved { get; private set; }
    public ICommand DeleteCommand { get; }

    public bool IsDeleted { get; private set; }

    private void Save()
    {
        if (!decimal.TryParse(Amount, out var parsedAmount) || parsedAmount <= 0)
        {
            StatusMessage = "Please enter a valid amount.";
            return;
        }

        if (SelectedCategory == null)
        {
            StatusMessage = "Please select a category.";
            return;
        }

        if (!Session.IsLoggedIn)
        {
            StatusMessage = "Error: User not logged in.";
            return;
        }

        var updatedTransaction = new Transaction
        {
            Id = _transactionId,
            Amount = parsedAmount,
            IsIncome = IsIncome,
            Date = Date.DateTime,
            Note = Note,
            CategoryId = SelectedCategory.Id,
            UserId = Session.CurrentUserId
        };

        DbService.UpdateTransaction(updatedTransaction);
        IsSaved = true;
        CloseAction?.Invoke();
    }

    private void Cancel()
    {
        IsSaved = false;
        CloseAction?.Invoke();
    }

    private void Delete()
    {
        if (!Session.IsLoggedIn)
        {
            StatusMessage = "Error: User not logged in.";
            return;
        }

        DbService.DeleteTransaction(_transactionId);
        IsDeleted = true;
        CloseAction?.Invoke();
    }
}