using System;
using Avalonia.Media.Fonts;

public sealed class BaskervilleFontCollection : EmbeddedFontCollection
{
    public BaskervilleFontCollection() : base(
        new Uri("fonts:Baskerville", UriKind.Absolute),
        new Uri("/Library/CodeProjects/CreativeFileBrowser_Scratch_Avalonia/CreativeFileBrowser/Assets/Fonts", UriKind.Absolute))
    {
    }
}