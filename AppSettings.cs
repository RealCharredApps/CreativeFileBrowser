namespace CreativeFileBrowser;

public class AppSettings
{
    public int Width { get; set; } = 1200;
    public int Height { get; set; } = 800;
    public int Top { get; set; } = 100;
    public int Left { get; set; } = 100;
    public int VerticalSplit { get; set; } = 360;
    public int HorizontalTopSplit { get; set; } = 600;
    public int HorizontalBottomSplit { get; set; } = 600;


    public List<string> MonitoredFolders { get; set; } = new();
}
