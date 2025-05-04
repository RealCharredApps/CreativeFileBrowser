using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CreativeFileBrowser.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CreativeFileBrowser.ViewModels
{
    public class FilePreviewViewModel : ObservableObject
    {
        private object _previewContent = "Select a file to preview";
        private string _filePath = string.Empty;
        private string _currentDirectory = string.Empty;
        private ObservableCollection<ImageItem> _imageGallery = new();
        private bool _isGalleryView = false;
        private readonly ThumbnailService _thumbnailService;
        private CancellationTokenSource? _loadCancellation;

        public FilePreviewViewModel()
        {
            _thumbnailService = new ThumbnailService(100);
        }

        public object PreviewContent
        {
            get => _previewContent;
            set => SetProperty(ref _previewContent, value);
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (SetProperty(ref _filePath, value) && !string.IsNullOrEmpty(value))
                {
                    // Start preview loading
                    _ = LoadPreviewAsync(value);

                    // Update current directory
                    string directory = Path.GetDirectoryName(value) ?? string.Empty;
                    if (_currentDirectory != directory)
                    {
                        _currentDirectory = directory;
                        _ = LoadImageGalleryAsync(directory);
                    }
                }
            }
        }

        public ObservableCollection<ImageItem> ImageGallery
        {
            get => _imageGallery;
            private set => SetProperty(ref _imageGallery, value);
        }

        public bool IsGalleryView
        {
            get => _isGalleryView;
            set => SetProperty(ref _isGalleryView, value);
        }

        public async Task LoadFolderContentsAsync(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
                return;
                
            // Cancel any ongoing loading
            _loadCancellation?.Cancel();
            _loadCancellation = new CancellationTokenSource();
            var token = _loadCancellation.Token;
            
            try
            {
                // Clear existing gallery
                ImageGallery.Clear();
                
                // Check if folder contains images
                var imageFiles = Directory.GetFiles(folderPath)
                    .Where(file => IsImageFile(Path.GetExtension(file).ToLowerInvariant()))
                    .Take(100) // Limit to prevent memory issues
                    .ToList();
                    
                if (imageFiles.Count > 0)
                {
                    IsGalleryView = true;
                    
                    // Load thumbnails in batches to prevent UI freezing
                    const int batchSize = 10;
                    for (int i = 0; i < imageFiles.Count; i += batchSize)
                    {
                        if (token.IsCancellationRequested)
                            break;
                            
                        var batch = imageFiles.Skip(i).Take(batchSize);
                        foreach (var imagePath in batch)
                        {
                            if (token.IsCancellationRequested) 
                                break;
                                
                            var item = new ImageItem
                            {
                                FilePath = imagePath,
                                FileName = Path.GetFileName(imagePath)
                            };
                            
                            ImageGallery.Add(item);
                        }
                        
                        // Small delay to allow UI to update
                        await Task.Delay(10, token);
                    }
                }
                else
                {
                    IsGalleryView = false;
                }
            }
            catch (OperationCanceledException)
            {
                // Operation was canceled, do nothing
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading folder: {ex.Message}");
                IsGalleryView = false;
            }
        }
        // Simple method to load a preview based on file type
        private async Task LoadPreviewAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                PreviewContent = new TextBlock { Text = "File not found" };
                return;
            }

            try
            {
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                // Handle image files
                if (IsImageFile(extension))
                {
                    await LoadImagePreviewAsync(filePath);

                    // Show gallery view if it's an image
                    IsGalleryView = true;
                }
                else
                {
                    // For other files, just show basic info
                    var fileInfo = new FileInfo(filePath);
                    PreviewContent = new TextBlock
                    {
                        Text = $"File: {Path.GetFileName(filePath)}\nType: {extension}\nSize: {fileInfo.Length / 1024} KB"
                    };

                    // Hide gallery view for non-images
                    IsGalleryView = false;
                }
            }
            catch (Exception ex)
            {
                PreviewContent = new TextBlock { Text = $"Error: {ex.Message}" };
            }
        }

        // Load image preview
        private async Task LoadImagePreviewAsync(string filePath)
        {
            try
            {
                // Load image on background thread
                var bitmap = await Task.Run(() => new Bitmap(filePath));

                // Create simple image control
                var image = new Image
                {
                    Source = bitmap,
                    Stretch = Avalonia.Media.Stretch.Uniform,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                PreviewContent = image;
            }
            catch (Exception ex)
            {
                PreviewContent = new TextBlock { Text = $"Failed to load image: {ex.Message}" };
            }
        }

        // Load all images from directory for gallery view
        private async Task LoadImageGalleryAsync(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return;

            try
            {
                // Clear existing gallery
                await Task.Run(() =>
                {
                    // Find all image files in the directory
                    var imageFiles = Directory.GetFiles(directory)
                        .Where(file => IsImageFile(Path.GetExtension(file).ToLowerInvariant()))
                        .OrderBy(file => Path.GetFileName(file))
                        .ToList();

                    // Create new collection to avoid UI updates for each item
                    var newGallery = new ObservableCollection<ImageItem>();

                    // Create thumbnails for each image
                    foreach (var imagePath in imageFiles)
                    {
                        try
                        {
                            newGallery.Add(new ImageItem
                            {
                                FilePath = imagePath,
                                FileName = Path.GetFileName(imagePath)
                            });
                        }
                        catch
                        {
                            // Skip failed images
                        }
                    }

                    // Update gallery
                    ImageGallery = newGallery;
                });
            }
            catch (Exception ex)
            {
                // Handle errors silently
                System.Diagnostics.Debug.WriteLine($"Error loading gallery: {ex.Message}");
            }
        }

        private bool IsImageFile(string extension)
        {
            return extension is ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif";
        }
    }

    // Image item for gallery
    public class ImageItem : ObservableObject
    {
        private string _filePath = string.Empty;
        private string _fileName = string.Empty;
        private Bitmap? _thumbnail;
        private bool _isLoading = false;
        private static readonly ThumbnailService _sharedThumbnailService = new(100);

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (SetProperty(ref _filePath, value) && !string.IsNullOrEmpty(value))
                {
                    // Load thumbnail
                    _ = LoadThumbnailAsync();
                }
            }
        }

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public Bitmap? Thumbnail
        {
            get => _thumbnail;
            private set => SetProperty(ref _thumbnail, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        private async Task LoadThumbnailAsync()
        {
            if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath) || IsLoading)
                return;

            IsLoading = true;
            try
            {
                // Use the shared thumbnail service
                Thumbnail = await _sharedThumbnailService.GetThumbnailAsync(FilePath);
            }
            catch
            {
                // Ignore thumbnail errors
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}