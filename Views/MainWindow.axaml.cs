using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CreativeFileBrowser.Models;
using CreativeFileBrowser.ViewModels;

namespace CreativeFileBrowser.Views;

public partial class MainWindow : Window
{
    //private string? fullPath;

    public MainWindow()
    {
        InitializeComponent();
        // Connecting the ViewModel with Code-Behind
        DataContext = new MainViewModel();
    }

    // Handle double-click on items to expand directories
    private void OnItemDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is MainViewModel mainVm &&
            sender is StackPanel panel &&
            panel.DataContext is FileSystemItem item)
        {
            if (item.IsDirectory)
            {
                mainVm.FileExplorer.ExpandDirectoryCommand.Execute(item);
            }
        }
    }


    // Handle path text box click to copy path
    private async void OnPathTextBoxTapped(object sender, TappedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string path = textBox.Text ?? string.Empty;

            // Copy path to clipboard
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(path);
            }

            // Show confirmation message
            CopyNotification.IsVisible = true;

            // Hide confirmation after 3 seconds
            await Task.Delay(3000);
            CopyNotification.IsVisible = false;
        }
    }

    // Handle tree item click to copy full path
    private async void OnTreeItemTapped(object sender, TappedEventArgs e)
    {
        if (sender is TextBlock textBlock &&
            textBlock.DataContext is FileSystemItem item)
        {
            // Get the full path from the item
            string path = item.FullPath;

            // Copy path to clipboard
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(path);

                // Show confirmation message
                CopyNotification.IsVisible = true;

                // Use DispatcherTimer for more reliable UI updates
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };

                timer.Tick += (s, args) =>
                {
                    CopyNotification.IsVisible = false;
                    timer.Stop();
                };

                timer.Start();
            }

            e.Handled = true; // Prevent event bubbling
        }
    }
}