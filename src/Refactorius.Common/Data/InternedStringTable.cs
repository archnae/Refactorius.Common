using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Refactorius.Data;

/// <summary>A user-space variant of the system interned string table. It provides functionality similar to
/// <b>System.String.Intern()</b> method but allows to clear interned strings and keeps hit-and-miss counters.</summary>
/// <remarks>This class is useful when the code has to load into memory a <b>very</b> big dataset containing a lot of
/// duplicate string values. Without some kind of interning, after few hundred thousands string fields are loaded, .NET
/// framework becomes a memory hog, even if all fields have the same value.
/// <para>Using <b>String.Intern()</b> method helps, but interned strings cannot be unloaded, so sooner or later the memory
/// problem arises again.</para>
/// </remarks>
[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors",
    Justification = "Class must have at least a protected constructor to be ihneritable.")]
public static class InternedStringTable
{
    /// <summary>Gets or sets the maximal number of truly interned strings.</summary>
    /// <value>The maximal number of truly interned strings.</value>
    public static int InternedLimit
    {
        get { return _internedLimit; }
        set { _internedLimit = Math.Max(0, value); }
    }

    /// <summary>Gets or sets the maximal number of user-space interned strings.</summary>
    /// <value>The maximal number of user-space interned strings.</value>
    public static int AddedLimit
    {
        get { return _addedLimit; }
        set { _addedLimit = Math.Max(0, value); }
    }

    /// <summary>Gets the number of truly interned strings.</summary>
    /// <value>The number of truly interned strings.</value>
    public static int InternedCount => _internedCount;

    /// <summary>Gets the number of user-space interned strings.</summary>
    /// <value>The number of user-space interned strings.</value>
    // ReSharper disable once InconsistentlySynchronizedField
    public static int AddedCount => _strings.Count;

    /// <summary>Gets the <b>hit</b> count (reused system- or user-space interned strings).</summary>
    /// <value>The <b>hit</b> count.</value>
    public static int HitCount => _hitCount;

    /// <summary>Gets the <b>miss</b> count.</summary>
    /// <value>The <b>miss</b> count.</value>
    public static int MissCount => _missCount;

    /// <summary>Retrieves the interned value of the specified string, if possible.</summary>
    /// <param name="value">A string to intern.</param>
    /// <returns>the interned value of the specified string, if possible, or <paramref name="value"/> itself if not.</returns>
    public static string Intern(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var isMiss = false;
        try
        {
            var internedValue = string.IsInterned(value);
            if (internedValue != null)
                return internedValue;

            if (value.Length < DEFAULT_MAX_LENGTH)
            {
                if (_strings.TryGetValue(value, out internedValue))
                    return internedValue;

                if (_internedCount < _internedLimit)
                {
                    Interlocked.Increment(ref _internedCount);
                    return string.Intern(value);
                }

                if (_strings.Count < _addedLimit)
                {
                    _strings.TryAdd(value, value);
                    return value;
                }
            }

            isMiss = true;
            return value;
        }
        finally
        {
            if (isMiss)
                Interlocked.Increment(ref _missCount);
            else
                Interlocked.Increment(ref _hitCount);
        }
    }

    /// <summary>Clears user-space interned strings table.</summary>
    /// <remarks>This method calls <see cref="GC.Collect()"/> to reuse freed memory space.</remarks>
    [SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods",
        Justification =
            "Method explicitly uses garbage collection to reclaim potentially huge interned string memory pool.")]
    public static void Clear()
    {
        _strings.Clear();

        // collect memory as table is supposed to be really big
        GC.Collect();
    }

    #region private fields and constants

    private const int DEFAULT_CONCURRENCY = 4;
    private const int DEFAULT_SIZE = 4 * 1024;
    private const int DEFAULT_INTERNED_LIMIT = 8 * 1024;
    private const int DEFAULT_ADD_LIMIT = 16 * 1024;
    private const int DEFAULT_MAX_LENGTH = 32 * 1024;

    private static readonly ConcurrentDictionary<string, string> _strings =
        new ConcurrentDictionary<string, string>(DEFAULT_CONCURRENCY, DEFAULT_SIZE, StringComparer.Ordinal);

    private static int _internedLimit = DEFAULT_INTERNED_LIMIT;
    private static int _addedLimit = DEFAULT_ADD_LIMIT;

    private static int _internedCount;
    private static int _hitCount;
    private static int _missCount;

    #endregion
}