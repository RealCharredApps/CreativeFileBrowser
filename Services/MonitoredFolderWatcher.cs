// Services/MonitoredFolderWatcher.cs
using System.IO;
using System.Collections.Generic;


public class MonitoredFolderWatcher
{
    private readonly List<FileSystemWatcher> _watchers = new();
    private System.Timers.Timer? _debounceTimer;
    private readonly int _debounceMs = 500;
    public event Action? OnAnyFolderChanged;

    public void SetFolders(IEnumerable<string> paths)
    {
        StopAll();

        foreach (var path in paths)
        {
            if (!Directory.Exists(path)) continue;

            var watcher = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            watcher.Changed += (_, __) => DebounceChanged();
            watcher.Created += (_, __) => OnAnyFolderChanged?.Invoke();
            watcher.Deleted += (_, __) => OnAnyFolderChanged?.Invoke();
            watcher.Renamed += (_, __) => OnAnyFolderChanged?.Invoke();

            watcher.EnableRaisingEvents = true;
            _watchers.Add(watcher);
        }
    }

    private void DebounceChanged()
    {
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();

        _debounceTimer = new System.Timers.Timer(_debounceMs)
        {
            AutoReset = false,
            Enabled = true
        };
        _debounceTimer.Elapsed += (_, __) => OnAnyFolderChanged?.Invoke();
    }


    public void StopAll()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
        _watchers.Clear();
    }
}
