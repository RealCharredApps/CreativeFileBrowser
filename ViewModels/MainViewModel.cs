using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace CreativeFileBrowser.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // Title Top Area
    [ObservableProperty]
    private string _title = "FH";
    // SubTitle Top Area
    [ObservableProperty]
    private string _subtitle = "Creative File Browser";
    [ObservableProperty]
    private ObservableCollection<SimpleDriveInfo> _drives;
    // Current view state - determines which UI elements are visible
    [ObservableProperty]
    private ViewType currentView = ViewType.SystemFiles;

    // Left Options Area
    //Each Feature item will have label + svg.icon button above

    // Text bindings for the workspace bar
    [ObservableProperty]
    private string currentWorkspaceTxt = string.Empty;
    [ObservableProperty]
    private string addToCurrentWorkspaceTxt = string.Empty;
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

    /*Start Future Features
        [ObservableProperty]
        private string _smartFoldersTxt = "Smart Folders";
        [ObservableProperty]
        private string _userNotesTxt = "Notes";
        [ObservableProperty]
        private string _inspoTagsTxt = "Inspo Tags";
    */
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
    private FileExplorerViewModel? _fileExplorer;
    public FileExplorerViewModel FileExplorer => _fileExplorer ??= new FileExplorerViewModel();
    // View 2 - Monitored Folders
    [ObservableProperty]
    private string currentFilePath = string.Empty;

    // Collection for workspaces (for View2)
    [ObservableProperty]
    private ObservableCollection<WorkspaceItem> workspaces = new();

    // Currently selected workspace
    [ObservableProperty]
    private WorkspaceItem? selectedWorkspace;

    // Command to load drives
    public IRelayCommand LoadDrivesCommand { get; }

    // Constructor initializes default state
    public MainViewModel()
    {
        UpdateViewState(ViewType.SystemFiles);
        // Initialize the collection
        Drives = new ObservableCollection<SimpleDriveInfo>();
        // Initialize the FileExplorer
        _fileExplorer = new FileExplorerViewModel();
        // Initialize the LoadDrivesCommand
        LoadDrivesCommand = new RelayCommand(LoadDrives);
        // Load drives when created
        LoadDrivesCommand.Execute(null);
    }

    // Updates UI based on selected view
    private void UpdateViewState(ViewType viewType)
    {
        CurrentView = viewType;

        switch (viewType)
        {
            case ViewType.SystemFiles:
                CurrentWorkspaceTxt = "System Files:";
                AddToCurrentWorkspaceTxt = "Monitor Folder";
                SaveCurrentWorkspaceTxt = string.Empty;
                UpdateCurrentWorkspaceTxt = string.Empty;
                RemoveCurrentWorkspaceTxt = string.Empty;
                CurrentFilePath = "CurrentFilePath";
                break;

            case ViewType.MonitoredFolders:
                CurrentWorkspaceTxt = "Monitored Folders:";
                AddToCurrentWorkspaceTxt = string.Empty;
                SaveCurrentWorkspaceTxt = "Save New";
                UpdateCurrentWorkspaceTxt = "Update Current";
                RemoveCurrentWorkspaceTxt = "Remove Folder";
                CurrentFilePath = "CurrentFilePath";
                break;

            case ViewType.WorkspaceFolders:
                CurrentWorkspaceTxt = "Workspace Folders:";
                AddToCurrentWorkspaceTxt = string.Empty;
                SaveCurrentWorkspaceTxt = "Save New";
                UpdateCurrentWorkspaceTxt = "Update Current";
                RemoveCurrentWorkspaceTxt = "Remove Workspace";
                CurrentFilePath = "CurrentFilePath";
                break;
        }
        // Notify that multiple properties changed
        OnPropertyChanged(nameof(IsSystemFilesView));
        OnPropertyChanged(nameof(IsMonitoredFoldersView));
        OnPropertyChanged(nameof(IsWorkspaceFoldersView));
    }

    // View helpers for visibility bindings
    public bool IsSystemFilesView => CurrentView == ViewType.SystemFiles;
    public bool IsMonitoredFoldersView => CurrentView == ViewType.MonitoredFolders;
    public bool IsWorkspaceFoldersView => CurrentView == ViewType.WorkspaceFolders;

    // Command handlers for icon button clicks
    [RelayCommand]
    private void ShowSystemFiles()
    {
        UpdateViewState(ViewType.SystemFiles);
    }
    [RelayCommand]
    private void ShowMonitoredFiles()
    {
        UpdateViewState(ViewType.MonitoredFolders);
    }
    [RelayCommand]
    private void ShowWorkspaceFolders()
    {
        UpdateViewState(ViewType.WorkspaceFolders);
    }


    /// <summary>
    /// Loads all available drives on the system
    /// </summary>
    private void LoadDrives()
    {
        Drives.Clear();

        // Get all drives and add to the collection
        foreach (var drive in DriveInfo.GetDrives())
        {
            Drives.Add(new SimpleDriveInfo(drive));
        }
    }

    /// <summary>
    /// Command to refresh the drive list
    /// </summary>
    [RelayCommand]
    private void RefreshDrives()
    {
        LoadDrives();
    }

    // Simple drive info class with user-friendly properties
    public class SimpleDriveInfo
    {
        public string DriveLetter { get; set; }
        public DriveType DriveType { get; set; }
        public bool IsReady { get; set; }
        public string VolumeLabel { get; set; }
        public DriveInfo OriginalDriveInfo { get; set; }

        /// <summary>
        /// Gets default label for drives with no volume label
        /// </summary>
        private string GetDefaultLabel(DriveInfo drive)
        {
            return drive.DriveType switch
            {
                DriveType.Fixed => "Local Disk",
                DriveType.Removable => "Removable Drive",
                DriveType.Network => "Network Drive",
                _ => "Drive"
            };
        }
        public SimpleDriveInfo(DriveInfo driveInfo)
        {
            DriveLetter = driveInfo.Name;
            DriveType = driveInfo.DriveType;
            OriginalDriveInfo = driveInfo;
            VolumeLabel = string.Empty; // Initialize with a default value

            // Only access these properties if the drive is ready
            if (driveInfo.IsReady)
            {
                IsReady = true;
                VolumeLabel = driveInfo.VolumeLabel;
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    Console.WriteLine($"Drive {DriveLetter} is ready with label: {VolumeLabel}");
                }
            }
        }
    }



    // Additional commands for the workspace actions
    [RelayCommand]
    private async Task AddToWorkspace()
    {
        // Show user override popup for both views
        var result = await ShowConfirmDialog("Folder Added to Monitored in Current Workspace");
        if (result)
        {
            // Perform update based on current view
        }
    }
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
    MonitoredFolders,
    WorkspaceFolders
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

