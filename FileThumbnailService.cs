using System;
using System.Diagnostics;
//************************************************************************//
//THUMBNAIL HELPER METHOD - LOADIMAGESFOLDER
//************************************************************************//
public static class FileThumbnailService
{
    public static Image? GenerateThumbnail(string path, int maxSize)
    {
        try
        {
            // Image files
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var original = Image.FromStream(fs);

            int originalWidth = original.Width;
            int originalHeight = original.Height;

            float scale = Math.Min((float)maxSize / originalWidth, (float)maxSize / originalHeight);
            int newW = (int)(originalWidth * scale);
            int newH = (int)(originalHeight * scale);

            return new Bitmap(original, new Size(newW, newH));
        }
        catch (OutOfMemoryException)
        {
            GC.Collect();
            Debug.WriteLine($"⚠️ Out of memory on file: {path}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Failed to load image: {path} - {ex.Message}");
            return null;
        }
    }

    public static Image? GenerateProportionalThumbnail(string path, int maxSize)
    {
        try
        {
            using var original = Image.FromFile(path);
            int width = original.Width;
            int height = original.Height;

            float scale = Math.Min((float)maxSize / width, (float)maxSize / height);
            int newW = (int)(width * scale);
            int newH = (int)(height * scale);

            return new Bitmap(original, new Size(newW, newH));
        }
        catch (OutOfMemoryException) { GC.Collect(); return null; }
        catch { return null; }
    }
    
}