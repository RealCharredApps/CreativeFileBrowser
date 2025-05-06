using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CreativeFileBrowser.Models;
using CreativeFileBrowser.ViewModels;
using static CreativeFileBrowser.Models.FileSystemItem;

namespace CreativeFileBrowser.ViewModels
{
    public class FileExplorerViewModel : ObservableObject
    {
        private ObservableCollection<FileSystemItem> _rootItems = new();
        private FileSystemItem? _selectedItem;
        private string _currentPath = string.Empty;
        //private object _filePreview = string.Empty;
        private readonly Stack<string> _backStack = new();
        private readonly Stack<string> _forwardStack = new();
        private FilePreviewViewModel? _filePreviewViewModel;
        public FilePreviewViewModel FilePreviewViewModel
        {
            get => _filePreviewViewModel ??= new FilePreviewViewModel();
        }
        private bool _isCopySuccessful;
        public bool IsCopySuccessful
        {
            get => _isCopySuccessful;
            set => SetProperty(ref _isCopySuccessful, value);
        }

        // Collection of root drives/directories
        public ObservableCollection<FileSystemItem> RootItems
        {
            get => _rootItems ??= new ObservableCollection<FileSystemItem>()!;
            set => SetProperty(ref _rootItems, value);
        }

        // Currently selected item in the tree
        public FileSystemItem SelectedItem
        {
            get => _selectedItem!;
            set
            {
                if (SetProperty(ref _selectedItem, value) && value != null)
                {
                    // Save previous location to back stack (if different)
                    if (!string.IsNullOrEmpty(CurrentPath) &&
                        !CurrentPath.Equals(value.FullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _backStack.Push(CurrentPath);
                        _forwardStack.Clear();

                        // Update commands
                        (NavigateBackCommand as RelayCommand)?.NotifyCanExecuteChanged();
                        (NavigateForwardCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    }

                    // Update current path
                    CurrentPath = value.FullPath;

                    // Update file preview
                    LoadPreview(value);
                }
            }
        }

        // Current path string
        public string CurrentPath
        {
            get => _currentPath;
            set => SetProperty(ref _currentPath, value);
        }

        // Command to load drives
        public IRelayCommand LoadDrivesCommand { get; }
        public IRelayCommand CopySelectedPathCommand { get; }
        public IRelayCommand NavigateBackCommand { get; }
        public IRelayCommand NavigateForwardCommand { get; }
        public IRelayCommand NavigateUpCommand { get; }


        // Command to expand a directory
        public IRelayCommand<FileSystemItem?> ExpandDirectoryCommand { get; }

        public FileExplorerViewModel()
        {
            LoadDrivesCommand = new RelayCommand(LoadDrives);
            ExpandDirectoryCommand = new RelayCommand<FileSystemItem?>(ExpandDirectory);
            CopySelectedPathCommand = new RelayCommand(CopySelectedPath, CanCopySelectedPath);

            // Navigation commands
            NavigateBackCommand = new RelayCommand(NavigateBack, CanNavigateBack);
            NavigateForwardCommand = new RelayCommand(NavigateForward, CanNavigateForward);
            NavigateUpCommand = new RelayCommand(NavigateUp, CanNavigateUp);

            LoadDrives();
        }

        private void LoadDrives()
        {
            RootItems.Clear();

            try
            {
                // Add local system drives
                LoadLocalDrives();

                // Add removable drives (USB, SSD, SD cards)
                LoadRemovableDrives();

                // Add network drives and server connections
                LoadNetworkDrives();

                // Add SharePoint connections if available
                LoadSharePointConnections();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Debug.WriteLine($"Error loading drives: {ex.Message}");
            }
        }

        private void LoadLocalDrives()
        {
            if (OperatingSystem.IsMacOS())
            {
                // Add main Macintosh HD drive only if not excluded
                string macHDPath = "/";
                if (!FolderExclusionRules.ShouldExcludeFolder("Macintosh HD", macHDPath))
                {
                    var macHD = new FileSystemItem
                    {
                        Name = "Macintosh HD",
                        FullPath = macHDPath,
                        IsDirectory = true,
                        IsExpanded = false
                    };
                    RootItems.Add(macHD);
                    PreloadSystemFolders(macHD);
                }
            }
            else if (OperatingSystem.IsWindows())
            {
                // Get fixed drives (C:, D:, etc)
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed))
                {
                    string driveName = string.IsNullOrEmpty(drive.VolumeLabel)
                        ? $"{drive.Name} (Local Disk)"
                        : $"{drive.Name} ({drive.VolumeLabel})";

                    // Only add drives that aren't excluded
                    if (!FolderExclusionRules.ShouldExcludeFolder(driveName, drive.Name))
                    {
                        var driveItem = new FileSystemItem
                        {
                            Name = driveName.TrimEnd('\\'),
                            FullPath = drive.Name,
                            IsDirectory = true,
                            IsExpanded = false,
                            DriveType = DriveTypeInfo.Fixed
                        };

                        RootItems.Add(driveItem);
                        PreloadSystemFolders(driveItem);
                    }
                }
            }
            else // Linux or other OS
            {
                // Fallback to root directory
                var rootItem = new FileSystemItem
                {
                    Name = "Root",
                    FullPath = "/",
                    IsDirectory = true,
                    IsExpanded = false,
                    DriveType = DriveTypeInfo.Fixed
                };
                RootItems.Add(rootItem);

                PreloadSystemFolders(rootItem);
            }
        }

