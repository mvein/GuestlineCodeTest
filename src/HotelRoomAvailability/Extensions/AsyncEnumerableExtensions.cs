using System.Runtime.CompilerServices;

namespace HotelRoomAvailability.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async Task<int> CountAsync<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);

        var count = 0;
        await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            count++;
        }

        return count;
    }

    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var item in source)
        {
            yield return item;
            await Task.Yield(); // Ensure async context
        }
    }
}
