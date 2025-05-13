namespace expense_manager_30.Models;

public class Category
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int UserId { get; set; }

    public override string ToString()
    {
        return Name;
    }
}