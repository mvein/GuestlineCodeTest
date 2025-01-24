using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace HotelRoomAvailability.Repositories.Abstractions;

public abstract class FileSourceRepository<T>(IMemoryCache memoryCache)
{
    protected abstract string CacheKey { get; }

    protected readonly IMemoryCache MemoryCache = memoryCache;

    protected async IAsyncEnumerable<T> LoadData([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!MemoryCache.TryGetValue(CacheKey, out string? filePath))
        {
            throw new InvalidOperationException($"The {{filePath}} was not specified for '{CacheKey}'.");
        }

        using var stream = new StreamReader(filePath!);
        using var jsonReader = new JsonTextReader(stream);

        var serializer = new JsonSerializer();
        while (await jsonReader.ReadAsync(cancellationToken))
        {
            if (jsonReader.TokenType == JsonToken.StartArray)
            {
                while (await jsonReader.ReadAsync(cancellationToken) && jsonReader.TokenType != JsonToken.EndArray)
                {
                    yield return serializer.Deserialize<T>(jsonReader)!;
                }
            }
        }
    }
}