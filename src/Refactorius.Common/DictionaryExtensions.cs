using System.Runtime.CompilerServices;

namespace Refactorius;

/// <summary>The collection of useful dictionary-related extension methods.</summary>
[PublicAPI]
public static class DictionaryExtensions
{
    /// <summary>Adds a new entry to a dictionary, returning the dictionary for call chaining.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TElement">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="key">A key.</param>
    /// <param name="value">A value.</param>
    /// <exception cref="ArgumentException">An element with the same key already exists in the dictionary.</exception>
    /// <returns>The <paramref name="dict"/> itself (for call chaining).</returns>
    public static IDictionary<TKey, TElement> AddOne<TKey, TElement>(
        this IDictionary<TKey, TElement> dict,
        [NotNull] TKey key,
        TElement value)
    {
        dict.MustNotBeNull(nameof(dict));

        dict.Add(key, value);
        return dict;
    }

    /// <summary>Adds a sequence of items to a dictionary.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TElement">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="other">A sequence of name-value pairs.</param>
    /// <exception cref="ArgumentException">An element with the same key already exists in the dictionary.</exception>
    /// <returns>The <paramref name="dict"/> itself (for call chaining).</returns>
    public static IDictionary<TKey, TElement> AddRange<TKey, TElement>(
        this IDictionary<TKey, TElement> dict,
        [InstantHandle] IEnumerable<KeyValuePair<TKey, TElement>> other)
    {
        dict.MustNotBeNull(nameof(dict));
        other.MustNotBeNull(nameof(other));

        foreach (var kvp in other)
            dict.Add(kvp.Key, kvp.Value);

        return dict;
    }

    /// <summary>Adds a sequence of items to a dictionary, prefixing the keys.</summary>
    /// <typeparam name="TElement">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="other">A sequence of name-value pairs.</param>
    /// <param name="keyPrefix">A prefix to add to the keys.</param>
    /// <exception cref="ArgumentException">An element with the same key already exists in the dictionary.</exception>
    /// <returns>The <paramref name="dict"/> itself (for call chaining).</returns>
    public static IDictionary<string, TElement> AddRange<TElement>(
        this IDictionary<string, TElement> dict,
        [InstantHandle] IEnumerable<KeyValuePair<string, TElement>> other,
        string? keyPrefix)
    {
        return string.IsNullOrEmpty(keyPrefix)
            ? dict.AddRange(other)
            : dict.AddRange(
                other.Select(kvp => new KeyValuePair<string, TElement>(keyPrefix + kvp.Key, kvp.Value)));
    }

    /// <summary>Returns a dictionary value by key or a default.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TElement">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="key">A key.</param>
    /// <returns>The value for <paramref name="key"/> or the default value of type <typeparamref name="TElement"/> if not
    /// found.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TElement? Get<TKey, TElement>(this IDictionary<TKey, TElement> dict, [NotNull] TKey key)
    {
        dict.MustNotBeNull(nameof(dict));

        return dict.TryGetValue(key, out var value) ? value : default;
    }

    /// <summary>Returns a dictionary value of a specified type by key or a default value of this type.</summary>
    /// <typeparam name="TResult">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="key">A key.</param>
    /// <returns>The value for <paramref name="key"/> or the default value of type <typeparamref name="TResult"/> if not found.</returns>
    [Pure]
    public static TResult GetAs<TResult>(this IPropertyBag dict, string key)
    {
        return GetAs(dict, key, default(TResult)!);
    }

    /// <summary>Returns a dictionary value of a specified type by key or a default value.</summary>
    /// <typeparam name="TResult">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="key">A key.</param>
    /// <param name="defaultValue">A default value.</param>
    /// <returns>The value for <paramref name="key"/> or the default value of type <typeparamref name="TResult"/> if not found.</returns>
    [Pure]
    public static TResult GetAs<TResult>(this IPropertyBag dict, string key,
        TResult defaultValue)
    {
        dict.MustNotBeNull(nameof(dict));

        if (!dict.ContainsKey(key))
            return defaultValue;

        var value = dict[key];
        if (value == null)
            return defaultValue;

        if (value is TResult result)
            return result;

        // use custom to string convertor (mostly to handle UTC date format)
        value = typeof(TResult) == typeof(string)
            ? ConvertUtils.ToString(value)
            : ConvertUtils.ChangeType(value, typeof(TResult));
        return (TResult)value!;
    }

#if READONLY

