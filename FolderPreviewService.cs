using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CreativeFileBrowser.Services;

public static class FolderPreviewService
{
    private static readonly string[] allowedExtensions = new[]
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp",
        ".mp4", ".mov", ".psd", ".tiff", ".bmp", ".raw", ".heic"
    };

    private static readonly HashSet<string> loadedPaths = new(StringComparer.OrdinalIgnoreCase);
    private static CancellationTokenSource? cts;

    public static void LoadThumbnails(string folderPath, FlowLayoutPanel targetPanel)
    {
        cts?.Cancel();
        cts = new CancellationTokenSource();
        loadedPaths.Clear();

        targetPanel.Controls.Clear();

        Task.Run(() => Enumerate(folderPath, targetPanel, cts.Token));
    }

    private static void Enumerate(string folder, FlowLayoutPanel target, CancellationToken token)
    {
        try
        {
            foreach (var file in Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                     .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                     .OrderByDescending(File.GetLastWriteTime))
            {
                if (token.IsCancellationRequested || !File.Exists(file) || !loadedPaths.Add(file)) continue;

                try
                {
                    var thumb = FileThumbnailService.GenerateProportionalThumbnail(file, 160);
                    if (thumb == null) continue; // skip if thumbnail generation failed
                    /*{
                        thumb = new Bitmap(160, 160); // dummy gray fallback
                        using (Graphics g = Graphics.FromImage(thumb))
                            g.Clear(Color.LightGray);
                    }*/


                    target.Invoke(() =>
                    {
                        var pic = new PictureBox
                        {
                            Image = thumb,
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Width = 160,
                            Height = 160,
                            Margin = new Padding(4),
                            Cursor = Cursors.Hand,
                            Tag = file
                        };

                        pic.Click += (_, _) => Process.Start("explorer.exe", $"/select,\"{file}\"");
                        target.Controls.Add(pic);
                    });
                }
                catch (OutOfMemoryException) { GC.Collect(); continue; }
                catch { continue; }
            }

            foreach (var sub in Directory.EnumerateDirectories(folder))
            {
                if (token.IsCancellationRequested) return;
                Enumerate(sub, target, token); // recursive
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"❌ Skipped {folder}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Failed {folder}: {ex.Message}");
        }
    }

}
