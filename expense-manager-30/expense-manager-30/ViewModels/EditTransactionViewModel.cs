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

public partial class EditTransactionViewModel : ViewModelBase
{
    private readonly DbService _dbService;

    public Transaction Transaction { get; }

    public ObservableCollection<Category> Categories { get; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public Action? CloseAction { get; set; }
    public bool IsSaved { get; private set; } = false;

    public EditTransactionViewModel(Transaction transaction, ObservableCollection<Category> categories)
    {
        _dbService = new DbService();
        Transaction = transaction;
        Categories = categories;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    private void Save()
    {
        _dbService.UpdateTransaction(Transaction);
        IsSaved = true;
        CloseAction?.Invoke();
    }

    private void Cancel()
    {
        IsSaved = false;
        CloseAction?.Invoke();
    }
}