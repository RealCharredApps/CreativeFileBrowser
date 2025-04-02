using CreativeFileBrowser.Services;
using System.Diagnostics;

public static class MonitoredGalleryService
{
    public static void LoadThumbnailsParallel(
        IEnumerable<string> folderPaths,
        Control targetPanel)
    {
        if (targetPanel is not FlowLayoutPanel flow) return;

        flow.Controls.Clear();

        var folders = folderPaths.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var extensions = FolderPreviewService.allowedExtensions;

        Task.Run(() =>
        {
            Parallel.ForEach(folders, folder =>
            {
                try
                {
                    var files = Directory
                        .EnumerateFiles(folder, "*.*", SearchOption.AllDirectories)
                        .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

                    foreach (var file in files)
                    {
                        var thumb = FileThumbnailService.GenerateProportionalThumbnail(file, 160);
                        if (thumb == null) continue;

                        flow.Invoke(() =>
                        {
                            var panel = CreateThumbnailWithLabel(file, thumb);
                            flow.Controls.Add(panel);
                        });
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Debug.WriteLine($"🔒 Access denied: {folder} — {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Load error in {folder}: {ex.Message}");
                }
            });
        });
    }

    private static Panel CreateThumbnailWithLabel(string file, Image thumb)
    {
        var pic = new PictureBox
        {
            Image = thumb,
            Width = 160,
            Height = 160,
            SizeMode = PictureBoxSizeMode.Zoom,
            Dock = DockStyle.Top,
            Cursor = Cursors.Hand
        };

        var label = new Label
        {
            Text = Path.GetFileName(file),
            Dock = DockStyle.Bottom,
            Font = new Font("Segoe UI", 8),
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 20
        };

        var container = new Panel
        {
            Width = 160,
            Height = 180,
            Margin = new Padding(6)
        };

        container.Controls.Add(pic);
        container.Controls.Add(label);
        return container;
    }
}
