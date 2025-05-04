using Avalonia.Media.Imaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CreativeFileBrowser.Services
{
    public class ThumbnailService : IDisposable
    {
        // Use a concurrent dictionary for thread-safe caching
        private readonly ConcurrentDictionary<string, Bitmap> _thumbnailCache = new();
        
        // Set a reasonable cache size limit (e.g., 100 thumbnails)
        private const int MaxCacheSize = 100;
        
        // Use LRU (least recently used) tracking for cache management
        private readonly LinkedList<string> _lruList = new();
        private readonly SemaphoreSlim _cacheLock = new(1, 1);
        
        // Thumbnail size
        private readonly int _thumbWidth = 100;
        
        public ThumbnailService(int thumbnailWidth = 100)
        {
            _thumbWidth = thumbnailWidth;
        }
        
        public async Task<Bitmap?> GetThumbnailAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;
                
            // Check if thumbnail is in cache
            if (_thumbnailCache.TryGetValue(filePath, out var cachedThumbnail))
            {
                await UpdateLruAsync(filePath);
                return cachedThumbnail;
            }
            
            // Generate new thumbnail
            try
            {
                var newThumb = await Task.Run(() => GenerateThumbnail(filePath));
                if (newThumb != null)
                {
                    await AddToCacheAsync(filePath, newThumb);
                }
                return newThumb;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to generate thumbnail for {filePath}: {ex.Message}");
                return null;
            }
        }
        
        private Bitmap? GenerateThumbnail(string filePath)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return Bitmap.DecodeToWidth(fs, _thumbWidth);
            }
            catch
            {
                return null;
            }
        }
        
        private async Task AddToCacheAsync(string filePath, Bitmap thumbnail)
        {
            await _cacheLock.WaitAsync();
            try
            {
                // Add to cache
                _thumbnailCache[filePath] = thumbnail;
                _lruList.AddFirst(filePath);
                
                // Check if cache is full and remove least recently used items
                while (_thumbnailCache.Count > MaxCacheSize && _lruList.Last != null)
                {
                    var lruPath = _lruList.Last.Value;
                    _lruList.RemoveLast();
                    
                    if (_thumbnailCache.TryRemove(lruPath, out var oldThumb))
                    {
                        oldThumb.Dispose();
                    }
                }
            }
            finally
            {
                _cacheLock.Release();
            }
        }
        
        private async Task UpdateLruAsync(string filePath)
        {
            await _cacheLock.WaitAsync();
            try
            {
                var node = _lruList.Find(filePath);
                if (node != null)
                {
                    _lruList.Remove(node);
                    _lruList.AddFirst(filePath);
                }
            }
            finally
            {
                _cacheLock.Release();
            }
        }
        
        public void ClearCache()
        {
            foreach (var thumb in _thumbnailCache.Values)
            {
                thumb.Dispose();
            }
            
            _thumbnailCache.Clear();
            _lruList.Clear();
        }
        
        public void Dispose()
        {
            ClearCache();
            _cacheLock.Dispose();
        }
    }
}