using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CreativeFileBrowser.Models;
using CreativeFileBrowser.ViewModels;

namespace CreativeFileBrowser.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Connecting the ViewModel with Code-Behind
        DataContext = new FileExplorerViewModel();
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

    // Handler for TextBox click to copy path
    private async void OnPathTextBoxTapped(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
        {
            try
            {
                // Debug output before attempting clipboard operation
                Console.WriteLine($"Attempting to copy: {textBox.Text}");
                
                // Get clipboard through TopLevel
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel != null)
                {
                    // Copy to clipboard - make sure to await this
                    if (topLevel.Clipboard != null)
                    {
                        await topLevel.Clipboard.SetTextAsync(textBox.Text);
                    }
                    else
                    {
                        Console.WriteLine("Clipboard is null - cannot copy text");
                    }
                    
                    // Debug to confirm copy worked
                    Console.WriteLine("Text copied to clipboard successfully");
                    
                    // Show notification
                    if (CopyNotification != null)
                    {
                        CopyNotification.IsVisible = true;
                        Console.WriteLine("Showing notification");
                        
                        // Create a timer to hide the notification
                        var timer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(1.5)
                        };
                        
                        timer.Tick += (s, args) =>
                        {
                            CopyNotification.IsVisible = false;
                            timer.Stop();
                            Console.WriteLine("Notification hidden");
                        };
                        
                        timer.Start();
                    }
                    else
                    {
                        Console.WriteLine("CopyNotification element is null");
                    }
                }
                else
                {
                    Console.WriteLine("TopLevel is null - cannot access clipboard");
                }
            }
            catch (Exception ex)
            {
                // Output exception for debugging
                Console.WriteLine($"Clipboard error: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}