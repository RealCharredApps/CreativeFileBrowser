using System.Diagnostics;
using System.Text.Json;

public class Workspace
{
    public string Name { get; set; } = "";
    public List<string> MonitoredFolders { get; set; } = new();
    public string? SelectedSystemFolder { get; set; }
}

public static class WorkspaceManager
{
    private const string FilePath = "workspaces.json";

    public static List<Workspace> SavedWorkspaces { get; private set; } = new();

    public static void LoadWorkspacesFromFile()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                File.WriteAllText(FilePath, "[]");
                SavedWorkspaces = new();
                return;
            }

            string json = File.ReadAllText(FilePath);
            SavedWorkspaces = JsonSerializer.Deserialize<List<Workspace>>(json) ?? new();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Failed to load workspaces: {ex.Message}");
            SavedWorkspaces = new();
        }
    }

    public static void SaveWorkspacesToFile()
    {
        try
        {
            string json = JsonSerializer.Serialize(SavedWorkspaces, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Save failed: {ex.Message}");
        }
    }

    public static void RemoveWorkspaceByName(string name)
    {
        SavedWorkspaces.RemoveAll(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        SaveWorkspacesToFile();
    }

    public static Workspace? GetWorkspaceByName(string name) =>
        SavedWorkspaces.FirstOrDefault(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public static List<string> GetAllWorkspaceNames() =>
        SavedWorkspaces.Select(w => w.Name).OrderBy(n => n).ToList();
}
