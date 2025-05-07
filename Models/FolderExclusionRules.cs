using System;
using System.Collections.Generic;
using System.IO;

namespace CreativeFileBrowser.Models
{
    /// <summary>
    /// Provides rules for excluding folders from file system browser.
    /// </summary>
    public static class FolderExclusionRules
    {
        // List of folder names to exclude (case-insensitive)
        private static readonly HashSet<string> ExcludedFolderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "security",
            "system",
            "timemachine",
            ".timemachine",
            ".timemachine",
            "C:\\System Volume Information",
            "com.apple.TimeMachine.localsnapshots",
            "/Volumes/com.apple.TimeMachine.localsnapshots",
            "/System/Volumes/Data/home",
            "/Library/TimeMachine",
            "/System/Volumes/Data",
            "/System/Volumes/Data/",
            "/System/Volumes/Data/home",
            ".Spotlight-V100",
            ".fseventsd",
            ".Trashes",
            "$recycle.bin",
            "system volume information",
            "pagefile.sys",
            "hiberfil.sys",
            "swapfile.sys",
            "thumbs.db",
            ".DS_Store",
            "__MACOSX",
            "lost+found",
            // Add more as needed
        };

        // Full paths that should be completely excluded - exact match required
        private static readonly HashSet<string> ExcludedExactPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/System/Volumes/Data/home",
            "/System/Volumes/Data",
            "/System/Volumes/Data/private",
            "/System/Volumes/Update",
            "/System/Volumes/Preboot"
        };

        // List of path patterns to exclude (using simple contains for now)
        private static readonly List<string> ExcludedPathPatterns = new List<string>
        {
            // macOS specific
            "/private/",
            "/System/",
            "/System/Volumes/Data/home",
            "/Library/TimeMachine",
            "/.DocumentRevisions-",
            "/.hotfiles.btree",
            "/Library/Caches/",
            ".timemachine",
            ".TimeMachine",
            ".TimeMachine.localsnapshots",
            "timemachine",
            "TimeMachine",
            ".Spotlight-V100",
            ".fseventsd",
            ".Trashes",
            
            // Windows specific
            "\\Windows\\",
            "\\Program Files\\WindowsApps\\",
            "\\AppData\\",
            "\\$Recycle.Bin\\",
            "\\System Volume Information\\",
            "$recycle.bin",
            "system volume information",
            "pagefile.sys",
            "hiberfil.sys",
            "swapfile.sys",
            
            // Linux specific
            "/proc/",
            "/sys/",
            "/dev/",
            "/run/",
            "/var/cache/",
            "/tmp/",
            "thumbs.db",
            ".DS_Store",
            "__MACOSX",
            "/lost+found/",
            
            // Add more as needed
        };

        // Root paths that should be completely excluded from browsing
        private static readonly HashSet<string> ExcludedRootPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // macOS specific
            "/.timemachine",
            "/.TimeMachine",
            "/Volumes/.TimeMachine",
            "/Volumes/TimeMachine",
            "/private/",
            "/System/",
            "/System/Volumes/Data/home",
            "/Library/TimeMachine",
            "/.DocumentRevisions-",
            "/.hotfiles.btree",
            "/Library/Caches/",
            
            // Windows specific
            "\\Windows\\",
            "\\Program Files\\WindowsApps\\",
            "\\AppData\\",
            "\\$Recycle.Bin\\",
            "\\System Volume Information\\",
            
            // Linux specific
            "/proc/",
            "/sys/",
            "/dev/",
            "/run/",
            "/var/cache/",
            "/tmp/",
            "/lost+found/"

        };

        // Check if a folder should be excluded based on name or path
        public static bool ShouldExcludeFolder(string name, string path)
        {
            // Check exact path exclusions first - most restrictive
            if (ExcludedExactPaths.Contains(path))
            {
                System.Diagnostics.Debug.WriteLine($"Excluding exact path match: {path}");
                return true;
            }

            // Check for specific parent paths
            foreach (var exactPath in ExcludedExactPaths)
            {
                if (path.StartsWith(exactPath + "/", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith(exactPath + "\\", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Excluding child of excluded path: {path}");
                    return true;
                }
            }

            // Check if name is in excluded list
            if (ExcludedFolderNames.Contains(name))
            {
                System.Diagnostics.Debug.WriteLine($"Excluding folder by name: {name}");
                return true;
            }

            // Check for TimeMachine folders specifically
            if (name.Contains("timemachine", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("TimeMachine", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"Excluding TimeMachine folder: {path}");
                return true;
            }

            // Check if path contains any excluded patterns
            foreach (string pattern in ExcludedPathPatterns)
            {
                if (path.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine($"Excluding folder by pattern match: {path} (pattern: {pattern})");
                    return true;
                }
            }

            return false;
        }

        // Method to add custom exclusions at runtime
        public static void AddExactPathExclusion(string path)
        {
            ExcludedExactPaths.Add(path);
        }
        
        // Method to add custom exclusions at runtime
        public static void AddCustomExclusion(string nameOrPattern, bool isPattern = false, bool isRootPath = false)
        {
            if (isRootPath)
                ExcludedRootPaths.Add(nameOrPattern);
            else if (isPattern)
                ExcludedPathPatterns.Add(nameOrPattern);
            else
                ExcludedFolderNames.Add(nameOrPattern);
        }
    }
}