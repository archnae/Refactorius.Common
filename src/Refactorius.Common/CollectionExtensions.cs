namespace Refactorius;

/// <summary>Handy extension methods for <see cref="ICollection{T}"/>.</summary>
[PublicAPI]
public static class CollectionExtensions
{
    /// <summary>Fluently adds a new item to a collection.</summary>
    /// <typeparam name="TElement"><c>Type</c> of the collection items.</typeparam>
    /// <param name="collection">A collection.</param>
    /// <param name="item">An item to add.</param>
    /// <returns>The <paramref name="collection"/> itself (for call chaining).</returns>
    /// <exception cref="ArgumentNullException">if <paramref name="collection"/> is <see langword="null"/>.</exception>
    public static ICollection<TElement> AddOne<TElement>(
        this ICollection<TElement> collection,
        TElement item)
    {
        collection.MustNotBeNull(nameof(collection));

        collection.Add(item);
        return collection;
    }

    /// <summary>Fluently adds new items to a collection.</summary>
    /// <typeparam name="TElement"><c>Type</c> of the collection items.</typeparam>
    /// <param name="collection">A collection.</param>
    /// <param name="items">The items to add.</param>
    /// <returns>The <paramref name="collection"/> itself (for call chaining).</returns>
    /// <exception cref="ArgumentNullException">if <paramref name="collection"/> oe <paramref name="items"/> is
    /// <see langword="null"/>.</exception>
    public static ICollection<TElement> AddRange<TElement>(
        this ICollection<TElement> collection,
        [InstantHandle] IEnumerable<TElement> items)
    {
        collection.MustNotBeNull(nameof(collection));
        items.MustNotBeNull(nameof(items));

        foreach (var item in items)
            collection.Add(item);

        return collection;
    }
}