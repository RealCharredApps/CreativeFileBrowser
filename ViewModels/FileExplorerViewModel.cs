using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CreativeFileBrowser.Models;

namespace CreativeFileBrowser.ViewModels
{
    public class FileExplorerViewModel : ObservableObject
    {
        private ObservableCollection<FileSystemItem> _rootItems = new();
        private FileSystemItem? _selectedItem;
        private string _currentPath = string.Empty;
        private object _filePreview = string.Empty;

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

        // Preview of selected file
        public object FilePreview
        {
            get => _filePreview;
            set => SetProperty(ref _filePreview, value);
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
                    var dirInfo = new DirectoryInfo(item.FullPath);
                    int fileCount = Directory.GetFiles(item.FullPath).Length;
                    int dirCount = Directory.GetDirectories(item.FullPath).Length;
                    FilePreview = $"{dirCount} directories, {fileCount} files";
                }
                catch
                {
                    FilePreview = "Access denied";
                }
                return;
            }

            // For files, show basic file info and try to generate a preview
            try
            {
                var fileInfo = new FileInfo(item.FullPath);
                string extension = fileInfo.Extension.ToLower();

                // Simple preview based on file type
                switch (extension)
                {
                    case ".txt":
                    case ".log":
                    case ".cs":
                    case ".xml":
                    case ".json":
                        // For text files, show first few lines
                        using (var reader = new StreamReader(item.FullPath))
                        {
                            string preview = "";
                            for (int i = 0; i < 10 && !reader.EndOfStream; i++)
                                preview += reader.ReadLine() + Environment.NewLine;

                            FilePreview = preview;
                        }
                        break;

                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                    case ".bmp":
                        // For images, we'd ideally load a thumbnail
                        FilePreview = "Image file: " + fileInfo.Length / 1024 + " KB";
                        break;

                    default:
                        // For other files, just show info
                        FilePreview = $"File size: {fileInfo.Length / 1024} KB\nLast modified: {fileInfo.LastWriteTime}";
                        break;
                }
            }
            catch
            {
                FilePreview = "Unable to generate preview";
            }
        }
    }
}