        private void LoadRemovableDrives()
        {
            // Get all removable drives
            var removableDrives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Removable && d.IsReady);

            foreach (var drive in removableDrives)
            {
                try
                {
                    string driveName = string.IsNullOrEmpty(drive.VolumeLabel)
                        ? $"{drive.Name} (Removable Storage)"
                        : $"{drive.Name} ({drive.VolumeLabel})";

                    var driveItem = new FileSystemItem
                    {
                        Name = driveName.TrimEnd('\\'),
                        FullPath = drive.Name,
                        IsDirectory = true,
                        IsExpanded = false,
                        DriveType = DriveTypeInfo.Removable
                    };

                    RootItems.Add(driveItem);
                }
                catch (IOException)
                {
                    // Handle case where drive may have been removed
                    Debug.WriteLine($"Could not access removable drive {drive.Name}");
                }
            }

            // For macOS, check for mounted volumes
            if (OperatingSystem.IsMacOS())
            {
                try
                {
                    string volumesPath = "/Volumes";
                    if (Directory.Exists(volumesPath))
                    {
                        foreach (var volume in Directory.GetDirectories(volumesPath))
                        {
                            // Skip Macintosh HD as it's already added
                            if (Path.GetFileName(volume) == "Macintosh HD")
                                continue;

                            var volumeItem = new FileSystemItem
                            {
                                Name = Path.GetFileName(volume),
                                FullPath = volume,
                                IsDirectory = true,
                                IsExpanded = false,
                                DriveType = DriveTypeInfo.Removable
                            };

                            RootItems.Add(volumeItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading Mac volumes: {ex.Message}");
                }
            }
        }

        private void LoadNetworkDrives()
        {
            // Get all network drives
            var networkDrives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Network && d.IsReady);

            foreach (var drive in networkDrives)
            {
                try
                {
                    string driveName = string.IsNullOrEmpty(drive.VolumeLabel)
                        ? $"{drive.Name} (Network Drive)"
                        : $"{drive.Name} ({drive.VolumeLabel})";

                    var driveItem = new FileSystemItem
                    {
                        Name = driveName.TrimEnd('\\'),
                        FullPath = drive.Name,
                        IsDirectory = true,
                        IsExpanded = false,
                        DriveType = DriveTypeInfo.Network
                    };

                    RootItems.Add(driveItem);
                }
                catch (IOException)
                {
                    // Handle case where network drive is unavailable
                    Debug.WriteLine($"Could not access network drive {drive.Name}");
                }
            }
        }

        private void LoadSharePointConnections()
        {
            // Windows: Check for SharePoint mapped drives using OneDrive for Business
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    // Common location for OneDrive for Business / SharePoint
                    string oneDrivePath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                        "OneDrive - ");

                    // Get the directory that should contain OneDrive folders
                    string oneDriveParentPath = Path.GetDirectoryName(oneDrivePath) ?? string.Empty;

                    // If the directory exists, find all OneDrive for Business directories
                    if (!string.IsNullOrEmpty(oneDriveParentPath) && Directory.Exists(oneDriveParentPath))
                    {
                        var oneDriveDirs = Directory.GetDirectories(
                            oneDriveParentPath,
                            "OneDrive - *");

                        foreach (var dir in oneDriveDirs)
                        {
                            // Extract organization name from path
                            string orgName = Path.GetFileName(dir).Substring("OneDrive - ".Length);

                            var sharePointItem = new FileSystemItem
                            {
                                Name = $"SharePoint - {orgName}",
                                FullPath = dir,
                                IsDirectory = true,
                                IsExpanded = false,
                                DriveType = DriveTypeInfo.SharePoint
                            };

                            RootItems.Add(sharePointItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading SharePoint connections: {ex.Message}");
                }
            }

            // Mac: Check for SharePoint mounted in Finder
            if (OperatingSystem.IsMacOS())
            {
                try
                {
                    // Common location for mounted SharePoint locations on Mac
                    string[] possibleSharePointPaths = {
            "/Volumes/SharePoint",
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library/Application Support/Microsoft/Office/SharePoint")
        };

                    foreach (var path in possibleSharePointPaths)
                    {
                        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                        {
                            foreach (var dir in Directory.GetDirectories(path))
                            {
                                var sharePointItem = new FileSystemItem
                                {
                                    Name = $"SharePoint - {Path.GetFileName(dir)}",
                                    FullPath = dir,
                                    IsDirectory = true,
                                    IsExpanded = false,
                                    DriveType = DriveTypeInfo.SharePoint
                                };

                                RootItems.Add(sharePointItem);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading Mac SharePoint connections: {ex.Message}");
                }
            }
        }

        private void PreloadSystemFolders(FileSystemItem driveItem)
        {
            try
            {
                string[] standardFolders;

                if (OperatingSystem.IsMacOS())
                {
                    standardFolders = new[] { "Applications", "System", "Library", "Users" };
                }
                else if (OperatingSystem.IsWindows())
                {
                    standardFolders = new[] { "Program Files", "Program Files (x86)", "Windows", "Users" };
                }
                else // Linux or other OS
                {
                    standardFolders = new[] { "bin", "etc", "home", "usr", "var" };
                }

                foreach (var folder in standardFolders)
                {
                    string folderPath = Path.Combine(driveItem.FullPath, folder);

                    // Check if folder exists and is not excluded before adding
                    if (Directory.Exists(folderPath) &&
                        !FolderExclusionRules.ShouldExcludeFolder(folder, folderPath))
                    {
                        driveItem.Children.Add(new FileSystemItem
                        {
                            Name = folder,
                            FullPath = folderPath,
                            IsDirectory = true,
                            IsExpanded = false
                        });
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle permission denied
                driveItem.Children.Add(new FileSystemItem
                {
                    Name = "Access Denied",
                    FullPath = driveItem.FullPath,
                    IsDirectory = false
                });
            }
            catch (Exception ex)
            {
                // Handle other errors
                Debug.WriteLine($"Error preloading system folders: {ex.Message}");
            }
        }

        private void ExpandDirectory(FileSystemItem? directory)
        {
            if (directory == null || !directory.IsDirectory)
                return;

            try
            {
                // Check if the drive is still accessible (important for removable and network drives)
                if (directory.DriveType == DriveTypeInfo.Removable ||
                    directory.DriveType == DriveTypeInfo.Network ||
                    directory.DriveType == DriveTypeInfo.SharePoint)
                {
                    if (!Directory.Exists(directory.FullPath))
                    {
                        directory.Children.Clear();
                        directory.Children.Add(new FileSystemItem
                        {
                            Name = "Drive not available",
                            FullPath = directory.FullPath,
                            IsDirectory = false
                        });
                        directory.IsExpanded = true;
                        return;
                    }
                }

                // Clear existing children
                directory.Children.Clear();

                // Add directories
                foreach (var dir in Directory.GetDirectories(directory.FullPath))
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        string dirName = dirInfo.Name;

                        // Skip hidden directories on Mac/Linux 
                        if ((OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()) &&
                            dirName.StartsWith("."))
                            continue;

                        // Skip excluded folders based on rules
                        if (FolderExclusionRules.ShouldExcludeFolder(dirName, dir))
                            continue;

                        directory.Children.Add(new FileSystemItem
                        {
                            Name = dirName,
                            FullPath = dir,
                            IsDirectory = true
                        });
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip directories we can't access
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error accessing directory {dir}: {ex.Message}");
                    }
                }

                // Add files
                foreach (var file in Directory.GetFiles(directory.FullPath))
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);

                        // Skip hidden files on Mac/Linux
                        if ((OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()) &&
                            fileInfo.Name.StartsWith("."))
                            continue;

                        directory.Children.Add(new FileSystemItem
                        {
                            Name = fileInfo.Name,
                            FullPath = file,
                            IsDirectory = false
                        });
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we can't access
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error accessing file {file}: {ex.Message}");
                    }
                }

                directory.IsExpanded = true;
            }
            catch (UnauthorizedAccessException)
            {
                // Handle permission denied
                directory.Children.Clear();
                directory.Children.Add(new FileSystemItem
                {
                    Name = "Access Denied",
                    FullPath = directory.FullPath,
                    IsDirectory = false
                });
                directory.IsExpanded = true;
            }
            catch (IOException ex)
            {
                // Handle IO exceptions (common with removable/network drives)
                directory.Children.Clear();
                directory.Children.Add(new FileSystemItem
                {
                    Name = $"Device Error: {ex.Message}",
                    FullPath = directory.FullPath,
                    IsDirectory = false
                });
                directory.IsExpanded = true;
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Debug.WriteLine($"Error expanding directory: {ex.Message}");
                directory.Children.Clear();
                directory.Children.Add(new FileSystemItem
                {
                    Name = $"Error: {ex.Message}",
                    FullPath = directory.FullPath,
                    IsDirectory = false
                });
                directory.IsExpanded = true;
            }
        }

        // Load a preview of the selected file
        private void LoadPreview(FileSystemItem item)
        {
            if (item == null)
                return;

            // For directories, show number of items
            if (item.IsDirectory)
            {
                try
                {
                    int fileCount = Directory.GetFiles(item.FullPath).Length;
                    int dirCount = Directory.GetDirectories(item.FullPath).Length;
                    FilePreviewViewModel.PreviewContent = $"{dirCount} directories, {fileCount} files";
                    _ = FilePreviewViewModel.LoadFolderContentsAsync(item.FullPath);
                }
                catch
                {
                    FilePreviewViewModel.PreviewContent = "Access denied";
                }
                return;
            }

            // For files, show basic file info and try to generate a preview
            try
            {
                // Just set the FilePath, the FilePreviewViewModel will handle the rest
                FilePreviewViewModel.FilePath = item.FullPath;
            }
            catch (Exception ex)
            {
                FilePreviewViewModel.PreviewContent = $"Unable to generate preview: {ex.Message}";
            }
        }

        private bool CanCopySelectedPath() => SelectedItem != null;

        private async void CopySelectedPath()
        {
            if (SelectedItem == null)
                return;

            try
            {
                var topLevel = Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime;
                var mainWindow = topLevel?.MainWindow;

                if (mainWindow?.Clipboard != null)
                {
                    await mainWindow.Clipboard.SetTextAsync(SelectedItem.FullPath);

                    // Signal success
                    IsCopySuccessful = true;

                    // Hide after delay
                    await Task.Delay(3000);
                    IsCopySuccessful = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error copying path: {ex.Message}");
            }
        }

        // Navigation command handlers
        private bool CanNavigateBack() => _backStack.Count > 0;
        private bool CanNavigateForward() => _forwardStack.Count > 0;
        private bool CanNavigateUp() => !string.IsNullOrEmpty(CurrentPath) &&
            Directory.GetParent(CurrentPath) != null;

        private void NavigateBack()
        {
            if (_backStack.Count > 0)
            {
                // Save current to forward stack
                if (!string.IsNullOrEmpty(CurrentPath))
                    _forwardStack.Push(CurrentPath);

                // Navigate to previous
                string previousPath = _backStack.Pop();
                NavigateToPathWithoutHistory(previousPath);

                // Update commands
                (NavigateBackCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (NavigateForwardCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        private void NavigateForward()
        {
            if (_forwardStack.Count > 0)
            {
                // Save current to back stack
                if (!string.IsNullOrEmpty(CurrentPath))
                    _backStack.Push(CurrentPath);

                // Navigate to next
                string nextPath = _forwardStack.Pop();
                NavigateToPathWithoutHistory(nextPath);

                // Update commands
                (NavigateBackCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (NavigateForwardCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        private void NavigateUp()
        {
            if (string.IsNullOrEmpty(CurrentPath))
                return;

            var parent = Directory.GetParent(CurrentPath);
            if (parent != null)
            {
                // Save current for back navigation
                _backStack.Push(CurrentPath);
                _forwardStack.Clear();

                // Navigate to parent
                NavigateToPathWithoutHistory(parent.FullName);

                // Update commands
                (NavigateBackCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (NavigateForwardCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        // Helper method to navigate without affecting history
        private void NavigateToPathWithoutHistory(string path)
        {
            if (!Directory.Exists(path))
                return;

            // Find or create the FileSystemItem for this path
            FileSystemItem? targetItem = FindFileSystemItemByPath(path);

            if (targetItem != null)
            {
                // Use existing item
                SelectedItem = targetItem;
                EnsureParentsExpanded(targetItem);
            }
            else
            {
                // Path not in current tree, must rebuild tree
                CurrentPath = path;

                // Create a temporary item to represent this location
                var tempItem = new FileSystemItem
                {
                    Name = Path.GetFileName(path) ?? path,
                    FullPath = path,
                    IsDirectory = true
                };

                // Expand it to populate children
                ExpandDirectory(tempItem);

                // Update selected item
                SelectedItem = tempItem;
            }
        }

        // Helper to find a FileSystemItem by path in our tree
        private FileSystemItem? FindFileSystemItemByPath(string path)
        {
            // Check each root item first
            foreach (var rootItem in RootItems)
            {
                if (rootItem.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase))
                    return rootItem;

                // Check children recursively
                var item = FindFileSystemItemByPathRecursive(rootItem, path);
                if (item != null)
                    return item;
            }

            return null;
        }

        private FileSystemItem? FindFileSystemItemByPathRecursive(
            FileSystemItem parent, string path)
        {
            foreach (var child in parent.Children)
            {
                if (child.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase))
                    return child;

                if (child.IsDirectory)
                {
                    var item = FindFileSystemItemByPathRecursive(child, path);
                    if (item != null)
                        return item;
                }
            }

            return null;
        }

        // Helper to ensure all parents of an item are expanded
        private void EnsureParentsExpanded(FileSystemItem item)
        {
            string itemPath = item.FullPath;

            foreach (var rootItem in RootItems)
            {
                if (itemPath.StartsWith(rootItem.FullPath, StringComparison.OrdinalIgnoreCase))
                {
                    rootItem.IsExpanded = true;
                    EnsurePathExpanded(rootItem, itemPath);
                    break;
                }
            }
        }

        private void EnsurePathExpanded(FileSystemItem parent, string targetPath)
        {
            foreach (var child in parent.Children)
            {
                if (!child.IsDirectory)
                    continue;

                if (targetPath.StartsWith(child.FullPath, StringComparison.OrdinalIgnoreCase))
                {
                    child.IsExpanded = true;

                    // Make sure this item's children are loaded
                    if (child.Children.Count == 0)
                        ExpandDirectory(child);

                    // Continue down the path
                    EnsurePathExpanded(child, targetPath);
                    break;
                }
            }
        }


    }
}