using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CreativeFileBrowser.ViewModels;

namespace CreativeFileBrowser.Views
{
    /// <summary>
    /// File tree view implementation
    /// </summary>
    public partial class FileTreeView : UserControl
    {
        public FileTreeView()
        {
            InitializeComponent();
            //DataContext set in parent control
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}