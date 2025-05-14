using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using expense_manager_30.ViewModels;

namespace expense_manager_30.Views;

public partial class ImportExportView : UserControl
{
    public ImportExportView()
    {
        InitializeComponent();
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not ImportExportViewModel vm)
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.StorageProvider is not { } storage) return;
            var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choose file to import",
                AllowMultiple = false
            });

            if (files.Count > 0) await vm.ImportFromFileAsync(files[0]);
        }
        catch (IOException ioEx)
        {
            if (DataContext is not ImportExportViewModel vm)
                return;

            vm.StatusMessage = $"I/O Error: {ioEx.Message}";
        }
        catch (UnauthorizedAccessException authEx)
        {
            if (DataContext is not ImportExportViewModel vm)
                return;

            vm.StatusMessage = $"Access denied: {authEx.Message}";
        }
        catch (Exception ex)
        {
            if (DataContext is not ImportExportViewModel vm)
                return;

            vm.StatusMessage = $"Unexpected error: {ex.Message[..Math.Min(ex.Message.Length, 100)]}...";
        }
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not ImportExportViewModel vm)
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.StorageProvider is not { } storage) return;
            var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Choose file to export",
                SuggestedFileName = "export.json"
            });

            if (file is not null) await vm.ExportToFileAsync(file);
        }
        catch (IOException ioEx)
        {
            if (DataContext is not ImportExportViewModel vm)
                return;

            vm.StatusMessage = $"I/O Error: {ioEx.Message}";
        }
        catch (UnauthorizedAccessException authEx)
        {
            if (DataContext is not ImportExportViewModel vm)
                return;

            vm.StatusMessage = $"Access denied: {authEx.Message}";
        }
        catch (Exception ex)
        {
            if (DataContext is not ImportExportViewModel vm)
                return;

            vm.StatusMessage = $"Unexpected error: {ex.Message[..Math.Min(ex.Message.Length, 100)]}...";
        }
    }
}