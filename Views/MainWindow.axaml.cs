using Avalonia.Controls;
using Avalonia.Input;
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
}