using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CreativeFileBrowser.Models;
using CreativeFileBrowser.ViewModels;

namespace CreativeFileBrowser.ViewModels
{
    public class FileExplorerViewModel : ObservableObject
    {
        private ObservableCollection<FileSystemItem> _rootItems = new();
        private FileSystemItem? _selectedItem;
        private string _currentPath = string.Empty;
        //private object _filePreview = string.Empty;
        private FilePreviewViewModel? _filePreviewViewModel;
        public FilePreviewViewModel FilePreviewViewModel
        {
            get => _filePreviewViewModel ??= new FilePreviewViewModel();
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
                    CurrentPath = value.FullPath;
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

        // Command to expand a directory
        public IRelayCommand<FileSystemItem?> ExpandDirectoryCommand { get; }

        public FileExplorerViewModel()
        {
            LoadDrivesCommand = new RelayCommand(LoadDrives);
            ExpandDirectoryCommand = new RelayCommand<FileSystemItem?>(ExpandDirectory);
            LoadDrives();
        }

        // Load system drives as root items
        private void LoadDrives()
        {
            RootItems.Clear();

            try
            {
                // Get all drives in the system
                foreach (var drive in Directory.GetLogicalDrives())
                {
                    var driveItem = new FileSystemItem
                    {
                        Name = drive,
                        FullPath = drive,
                        IsDirectory = true,
                        IsExpanded = false,
                    };

                    RootItems.Add(driveItem);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Debug.WriteLine($"Error loading drives: {ex.Message}");
            }
        }

        // Expand a directory to show its children
        private void ExpandDirectory(FileSystemItem? directory)
        {
            if (directory == null || !directory.IsDirectory)
                return;

            try
            {
                directory.Children.Clear();

                // Add directories
                foreach (var dir in Directory.GetDirectories(directory.FullPath))
                {
                    try
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        directory.Children.Add(new FileSystemItem
                        {
                            Name = dirInfo.Name,
                            FullPath = dir,
                            IsDirectory = true
                        });
                    }
                    catch
                    {
                        // Skip directories we can't access
                    }
                }

                // Add files
                foreach (var file in Directory.GetFiles(directory.FullPath))
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        directory.Children.Add(new FileSystemItem
                        {
                            Name = fileInfo.Name,
                            FullPath = file,
                            IsDirectory = false
                        });
                    }
                    catch
                    {
                        // Skip files we can't access
                    }
                }

                directory.IsExpanded = true;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Debug.WriteLine($"Error expanding directory: {ex.Message}");
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
    }
}