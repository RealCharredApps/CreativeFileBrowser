using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

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
            "$recycle.bin",
            "system volume information",
            "pagefile.sys",
            "hiberfil.sys",
            "swapfile.sys",
            "thumbs.db",
            ".DS_Store",
            "__MACOSX",
            // Add more as needed
        };

        // List of path patterns to exclude (using simple contains for now)
        private static readonly List<string> ExcludedPathPatterns = new List<string>
        {
            "/private/",
            "/System/",
            "/Library/TimeMachine",
            "\\Windows\\",
            "\\Program Files\\WindowsApps\\",
            "\\AppData\\",
            // Add more as needed
        };

        // Check if a folder should be excluded based on name or path
        public static bool ShouldExcludeFolder(string name, string path)
        {
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
        public static void AddCustomExclusion(string nameOrPattern, bool isPattern = false)
        {
            if (isPattern)
                ExcludedPathPatterns.Add(nameOrPattern);
            else
                ExcludedFolderNames.Add(nameOrPattern);
        }
    }
}