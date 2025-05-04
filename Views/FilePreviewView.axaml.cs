using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CreativeFileBrowser.ViewModels;

namespace CreativeFileBrowser.Views
{
    public partial class FilePreviewView : UserControl
    {
        public FilePreviewView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        // Handle thumbnail click event
        private void OnGalleryItemPressed(object sender, PointerPressedEventArgs e)
        {
            if (DataContext is FilePreviewViewModel viewModel && 
                sender is Border border && 
                border.DataContext is ImageItem imageItem)
            {
                // Update the selected image
                viewModel.FilePath = imageItem.FilePath;
            }
        }
    }
}