namespace Refactorius;

/// <summary>Represents a range of enumerable items.</summary>
/// <typeparam name="T">The type of range.</typeparam>
/// <author>http://code.google.com/p/magnum/</author>
[PublicAPI]
public class Range<T> : IEquatable<Range<T>> where T: struct
{
    #region private fields

    #endregion

    /// <summary>Initializes a new instance of the <see cref="Range{T}"/> class.</summary>
    public Range()
        : this(default, default, true, true, Comparer<T>.Default)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Range{T}"/> class.</summary>
    /// <param name="lowerBound">The lower bound of the range (inclusive).</param>
    /// <param name="upperBound">The upper bound of the range (inclusive).</param>
    public Range(T lowerBound, T upperBound)
        : this(lowerBound, upperBound, true, true, Comparer<T>.Default)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Range{T}"/> class.</summary>
    /// <param name="lowerBound">The lower bound of the range.</param>
    /// <param name="upperBound">The upper bound of the range.</param>
    /// <param name="includeLowerBound">If the lower bound should be included.</param>
    /// <param name="includeUpperBound">If the upper bound should be included.</param>
    public Range(T lowerBound, T upperBound, bool includeLowerBound, bool includeUpperBound)
        : this(lowerBound, upperBound, includeLowerBound, includeUpperBound, Comparer<T>.Default)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Range{T}"/> class.</summary>
    /// <param name="lowerBound">The lower bound of the range.</param>
    /// <param name="upperBound">The upper bound of the range.</param>
    /// <param name="includeLowerBound">If the lower bound should be included.</param>
    /// <param name="includeUpperBound">If the upper bound should be included.</param>
    /// <param name="comparer">The comparison to use for the range elements.</param>
    private Range(T lowerBound, T upperBound, bool includeLowerBound, bool includeUpperBound, IComparer<T> comparer)
    {
        Comparer = comparer;
        LowerBound = lowerBound;
        UpperBound = upperBound;
        IncludeLowerBound = includeLowerBound;
        IncludeUpperBound = includeUpperBound;
    }

    /// <summary>Gets the lower bound of the range.</summary>
    /// <value>The lower bound of the range.</value>
    public T LowerBound { get; }

    /// <summary>Gets the upper bound of the range.</summary>
    /// <value>The upper bound of the range.</value>
    public T UpperBound { get; }

    /// <summary>Gets the comparer used for the elements in the range.</summary>
    /// <value>The comparer used for the elements in the range.</value>
    public IComparer<T> Comparer { get; }

    /// <summary>Gets a value indicating whether the lower bound is included in the range.</summary>
    /// <value><see langword="true"/> to include the lower bound in the range (closed interval), <see langword="false"/> to
    /// exclude it (open interval).</value>
    public bool IncludeLowerBound { get; }

    /// <summary>Gets a value indicating whether the upper bound is included in the range.</summary>
    /// <value><see langword="true"/> to include the upper bound in the range (closed interval), <see langword="false"/> to
    /// exclude it (open interval).</value>
    public bool IncludeUpperBound { get; }

    #region Equality Stuff

    /// <summary>Compares two <see cref="Range{T}"/> instance for value equality.</summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns><see langword="true"/> if all properties of <paramref name="left"/> and <paramref name="right"/> instances are
    /// equal.</returns>
    public static bool operator ==(Range<T> left, Range<T> right)
    {
        return Equals(left, right);
    }

    /// <summary>Compares two <see cref="Range{T}"/> instance for value non-equality.</summary>
    /// <param name="left">The left instance.</param>
    /// <param name="right">The right instance.</param>
    /// <returns><see langword="true"/> if all properties of <paramref name="left"/> and <paramref name="right"/> instances
    /// differs.</returns>
    public static bool operator !=(Range<T> left, Range<T> right)
    {
        return !Equals(left, right);
    }

    /// <summary>Compares the current <see cref="Range{T}"/> instance with another instance.</summary>
    /// <param name="other">The other instance.</param>
    /// <returns><see langword="true"/> if all properties of the current <see cref="Range{T}"/> instance and
    /// <paramref name="other"/> are equal.</returns>
    public bool Equals(Range<T>? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return other.IncludeLowerBound.Equals(IncludeLowerBound)
               && other.IncludeUpperBound.Equals(IncludeUpperBound)
               && Equals(other.LowerBound, LowerBound)
               && Equals(other.UpperBound, UpperBound);
    }

    /// <summary>Compares the current <see cref="Range{T}"/> instance with another object.</summary>
    /// <param name="obj">The other object.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="Range{T}"/> and all properties of the current
    /// <see cref="Range{T}"/> instance and <paramref name="obj"/> are equal.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != typeof(Range<T>))
            return false;

        return Equals((Range<T>) obj);
    }

    /// <summary>Returns the hash code of the current <see cref="Range{T}"/> instance.</summary>
    /// <returns>The hash code of the current <see cref="Range{T}"/> instance.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var result = IncludeLowerBound.GetHashCode();
            result = (result * 397) ^ IncludeUpperBound.GetHashCode();
            result = (result * 397) ^ LowerBound.GetHashCode();
            result = (result * 397) ^ UpperBound.GetHashCode();
            return result;
        }
    }

    #endregion

    /// <summary>Determines if the value specified is contained within the range.</summary>
    /// <param name="value">The value to check.</param>
    /// <returns>Returns true if the value is contained within the range, otherwise false.</returns>
    public bool Contains(T value)
    {
        var left = Comparer.Compare(value, LowerBound);
        if (Comparer.Compare(value, LowerBound) < 0 || left == 0 && !IncludeLowerBound)
            return false;

        var right = Comparer.Compare(value, UpperBound);
        return right < 0 || right == 0 && IncludeUpperBound;
    }

#if not_yet /// <summary>
/// Returns a forward enumerator for the range
/// </summary>
/// <param name="step">A function used to step through the range</param>
/// <returns>An enumerator for the range</returns>
        public RangeEnumerator<T> Forward(Func<T, T> step)
        {
            return new RangeEnumerator<T>(this, step);
        }
#endif
}