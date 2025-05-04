using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CreativeFileBrowser.Models;
using static CreativeFileBrowser.Models.FileSystemItem;


namespace CreativeFileBrowser.Converters
{
    public class DriveIconConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // First parameter is DriveType, second is IsDirectory
            if (values.Count >= 2 && values[0] is DriveTypeInfo driveType && values[1] is bool isDirectory)
            {
                // For root items with drive type information
                if (driveType != DriveTypeInfo.Unknown)
                {
                    return driveType switch
                    {
                        DriveTypeInfo.Fixed => new SolidColorBrush(Colors.LightGreen),
                        DriveTypeInfo.Removable => new SolidColorBrush(Colors.Orange),
                        DriveTypeInfo.Network => new SolidColorBrush(Colors.LightBlue),
                        DriveTypeInfo.SharePoint => new SolidColorBrush(Colors.Purple),
                        _ => new SolidColorBrush(Colors.Gray)
                    };
                }

                // For regular folders and files
                return isDirectory
                    ? new SolidColorBrush(Colors.Gold)
                    : new SolidColorBrush(Colors.White);
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}