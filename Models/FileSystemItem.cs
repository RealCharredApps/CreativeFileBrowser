using System;
using System.IO;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace CreativeFileBrowser.Models
{
    /// <summary>
    /// Represents an item in the file system (file or directory)
    /// </summary>
    public class FileSystemItem : ObservableObject
{
    private string _name = string.Empty;
    private string _fullPath = string.Empty;
    private bool _isDirectory;
    private bool _isExpanded;
    private ObservableCollection<FileSystemItem>? _children;
    public enum DriveTypeInfo
    {
        Fixed,
        Removable,
        Network,
        SharePoint,
        Unknown
    }

    private DriveTypeInfo _driveType = DriveTypeInfo.Unknown;
    public DriveTypeInfo DriveType
    {
        get => _driveType;
        set => SetProperty(ref _driveType, value);
    }

    public string Name 
    { 
        get => _name; 
        set => SetProperty(ref _name, value); 
    }

    public string FullPath 
    { 
        get => _fullPath; 
        set => SetProperty(ref _fullPath, value); 
    }

    public bool IsDirectory 
    { 
        get => _isDirectory; 
        set => SetProperty(ref _isDirectory, value); 
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public ObservableCollection<FileSystemItem> Children
    {
        get => _children ??= new ObservableCollection<FileSystemItem>();
        set => SetProperty(ref _children, value);
    }
        // Removed redundant DriveType property to resolve conflict
    }
}