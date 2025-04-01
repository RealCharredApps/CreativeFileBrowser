//************************************************************************//
//THUMBNAIL HELPER METHOD - LOADIMAGESFOLDER
//************************************************************************//
public static class FileThumbnailService
{
    public static Image? GenerateThumbnail(string path, int width, int height)
    {
        try
        {
            // Image files
            using var original = Image.FromFile(path);
            return new Bitmap(original, new Size(width, height));
        }
        catch (OutOfMemoryException)
        {
            GC.Collect();
            return null;
        }
        catch
        {
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