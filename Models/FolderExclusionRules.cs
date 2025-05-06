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
            "//Volumes//com.apple.TimeMachine.localsnapshots",
            "//System//Volumes/Data/home",
            "//Library//TimeMachine",
            "//System//Volumes//Data",
            "//System//Volumes//Data//home",
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
            "/lost+found/",
            
            // Add more as needed
        };

        // Root paths that should be completely excluded from browsing
        private static readonly HashSet<string> ExcludedRootPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/System/Volumes/Data",
            "/dev",
            "/proc",
            "/sys",
            "C:\\Windows\\SoftwareDistribution",
            ".timemachine",
            "com.apple.TimeMachine.localsnapshots",
            "//Volumes//com.apple.TimeMachine.localsnapshots",
            "//System//Volumes/Data/home",
            "//Library//TimeMachine",
            "//System//Volumes//Data",
            "//System//Volumes//Data//home",
            "C:\\System Volume Information",
            "/Volumes/com.apple.TimeMachine.localsnapshots",
            "/Volumes/Recovery HD",
            "C:\\Program Files\\WindowsApps",
            "C:\\ProgramData",
            "C:\\$Recycle.Bin",
            "C:\\Users\\Public\\Documents\\Autodesk Shared",
            "C:\\Users\\Public\\Documents\\Intel",
            "C:\\Users\\Public\\Documents\\NVIDIA Corporation",
            "C:\\Users\\Public\\Documents\\NVIDIA GPU Computing SDK",
            "C:\\Users\\Public\\Documents\\NVIDIA Corporation",

        };

        // Check if a folder should be excluded based on name or path
        public static bool ShouldExcludeFolder(string name, string path)
        {
            // Always check root paths first (most restrictive)
            foreach (var rootPath in ExcludedRootPaths)
            {
                if (path.Equals(rootPath, StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith(rootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Check if name is in excluded list
            if (ExcludedFolderNames.Contains(name))
                return true;

            // Check if path contains any excluded patterns
            foreach (string pattern in ExcludedPathPatterns)
            {
                if (path.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
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