using System.Collections.Generic;

namespace Serilog.Sinks.Scalyr;

/// <summary>
/// Extension methods for <see cref="KeyValuePair{TKey,TValue}"/>.
/// </summary>
public static class KeyValuePairExtensions
{
    /// <summary>
    /// Deconstructs the key value pair into a key and value.
    /// </summary>
    /// <param name="kvp">The key value pair.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <typeparam name="TK">The type of the key.</typeparam>
    /// <typeparam name="TV">The type of the value.</typeparam>
    public static void Deconstruct<TK, TV>(this KeyValuePair<TK, TV> kvp, out TK key, out TV value)
    {
        key = kvp.Key;
        value = kvp.Value;
    }
}