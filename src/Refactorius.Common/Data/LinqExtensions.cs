using System.Collections;

namespace Refactorius.Data;

/// <summary>Extends Linq with <b>GroupAdjacentBy</b> method.</summary>
/// <author>Matt Hickford</author>
/// <remarks>see https://gist.github.com/matt-hickford/2277055 </remarks>
public static class LinqExtensions
{
    /// <summary>Groups the adjacent elements of a sequence according to a specified key selector function and creates a result
    /// value from each group and its key. The keys are compared by using a specified comparer.</summary>
    /// <typeparam name="TElement">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/>whose elements to group. </param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare keys with.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> where each element contains a collection of objects of type
    /// <typeparamref name="TElement"/> and a key.</returns>
    public static IEnumerable<IGrouping<TKey, TElement>> GroupAdjacentBy<TElement, TKey>(
        this IEnumerable<TElement> source,
        Func<TElement, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey: notnull
    {
        source.MustNotBeNull(nameof(source));
        keySelector.MustNotBeNull(nameof(keySelector));

        comparer = comparer ?? EqualityComparer<TKey>.Default;
        List<TElement>? elements = null;
        var key = default(TKey);
        var lastKey = default(TKey);
        foreach (var x in source)
        {
            key = keySelector(x);
            if (elements != null && elements.Any() && !comparer.Equals(lastKey!, key))
            {
                yield return new Grouping<TKey, TElement>(lastKey!, elements);
                elements = null;
            }

            if (elements == null)
            {
                elements = new List<TElement>();
                lastKey = key;
            }

            elements.Add(x);
        }

        if (elements != null && elements.Any())
            yield return new Grouping<TKey, TElement>(key!, elements);
    }

    /// <summary>Groups the adjacent elements of a sequence according to their values and creates a result value from each
    /// group. The values are compared by using a specified comparer.</summary>
    /// <typeparam name="TElement">The type of the elements of <paramref name="source"/>.</typeparam>
    /// <param name="source">An <see cref="IEnumerable{T}"/>whose elements to group. </param>
    /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare elements with.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> where each element contains a collection of objects of type
    /// <typeparamref name="TElement"/> and their value as a key.</returns>
    public static IEnumerable<IGrouping<TElement, TElement>> GroupAdjacentBy<TElement>(
        this IEnumerable<TElement> source,
        IEqualityComparer<TElement>? comparer = null)
        where TElement: notnull
    {
        return source.GroupAdjacentBy(x => x, comparer);
    }

    /// <summary>A helper class implementing <see cref="IGrouping{TKey, TElement}"/>.</summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TElement">The type of the elements.</typeparam>
    public sealed class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        /// <summary>Initializes a new instance of the <see cref="Grouping{TKey,TElement}"/> class.</summary>
        /// <param name="key">A key value.</param>
        /// <param name="elements">A list of elements.</param>
        public Grouping([NotNull] TKey key, List<TElement> elements)
        {
            Key = key;
            Elements = elements;
        }

        private List<TElement> Elements { get; }

        /// <summary>Gets the key.</summary>
        public TKey Key { get; }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TElement>) this).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
        {
            return ((IEnumerable<TElement>) Elements).GetEnumerator();
        }
    }
}