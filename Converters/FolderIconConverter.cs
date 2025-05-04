using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CreativeFileBrowser.Models;
using static CreativeFileBrowser.Models.FileSystemItem;


namespace CreativeFileBrowser.Converters
{
    public class FolderIconConverter : IValueConverter
    {
        public static readonly FolderIconConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isDirectory)
            {
                // Return different brushes based on directory status
                return isDirectory ? Brushes.Orange : Brushes.LightBlue;
            }
            return Brushes.Gray; // Default
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}