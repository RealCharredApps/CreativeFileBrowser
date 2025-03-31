public class Workspace
{
    public string Name { get; set; } = "Untitled";
    public List<string> MonitoredFolders { get; set; } = new();
    public string? SelectedSystemFolder { get; set; } = null;
}

