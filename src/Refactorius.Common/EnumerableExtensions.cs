using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Refactorius
{
    /// <summary>Handy extension methods for <see cref="IEnumerable{T}"/>.</summary>
    [PublicAPI]
    public static class EnumerableExtensions
    {
        /// <summary>Clones a sequence of <see cref="ICloneable"/> elements.</summary>
        /// <typeparam name="T">The <c>Type</c> of sequence elements.</typeparam>
        /// <param name="source">The sequence to clone.</param>
        /// <returns>The sequence of the cloned elements of <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>Changed in v11 to non null-propagating. Use ?. operator instead.</remarks>
        [LinqTunnel]
        [ContractAnnotation("null => null; notnull => notnull")]
        public static IEnumerable<T> CloneSequence<T>(this IEnumerable<T> source)
            where T : class, ICloneable
        {
            return source.MustNotBeNull(nameof(source))
                .Select(x => (T) x.Clone());
        }

        /// <summary>Filters out default (usually <see langword="null"/>) values from a sequence.</summary>
        /// <typeparam name="T">An item <c>Type</c>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> sequence.</param>
        /// <returns>The sequence of items of the <paramref name="source"/> without empty (<see langword="null"/>) elements.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <remarks>Changed in v11: {T} should be a class.</remarks>
        [LinqTunnel]
        public static IEnumerable<T> WhereNotEmpty<T>(this IEnumerable<T?> source)
            where T: class
        {
            return source.MustNotBeNull(nameof(source))
                .Where(element => element != null)
                .Cast<T>();
        }

        /// <summary>Filters out empty (<see langword="null"/> or whitespaces only) values from a string sequence.</summary>
        /// <param name="source">An <see cref="IEnumerable{T}"/> sequence.</param>
        /// <returns>The sequence of items of the <paramref name="source"/> without empty (<see langword="null"/>) elements.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> is <see langword="null"/>.</exception>
        [LinqTunnel]
        public static IEnumerable<string> WhereNotEmpty(this IEnumerable<string?> source)
        {
            return source.MustNotBeNull(nameof(source))
                .Where(element => !string.IsNullOrWhiteSpace(element))
                .Cast<string>();
        }

        /// <summary>Executes a specified action on every item.</summary>
        /// <typeparam name="T">An item <c>Type</c>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> source.</param>
        /// <param name="action">An action to execute on every item.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> or <paramref name="action"/> is
        /// <see langword="null"/>.</exception>
        public static void ForEach<T>(
            [InstantHandle] this IEnumerable<T> source,
            [InstantHandle] Action<T> action)
        {
            source.MustNotBeNull(nameof(source));
            action.MustNotBeNull(nameof(action));

            foreach (var element in source)
                action(element);
        }

        /// <summary>Executes a specified action on every item, providing the item index.</summary>
        /// <typeparam name="T">An item <c>Type</c>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> source.</param>
        /// <param name="action">An action to execute on every item.</param>
        /// <returns>The original <see cref="IEnumerable{T}"/> source. <b>Warning:</b> using the original <paramref name="source"/>
        /// or the returned value after this call will cause a new enumeration of the <paramref name="source"/>.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> or <paramref name="action"/> is
        /// <see langword="null"/>.</exception>
        public static void ForEach<T>(
            [InstantHandle] this IEnumerable<T> source,
            [InstantHandle] Action<T, int> action)
        {
            source.MustNotBeNull(nameof(source));
            action.MustNotBeNull(nameof(action));

            var index = 0;

            foreach (var element in source)
            {
                action(element, index);
                index++;
            }
        }

        /// <summary>Executes a specified action on every item of a sequence while it is enumerated.</summary>
        /// <typeparam name="T">An item <c>Type</c>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> source.</param>
        /// <param name="action">An action to execute on every item.</param>
        /// <returns>The sequence of the original items of the <paramref name="source"/>. <b>Warning:</b> no
        /// <paramref name="action"/> is executed on items that were <b>not</b> enumerated.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> or <paramref name="action"/> is
        /// <see langword="null"/>.</exception>
        [LinqTunnel]
        public static IEnumerable<T> PassThrough<T>(
            this IEnumerable<T> source,
            [InstantHandle] Action<T> action)
        {
            source.MustNotBeNull(nameof(source));
            action.MustNotBeNull(nameof(action));

            // ReSharper disable once PossibleMultipleEnumeration
            return source.Select(
                element =>
                {
                    action(element);
                    return element;
                });
        }

        /// <summary>Executes a specified action on every item of a sequence while it is enumerated, providing the item index.</summary>
        /// <typeparam name="T">An item <c>Type</c>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> source.</param>
        /// <param name="action">An action to execute on every item.</param>
        /// <returns>The sequence of the original items of the <paramref name="source"/>. <b>Warning:</b> no
        /// <paramref name="action"/> is executed on items that were <b>not</b> enumerated.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> or <paramref name="action"/> is
        /// <see langword="null"/>.</exception>
        [LinqTunnel]
        public static IEnumerable<T> PassThrough<T>(
            this IEnumerable<T> source,
            [InstantHandle] Action<T, int> action)
        {
            source.MustNotBeNull(nameof(source));
            action.MustNotBeNull(nameof(action));

            return source.Select(
                (element, idx) =>
                {
                    action(element, idx);
                    return element;
                });
        }

#if FOR_LATER_TryForEach /// <summary>
/// Executes a specified action on every item.
/// </summary>
/// <typeparam name="T">An item <c>Type</c>.</typeparam>
/// <param name="source">An <see cref="IEnumerable{T}"/> source.</param>
/// <param name="action">An action to execute on every item.</param>
/// <param name="errorAction">An action to execute on exception.</param>
/// <returns>The original <see cref="IEnumerable{T}"/> source.</returns>
        public static IEnumerable<T> TryForEach<T>(this IEnumerable<T> source, Action<Exception> errorAction, Action<T> action)
        {
            source.MustNotBeNull("source");
            action.MustNotBeNull("action");
            errorAction.MustNotBeNull("errorAction");

            foreach (T element in source)
            {
                try
                {
                    action(element);
                }
                catch (Exception ex)
                {
                    if (ex.IsCriticalOrThreadAbort())
                        throw;
                    errorAction(ex);
                }
            }

            return source;
        }

        /// <summary>
        /// Executes a specified action on every item, logging and then ignoring exceptions.
        /// </summary>
        /// <typeparam name="TSource">An source item <c>Type</c>.</typeparam>
        /// <typeparam name="TResult">A result item <c>Type</c>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> source.</param>
        /// <param name="selector">A transform function to apply to each item.</param>
        /// <param name="errorAction">An action to execute on exception.</param>
        /// <returns>The original <see cref="IEnumerable{T}"/> source.</returns>
        public static IEnumerable<TResult> TrySelect<TSource, TResult>(this IEnumerable<TSource> source, Action<Exception> errorAction, Func<TSource, TResult> selector)
        {
            source.MustNotBeNull("source");
            selector.MustNotBeNull("selector");
            errorAction.MustNotBeNull("errorAction");

            foreach (TSource element in source)
            {
                TResult result = default(TResult);
                bool ok = false;
                try
                {
                    result = selector(element);
                    ok = true;
                }
                catch (Exception ex)
                {
                    if (ex.IsCriticalOrThreadAbort())
                        throw;
                    errorAction(ex);
                }
                if (ok)
                {
                    yield return result;
                }
            }

            yield break;
        }
#endif

        /// <summary>Adds new entries to a dictionary or replaces existing ones, returning the dictionary for call chaining.</summary>
        /// <typeparam name="TSource">The type of sequence items.</typeparam>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="source">A sequence of items.</param>
        /// <param name="keySelector">A key mapping function.</param>
        /// <param name="valueSelector">A value mapping function.</param>
        /// <returns>The new sequence of <see cref="KeyValuePair{TKey, TValue}"/> containing items from <paramref name="source"/>
        /// converted by <paramref name="keySelector"/> and <paramref name="valueSelector"/>.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/>, <paramref name="keySelector"/> or
        /// <paramref name="valueSelector"/> is <see langword="null"/>.</exception>
        [LinqTunnel]
        public static IEnumerable<KeyValuePair<TKey, TValue>> Select<TSource, TKey, TValue>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TValue> valueSelector)
        {
            source.MustNotBeNull(nameof(source));
            keySelector.MustNotBeNull(nameof(keySelector));
            valueSelector.MustNotBeNull(nameof(valueSelector));

            return source.Select(item => new KeyValuePair<TKey, TValue>(keySelector(item), valueSelector(item)));
        }

        /// <summary>Groups the adjacent elements of a sequence of chunks (sub-sequences) of a specified size.</summary>
        /// <typeparam name="TElement">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/>whose elements to group. </param>
        /// <param name="chunkSize">The size of the chunk.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> where each element contains a subsequence of <paramref name="source"/>. </returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="chunkSize"/> is zero or negative.</exception>
        [LinqTunnel]
        public static IEnumerable<TElement[]> Chunk<TElement>(this IEnumerable<TElement> source,
            int chunkSize)
        {
            source.MustNotBeNull(nameof(source));
            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be a positive value.");

            var chunk = new List<TElement>(chunkSize);

            foreach (var item in source)
            {
                chunk.Add(item);
                if (chunk.Count == chunkSize)
                {
                    yield return chunk.ToArray();
                    chunk.Clear();
                }
            }

            if (chunk.Count > 0)
                yield return chunk.ToArray();
        }

        #region general purpose

        /// <summary>Maps an array into another array of the same size using a specified selector function.</summary>
        /// <typeparam name="TSource">The <c>Type</c> of the input array elements.</typeparam>
        /// <typeparam name="TResult">The <c>Type</c> of the output array elements.</typeparam>
        /// <param name="source">The input array.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns>The output array.</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="source"/> or <paramref name="selector"/> is
        /// <see langword="null"/>.</exception>
        public static TResult[] MapArray<TSource, TResult>(this TSource[] source,
            Func<TSource, TResult> selector)
        {
            source.MustNotBeNull(nameof(source));
            selector.MustNotBeNull(nameof(selector));

            var result = new TResult[source.Length];
            for (var i = 0; i < source.Length; i++)
                result[i] = selector(source[i]);
            return result;
        }

        /// <summary>Wraps a <see cref="IEnumerator{T}"/> into <see cref="IEnumerable{T}"/>.</summary>
        /// <typeparam name="T">The <c>Type</c> of sequence elements.</typeparam>
        /// <param name="enumerator">An enumerator to wrap.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> wrapping the <paramref name="enumerator"/>. It automatically disposes the
        /// <paramref name="enumerator"/> when the sequence is exhausted.
        /// <para><b>NB:</b> if the client abandons the enumeration before reaching the end, the <paramref name="enumerator"/> will
        /// <b>not</b> be disposed.</para>
        /// </returns>
        /// <exception cref="ArgumentNullException">if <paramref name="enumerator"/> is <see langword="null"/>.</exception>
        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator)
        {
            enumerator.MustNotBeNull(nameof(enumerator));

            while (enumerator.MoveNext())
                yield return enumerator.Current;

            enumerator.Dispose();
        }

        /// <summary>Converts a <c>IEnumerator{T[]}</c> into flattened <see cref="IEnumerable{T}"/>.</summary>
        /// <typeparam name="T">The <c>Type</c> of sequence elements.</typeparam>
        /// <param name="enumerator">An enumerator to wrap.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> wrapping and flattening the <paramref name="enumerator"/>. It automatically
        /// disposes the <paramref name="enumerator"/> when the sequence is exhausted.
        /// <para><b>NB:</b> if the client abandons the enumeration before reaching the end, the <paramref name="enumerator"/> will
        /// <b>not</b> be disposed.</para>
        /// </returns>
        /// <exception cref="ArgumentNullException">if <paramref name="enumerator"/> is <see langword="null"/>.</exception>
        public static IEnumerable<T> FlattenToIEnumerable<T>(this IEnumerator<T[]> enumerator)
        {
            return enumerator.ToIEnumerable().SelectMany(x => x);
        }

        #endregion
    }
}