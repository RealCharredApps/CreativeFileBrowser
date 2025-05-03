using System;
using Avalonia.Media.Fonts;

public sealed class RobotoFontCollection : EmbeddedFontCollection
{
    public RobotoFontCollection() : base(
        new Uri("fonts:Roboto", UriKind.Absolute),
        new Uri("/Library/CodeProjects/CreativeFileBrowser_Scratch_Avalonia/CreativeFileBrowser/Assets/Fonts", UriKind.Absolute))
    {
    }
}