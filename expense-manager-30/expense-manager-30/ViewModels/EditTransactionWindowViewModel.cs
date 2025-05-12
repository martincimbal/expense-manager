using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using expense_manager_30.Models;
using expense_manager_30.Services;

namespace expense_manager_30.ViewModels;

public partial class EditTransactionWindowViewModel : ViewModelBase
{
    private readonly DbService _dbService;

    [ObservableProperty]
    private string amount;

    [ObservableProperty]
    private bool isIncome;

    [ObservableProperty]
    private DateTimeOffset date;

    [ObservableProperty]
    private string note;

    [ObservableProperty]
    private ObservableCollection<Category> categories;

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public Action? CloseAction { get; set; }
    public bool IsSaved { get; private set; } = false;

    private readonly int transactionId;
    public ICommand DeleteCommand { get; }

    public bool IsDeleted { get; private set; } = false;


    public EditTransactionWindowViewModel(Transaction transaction, ObservableCollection<Category> categories)
    {
        _dbService = new DbService();
        
        transactionId = transaction.Id;
        Amount = transaction.Amount.ToString("0.##");
        IsIncome = transaction.IsIncome;
        Date = transaction.Date;
        Note = transaction.Note;
        SelectedCategory = categories.FirstOrDefault(c => c.Id == transaction.CategoryId);

        Categories = categories;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        DeleteCommand = new RelayCommand(Delete);
    }

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
            Id = transactionId,
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
        
        DbService.DeleteTransaction(transactionId);
        IsDeleted = true;
        CloseAction?.Invoke();
    }
}