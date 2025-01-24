using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace HotelRoomAvailability.Repositories.Abstractions;

public abstract class FileSourceRepository<T>(IMemoryCache memoryCache)
{
    private List<T>? _data;
    protected IEnumerable<T> Data => _data ??= LoadData();
    protected abstract string CacheKey { get; }

    protected readonly IMemoryCache MemoryCache = memoryCache;

    protected List<T> LoadData()
    {
        if (!MemoryCache.TryGetValue(CacheKey, out string? filePath))
        {
            throw new InvalidOperationException($"The {{filePath}} was not specified for '{CacheKey}'.");
        }

        try
        {
            return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(filePath!)) ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file {filePath}: {ex.Message}"); // TODO: change to ILogger
            return [];
        }
    }
}