        /// <summary>Returns a dictionary value by key or a default.</summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TElement">The value type.</typeparam>
        /// <param name="dict">A dictionary.</param>
        /// <param name="key">A key.</param>
        /// <returns>The value for <paramref name="key"/> or the default value of type <typeparamref name="TElement"/> if not
        /// found.</returns>
        [CanBeNull]
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement Get<TKey, TElement>([NotNull] this IReadOnlyDictionary<TKey, TElement> dict, [NotNull] TKey key)
        {
            dict.MustNotBeNull(nameof(dict));

            return dict.ContainsKey(key) ? dict[key] : default(TElement);
        }

        /// <summary>
        /// Returns a dictionary value of a specified type by key or a default value of this type.
        /// </summary>
        /// <typeparam name="TResult">The value type.</typeparam>
        /// <param name="dict">A dictionary.</param>
        /// <param name="key">A key.</param>
        /// <returns>The value for <paramref name="key"/> or the default value of type <typeparamref name="TResult"/> if not found.</returns>
        [Pure]
        public static TResult GetAs<TResult>([NotNull] this IReadOnlyDictionary<string, object> dict, [NotNull] string key)
        {
            return GetAs(dict, key, default(TResult));
        }

        /// <summary>
        /// Returns a dictionary value of a specified type by key or a default value.
        /// </summary>
        /// <typeparam name="TResult">The value type.</typeparam>
        /// <param name="dict">A dictionary.</param>
        /// <param name="key">A key.</param>
        /// <param name="defaultValue">A default value.</param>
        /// <returns>The value for <paramref name="key"/> or the default value of type <typeparamref name="TResult"/> if not found.</returns>
        [Pure]
        public static TResult GetAs<TResult>([NotNull] this IReadOnlyDictionary<string, object> dict, [NotNull] string key, TResult defaultValue)
        {
            dict.MustNotBeNull(nameof(dict));

            if (!dict.ContainsKey(key))
                return defaultValue;

            var value = dict[key];
            if (value == null)
                return defaultValue;

            if (value is TResult)
                return (TResult)value;

            // use custom tostring convertor (mostly to handle UTC date format)
            value = typeof(TResult) == typeof(string)
                ? ConvertUtils.ToString(value)
                : ConvertUtils.ChangeType(value, typeof(TResult));
            return (TResult)value;
        }
#endif

    /// <summary>Returns a sequence of dictionary values by keys.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TElement">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="keys">A sequence of keys.</param>
    /// <param name="force">A value indicating whether missing key is an error.</param>
    /// <returns>The sequence of values for <paramref name="keys"/>.</returns>
    [LinqTunnel]
    public static IEnumerable<TElement?> GetMany<TKey, TElement>(
        this IDictionary<TKey, TElement> dict,
        [ItemNotNull] IEnumerable<TKey> keys,
        bool force = false)
    {
        dict.MustNotBeNull(nameof(dict));
        keys.MustNotBeNull(nameof(keys));

        foreach (var key in keys)
            if (dict.ContainsKey(key))
                yield return dict[key];
            else if (force)
                throw new KeyNotFoundException("Key not found.");
    }

