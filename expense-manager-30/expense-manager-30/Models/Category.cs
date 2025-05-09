namespace expense_manager_30.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsIncome { get; set; }
    public int UserId { get; set; }
    
    public override string ToString() => Name;
}