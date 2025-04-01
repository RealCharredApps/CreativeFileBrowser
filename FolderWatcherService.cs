using System.IO;

namespace CreativeFileBrowser;

public class FolderWatcherService
{
    private readonly string folder;
    private readonly Action onChange;
    private FileSystemWatcher? watcher;

    public FolderWatcherService(string folder, Action onChange)
    {
        this.folder = folder;
        this.onChange = onChange;
    }

    public void Start()
    {
        watcher = new FileSystemWatcher(folder)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        watcher.Created += (_, _) => onChange();
        watcher.Deleted += (_, _) => onChange();
        watcher.Renamed += (_, _) => onChange();
        watcher.Changed += (_, _) => onChange();
    }

    public void Stop() => watcher?.Dispose();
}
