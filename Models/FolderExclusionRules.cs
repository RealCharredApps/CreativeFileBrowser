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
        private static readonly HashSet<string> ExcludedFolderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // System folders
            "security",
            "system",
            
            // TimeMachine related
            "timemachine",
            ".timemachine",
            "com.apple.TimeMachine.localsnapshots",
            
            // Mac system folders
            ".Spotlight-V100",
            ".fseventsd",
            ".Trashes",
            
            // Windows system files/folders
            "$recycle.bin",
            "system volume information",
            "pagefile.sys",
            "hiberfil.sys",
            "swapfile.sys",
            
            // Common hidden/temp files
            "thumbs.db",
            ".DS_Store",
            "__MACOSX",
            "lost+found"
        };

        private static string NormalizePath(string path)
        {
            return path.Replace('\\', '/').TrimEnd('/');
        }

        private static bool IsMacOSSystemPath(string path)
        {
            
            string normalizedPath = NormalizePath(path);
            if (normalizedPath.StartsWith("/system/") ||
                normalizedPath.StartsWith("/private/") ||
                normalizedPath.StartsWith("/dev/") ||
                normalizedPath.StartsWith("/bin/") ||
                normalizedPath.StartsWith("/sbin/") ||
                normalizedPath.Equals("/system") ||
                normalizedPath.Equals("/private") ||
                normalizedPath.Contains("/system/volumes/data") ||
                normalizedPath.Contains(".timemachine") ||
                normalizedPath.Contains(".timemachine"))
            {
                System.Diagnostics.Debug.WriteLine($"[PathExclusion] Excluding macOS system path: {normalizedPath}");
                return true;
            }
            return false;
        }
        private static bool IsWindowsSystemPath(string path)
        {
            string normalizedPath = NormalizePath(path);
            if (normalizedPath.Contains("/windows/") ||
                            normalizedPath.Contains("/system32/") ||
                            normalizedPath.Contains("/system volume information") ||
                            normalizedPath.Contains("/$recycle.bin"))
            {
                System.Diagnostics.Debug.WriteLine($"[PathExclusion] Excluding Windows system path: {normalizedPath}");
                return true;
            }
            return false;
        }

        private static readonly HashSet<string> ExcludedExactPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            NormalizePath("/System/Volumes/Data/home"),
            NormalizePath("/System/Volumes/Data"),
            NormalizePath("/System/Volumes/Data/private"),
            NormalizePath("/System/Volumes/Update"),
            NormalizePath("/System/Volumes/Preboot")
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
            // First normalize the path for all checks
            string normalizedPath = NormalizePath(path);

            try
            {
                // 1. Check exact paths first (most restrictive)
                if (ExcludedExactPaths.Contains(normalizedPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Excluding exact path match: {path}");
                    return true;
                }

                // 2. Check for children of excluded paths
                foreach (var exactPath in ExcludedExactPaths)
                {
                    if (normalizedPath.StartsWith(exactPath + "/", StringComparison.OrdinalIgnoreCase))
                    {
                        System.Diagnostics.Debug.WriteLine($"Excluding child of excluded path: {path}");
                        return true;
                    }
                }

                // 3. Check if name is in excluded list
                if (ExcludedFolderNames.Contains(name))
                {
                    System.Diagnostics.Debug.WriteLine($"Excluding folder by name: {name}");
                    return true;
                }

                // 4. OS-specific checks
                if (OperatingSystem.IsMacOS() && IsMacOSSystemPath(normalizedPath))
                {
                    return true;
                }
                else if (OperatingSystem.IsWindows() && IsWindowsSystemPath(normalizedPath))
                {
                    return true;
                }

                // 5. Common checks for all platforms
                string fileName = Path.GetFileName(normalizedPath).ToLowerInvariant();
                if (IsCommonExcludedFile(fileName))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking path exclusion for {path}: {ex.Message}");
                return false;
            }
        }

        private static bool IsCommonExcludedFile(string fileName)
        {
            if (fileName.StartsWith(".") || // Hidden files on Unix
                fileName == "$recycle.bin" ||
                fileName == "system volume information" ||
                fileName == "thumbs.db" ||
                fileName == ".ds_store" ||
                fileName.Contains("timemachine"))
            {
                System.Diagnostics.Debug.WriteLine($"[PathExclusion] Excluding system file: {fileName}");
                return true;
            }
            return false;
        }

        // Method to add custom exclusions at runtime
        public static void AddExactPathExclusion(string path)
        {
            ExcludedExactPaths.Add(NormalizePath(path));
            System.Diagnostics.Debug.WriteLine($"Added exact path exclusion: {path}");
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