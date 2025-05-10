using System.Collections.Generic;

namespace expense_manager_30.Services;

public class MenuItem
{
    public string Title { get; set; }
    public object? PageViewModel { get; set; }
    public string? ScrollTarget { get; set; } // "PieSection", "BarSection", etc.
    public List<MenuItem>? Children { get; set; }
}