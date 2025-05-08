using System;

namespace expense_manager_30.Models;

public class Transaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public bool IsIncome { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public int CategoryId { get; set; }
    public int UserId { get; set; }
}