    /// <summary>Returns a sequence of dictionary entries by keys.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TElement">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="keys">A sequence of keys.</param>
    /// <param name="force">A value indicating whether missing key is an error.</param>
    /// <returns>The sequence containing only entries for <paramref name="keys"/>.</returns>
    [LinqTunnel]
    public static IEnumerable<KeyValuePair<TKey, TElement>> Select<TKey, TElement>(
        this IDictionary<TKey, TElement> dict,
        [ItemNotNull] IEnumerable<TKey> keys,
        bool force = false)
    {
        dict.MustNotBeNull(nameof(dict));
        keys.MustNotBeNull(nameof(keys));

        foreach (var key in keys)
            if (dict.ContainsKey(key))
                yield return new KeyValuePair<TKey, TElement>(key, dict[key]);
            else if (force)
                throw new KeyNotFoundException("Key not found.");
    }

    /// <summary>Adds a new entry to a dictionary or replaces an existing one, returning the dictionary for call chaining.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TElement">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="key">A key.</param>
    /// <param name="value">A value.</param>
    /// <returns>The <paramref name="dict"/> itself (for call chaining).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IDictionary<TKey, TElement> Set<TKey, TElement>(
        this IDictionary<TKey, TElement> dict,
        [NotNull] TKey key,
        TElement value)
    {
        dict.MustNotBeNull(nameof(dict));

        if (dict.ContainsKey(key))
            dict[key] = value;
        else
            dict.Add(key, value);
        return dict;
    }

    /// <summary>Adds new entries to a dictionary or replaces existing ones, returning the dictionary for call chaining.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TElement">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="other">A sequence of name-value pairs.</param>
    /// <returns>The <paramref name="dict"/> itself (for call chaining).</returns>
    public static IDictionary<TKey, TElement> SetRange<TKey, TElement>(
        this IDictionary<TKey, TElement> dict,
        [InstantHandle] IEnumerable<KeyValuePair<TKey, TElement>> other)
    {
        dict.MustNotBeNull(nameof(dict));
        other.MustNotBeNull(nameof(other));

        foreach (var kvp in other)
            dict.Set(kvp.Key, kvp.Value);

        return dict;
    }

    /// <summary>Adds new entries to a dictionary or replaces existing ones, returning the dictionary for call chaining.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TElement">The value type.</typeparam>
    /// <typeparam name="TSource">The type of the sequence items.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="other">A sequence of items.</param>
    /// <param name="keySelector">A key mapping function.</param>
    /// <param name="elementSelector">A value mapping function.</param>
    /// <returns>The <paramref name="dict"/> itself (for call chaining).</returns>
    public static IDictionary<TKey, TElement> SetRange<TKey, TElement, TSource>(
        this IDictionary<TKey, TElement> dict,
        [InstantHandle] IEnumerable<TSource> other,
        [InstantHandle] Func<TSource, TKey> keySelector,
        [InstantHandle] Func<TSource, TElement> elementSelector)
    {
        dict.MustNotBeNull(nameof(dict));
        other.MustNotBeNull(nameof(other));
        keySelector.MustNotBeNull(nameof(keySelector));
        elementSelector.MustNotBeNull(nameof(elementSelector));

        foreach (var item in other)
            dict.Set(keySelector(item), elementSelector(item));

        return dict;
    }

    /// <summary>Adds new entries to a dictionary or replaces existing ones, returning the dictionary for call chaining.</summary>
    /// <typeparam name="TElement">The value type.</typeparam>
    /// <param name="dict">A dictionary.</param>
    /// <param name="other">A sequence of name-value pairs.</param>
    /// <param name="keyPrefix">A prefix to add to the keys.</param>
    /// <exception cref="ArgumentException">An element with the same key already exists in the dictionary.</exception>
    /// <returns>The <paramref name="dict"/> itself (for call chaining).</returns>
    public static IDictionary<string, TElement> SetRange<TElement>(
        this IDictionary<string, TElement> dict,
        [InstantHandle] IEnumerable<KeyValuePair<string, TElement>> other,
        string? keyPrefix)
    {
        return string.IsNullOrEmpty(keyPrefix)
            ? dict.SetRange(other)
            : dict.SetRange(
                other.Select(kvp => new KeyValuePair<string, TElement>(keyPrefix + kvp.Key, kvp.Value)));
    }
}