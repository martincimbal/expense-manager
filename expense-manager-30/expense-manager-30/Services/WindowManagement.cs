using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace expense_manager_30.Services;

public static class WindowManagement
{
    public static void ReplaceWindow(Window newWindow)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
            return;
        var currentWindow = lifetime.Windows.FirstOrDefault(w => w.IsActive);
        newWindow.Show();
        currentWindow?.Close();
    }
}