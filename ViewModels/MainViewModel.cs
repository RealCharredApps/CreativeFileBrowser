using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CreativeFileBrowser.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // Title Top Area
    [ObservableProperty]
    private string _title = "CFB";

    // Current view state - determines which UI elements are visible
    [ObservableProperty]
    private ViewType currentView = ViewType.SystemFiles;

    // Left Options Area
    //Each Feature item will have label + svg.icon button above

    // Text bindings for the workspace bar
    [ObservableProperty]
    private string currentWorkspaceTxt = string.Empty;

    [ObservableProperty]
    private string saveCurrentWorkspaceTxt = string.Empty;

    [ObservableProperty]
    private string updateCurrentWorkspaceTxt = string.Empty;

    [ObservableProperty]
    private string removeCurrentWorkspaceTxt = string.Empty;

    // Feature Text Bindings
    [ObservableProperty]
    private string _systemFilesIconTxt = "System Files";
    [ObservableProperty]
    private string _monitoredFilesIconTxt = "Monitored";
    [ObservableProperty]
    private string _workspacesTxt = "Workspaces";

    //Start Future Features
    [ObservableProperty]
    private string _smartFoldersTxt = "Smart Folders";
    [ObservableProperty]
    private string _userNotesTxt = "Notes";
    [ObservableProperty]
    private string _inspoTagsTxt = "Inspo Tags";
    //End Future Features

    [ObservableProperty]
    private string _userSettingsTxt = "Settings";
    
    // Settings View popup//
    [RelayCommand]
    private void ShowSettings()
    {
        // Handle settings button click
        // Perhaps show a settings popup or navigate to settings view
    }


    // Top Dynamic - Options Area - If System Files, show filepath
    // View 1 - System Files 
    // View 2 - Monitored Folders
    [ObservableProperty]
    private string currentFilePath = string.Empty;

    // Collection for workspaces (for View2)
    [ObservableProperty]
    private ObservableCollection<WorkspaceItem> workspaces = new();

    // Currently selected workspace
    [ObservableProperty]
    private WorkspaceItem? selectedWorkspace;

    // Constructor initializes default state
    public MainViewModel()
    {
        UpdateViewState(ViewType.SystemFiles);
    }

    // Updates UI based on selected view
    private void UpdateViewState(ViewType viewType)
    {
        CurrentView = viewType;

        switch (viewType)
        {
            case ViewType.SystemFiles:
                CurrentWorkspaceTxt = "System Files";
                SaveCurrentWorkspaceTxt = "Add to Monitored";
                //UpdateCurrentWorkspaceTxt = "Override";
                RemoveCurrentWorkspaceTxt = "Remove";
                break;

            case ViewType.MonitoredFolders:
                CurrentWorkspaceTxt = "Monitored Folders";
                SaveCurrentWorkspaceTxt = "Save Workspace";
                UpdateCurrentWorkspaceTxt = "Update";
                RemoveCurrentWorkspaceTxt = "Remove";
                break;
        }
        // Notify that multiple properties changed
        OnPropertyChanged(nameof(IsSystemFilesView));
        OnPropertyChanged(nameof(IsMonitoredFoldersView));
    }

    // View helpers for visibility bindings
    public bool IsSystemFilesView => CurrentView == ViewType.SystemFiles;
    public bool IsMonitoredFoldersView => CurrentView == ViewType.MonitoredFolders;

    // Command handlers for icon button clicks
    [RelayCommand]
    private void ShowSystemFiles()
    {
        UpdateViewState(ViewType.SystemFiles);
    }

    [RelayCommand]
    private void ShowWorkspaces()
    {
        // Handle workspaces button click
        // Perhaps show workspace selection or management UI
    }
    [RelayCommand]
    private void ShowMonitoredFiles()
    {
        UpdateViewState(ViewType.MonitoredFolders);
    }

    // Additional commands for the workspace actions
    [RelayCommand]
    private async Task SaveWorkspace()
    {
        if (CurrentView == ViewType.SystemFiles)
        {
            // Logic for "Add to Monitored" action
            // Show green confirm popup
            await ShowConfirmDialog("Add to monitored folders?");
        }
        else
        {
            // Logic for "Save Workspace" action
            // Show name popup
            var name = await ShowNameInputDialog("Enter workspace name");
            if (!string.IsNullOrEmpty(name))
            {
                // Save workspace with the given name
            }
        }
    }

    [RelayCommand]
    private async Task UpdateWorkspace()
    {
        // Show user override popup for both views
        var result = await ShowConfirmDialog("Override existing item?");
        if (result)
        {
            // Perform update based on current view
        }
    }

    [RelayCommand]
    private async Task RemoveWorkspace()
    {
        // Show user check popup for both views
        var result = await ShowConfirmDialog("Are you sure you want to remove this item?");
        if (result)
        {
            // Perform removal based on current view
        }
    }

    // Helper methods for dialogs - implementation depends on UI framework
    private Task<bool> ShowConfirmDialog(string message)
    {
        // Implementation depends on your UI framework
        // For Avalonia, you would use DialogService or a similar approach
        return Task.FromResult(false); // Placeholder
    }

    private Task<string> ShowNameInputDialog(string prompt)
    {
        // Implementation depends on your UI framework
        return Task.FromResult(string.Empty); // Placeholder
    }

    // If you need workspace selection handling
    partial void OnSelectedWorkspaceChanged(WorkspaceItem? value)
    {
        if (value != null)
        {
            // Update the monitored items listview based on selected workspace
            CurrentFilePath = value.Path;
        }
    }
}

// Enum to track the current view state
public enum ViewType
{
    SystemFiles,
    MonitoredFolders
}

// Model for workspace items
public class WorkspaceItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public ObservableCollection<string> MonitoredFiles { get; set; } = new();
}
// Center Dynamic - Options Area
// Contents/Rows will change based on loaded view from left features
//View 1 - System Files UI + Treeview
//dynamic object scale
//dynamic search box for folder name search within file tree
//dynamic refresh button
//dynamic object for file treeview

//View 2 - Monitored Files UI + Listview
//dynamic object scale
//dynamic search box for folder name search within file tree
//dynamic refresh button
//dynamic object for folder listview

// Right Dynamic - Gallery View Area
// Contents/Rows will change based on loaded view from left features
//View 1 - System Gallery View
//dynamic data

//View 2 - Monitored Gallery View
//dynamic data

//View 3 - Workspaces Monitored list View
//dynamic data / list

