//using System.Diagnostics.CodeAnalysis;

using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Global

namespace Refactorius;

/// <summary>The collection of useful string utility methods.</summary>
/// <remarks>Ripped off from <b>Spring.Net</b> framework <b>Spring.Util.StringUtils</b>.</remarks>
[PublicAPI]
public static class StringUtils
{
    /// <summary>Gets an an empty array of <see cref="string"/>.</summary>
    /// <value>An zero-length array of <see cref="string"/>.</value>
    //[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays",
    //    Justification = "Creates a new instance on every access.")]
    public static string[] EmptyStrings => Array.Empty<string>();

    /// <summary>Sanitizes a <see cref="string"/> into a valid <see cref="Path"/> by replacing all invalid characters
    /// in it with spaces.</summary>
    /// <param name="value">A path to sanitize.</param>
    /// <returns>A sanitized path.</returns>
    /// <seealso cref="Sanitize"/>
    /// <seealso cref="Path.GetInvalidPathChars"/>
    /// <remarks>Changed in v11 to non null-propagating. Use ?. operator instead.</remarks>
    [Pure]
    public static string SanitizePath(string value)
    {
        return value.Sanitize(Path.GetInvalidPathChars(), '_').Trim();
    }

    /// <summary>Sanitizes a <see cref="string"/> into a valid <see cref="File"/> name by replacing all invalid
    /// characters in it with spaces.</summary>
    /// <param name="value">A file name to sanitize.</param>
    /// <returns>A sanitized file name.</returns>
    /// <seealso cref="Sanitize"/>
    /// <seealso cref="Path.GetInvalidFileNameChars"/>
    /// <remarks>Changed in v11 to non null-propagating. Use ?. operator instead.</remarks>
    [Pure]
    public static string SanitizeFileName(string value)
    {
        return value.Sanitize(Path.GetInvalidFileNameChars(), '_').Trim();
    }

    /// <summary>Indicates whether the specified <see cref="string"/> object is <see langword="null"/> or an
    /// <see cref="string.Empty"/> string.</summary>
    /// <param name="value">A <see cref="string"/> reference.</param>
    /// <returns><see langword="true"/> if the value parameter is <see langword="null"/>  or an empty string (""); otherwise,
    /// <see langword="false"/>.</returns>
    [ContractAnnotation("value:null => true")]
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Obsolete("Use string.IsNullOrEmpty().")]
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>Tests whether a <see cref="string"/> is <see langword="null"/>, is empty or consists solely of whitespaces.
    /// <para>It is the inverse function to the <see cref="HasText"/>.</para>
    /// </summary>
    /// <param name="target">A <see cref="string"/> to test, may be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the <paramref name="target"/> is <see langword="null"/>, equal to
    /// <see cref="string.Empty"/> or is composed entirely of whitespace characters.</returns>
    /// <seealso cref="HasText"/>
    /// <remarks>It is the inverse function to the <see cref="HasText"/>.
    /// <para>This method is included to simplify porting of code from the <b>Spring.Net</b> framework.</para>
    /// </remarks>
    /// <example>
    /// <code language="C#">
    /// StringUtils.IsEmpty(null) == true
    /// StringUtils.IsEmpty("") == true
    /// StringUtils.IsEmpty(" ") == true
    /// StringUtils.IsEmpty("12345") == false
    /// StringUtils.IsEmpty(" 12345 ") = false
    /// </code>
    /// </example>
    [ContractAnnotation("target:null => true")]
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //[Obsolete("Use string.IsNullOrWhiteSpace().")]
    public static bool IsEmpty(this string? target)
    {
        return string.IsNullOrWhiteSpace(target);
    }

    /// <summary>Tests whether a <see cref="string"/> has length.
    /// <para>It is the inverse function to the <see cref="string.IsNullOrEmpty"/>.</para>
    /// </summary>
    /// <param name="target">A <see cref="string"/> to test, may be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the string is not <see langword="null"/> and has non-zero length, otherwise
    /// <see langword="false"/>.</returns>
    /// <remarks>This method is included to simplify porting of code from the <b>Spring.Net</b> framework.</remarks>
    /// <seealso cref="string.IsNullOrEmpty"/>
    /// <example>
    /// <code lang="C#">
    /// StringUtils.HasLength(null) == false
    /// StringUtils.HasLength("") == false
    /// StringUtils.HasLength(" ") == true
    /// StringUtils.HasLength("Hello") == true
    /// </code>
    /// </example>
    [Obsolete("Use !string.IsNullOrEmpty().")]
    [ContractAnnotation("target:null => false")]
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasLength(this string? target)
    {
        return !string.IsNullOrEmpty(target);
    }

    /// <summary>Tests whether a <see cref="string"/> has text.
    /// <para>It is the inverse function to the <see cref="IsEmpty"/>.</para>
    /// </summary>
    /// <param name="target">A <see cref="string"/> to test, may be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the string is not <see langword="null"/>, has non-zero length and does not consist
    /// solely of whitespace, otherwise <see langword="false"/>.</returns>
    /// <seealso cref="IsEmpty"/>
    /// <remarks>It is the inverse function to the <see cref="IsEmpty"/>.</remarks>
    /// <example>
    /// <code language="C#">
    /// StringUtils.HasText(null) == false
    /// StringUtils.HasText("") == false
    /// StringUtils.HasText(" ") == false
    /// StringUtils.HasText("12345") == true
    /// StringUtils.HasText(" 12345 ") = true
    /// </code>
    /// </example>
    [ContractAnnotation("target:null => true")]
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //[Obsolete("Use !string.IsNullOrWhiteSpace().")]
    public static bool HasText(this string target)
    {
        return !string.IsNullOrWhiteSpace(target);
    }

    /// <summary>Truncates a <see cref="string"/> to a specified length, adding at the end an ellipsis.</summary>
    /// <param name="value">A <see cref="string"/> to truncate or <see langword="null"/>.</param>
    /// <param name="maxLength">The length to which the <paramref name="value"/> should be truncated.</param>
    /// <returns>The original <paramref name="value"/> if it was <see langword="null"/>, empty or had length not greater that
    /// <paramref name="maxLength"/>; otherwise, the string of length <paramref name="maxLength"/>, consisting of the truncated
    /// <paramref name="value"/> and the <see cref="DefaultEllipsisPostfix"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="maxLength"/> is less then the length of
    /// <see cref="DefaultEllipsisPostfix"/>.</exception>
    /// <example>
    /// <code lang="C#">
    /// StringUtils.Ellipsis(null, 10) == null;
    /// StringUtils.Ellipsis("something", 12) == "something";
    /// StringUtils.Ellipsis("something", 9) == "something";
    /// StringUtils.Ellipsis("something", 5) == "s ...";
    /// </code>
    /// </example>
    /// <remarks>Changed in v11 to non null-propagating.</remarks>
    //[ContractAnnotation("value:null => null; value:notnull => notnull")]
    [Pure]
    public static string Ellipsis(this string? value, int maxLength)
    {
        value = value?.Trim() ?? string.Empty;
        if (maxLength <= DefaultEllipsisPostfix.Length)
            throw new ArgumentOutOfRangeException(
                nameof(maxLength),
                "The desired maximal length of the string with an ellipsis must be greater then the length of the ellipsis postfix");

        return string.IsNullOrEmpty(value) || value.Length <= maxLength
            ? value
            : value.Substring(0, maxLength - DefaultEllipsisPostfix.Length) + DefaultEllipsisPostfix;
    }

    /// <summary>Truncates a <see cref="string"/> to a specified length, adding at the beginning an ellipsis.</summary>
    /// <param name="value">A <see cref="string"/> to truncate or <see langword="null"/>.</param>
    /// <param name="maxLength">The length to which the <paramref name="value"/> should be truncated.</param>
    /// <returns>The original <paramref name="value"/> if it was <see langword="null"/>, empty or had length not greater that
    /// <paramref name="maxLength"/>; otherwise, the string of length <paramref name="maxLength"/>,
    /// consisting of the <see cref="DefaultEllipsisPostfix"/> and the truncated at the beginning <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="maxLength"/> is less then the length of
    /// <see cref="DefaultEllipsisPostfix"/>.</exception>
    /// <remarks>Changed in v11 to non null-propagating. Use ?. operator instead.</remarks>
    //[ContractAnnotation("value:null => null; value:notnull => notnull")]
    [Pure]
    public static string ReverseEllipsis(this string? value, int maxLength)
    {
        value = value?.Trim() ?? string.Empty;
        if (maxLength <= DefaultEllipsisPostfix.Length)
            throw new ArgumentOutOfRangeException(
                nameof(maxLength),
                "The desired maximal length of the string with an ellipsis must be greater then the length of the ellipsis prefix");
        return string.IsNullOrEmpty(value) || value.Length <= maxLength
            ? value
            : DefaultEllipsisPostfix + value.Substring(Math.Max(0, value.Length + DefaultEllipsisPostfix.Length - maxLength));
    }

    /// <summary>Truncates a <see cref="string"/> to a specified length, adding in the middle an ellipsis.</summary>
    /// <param name="value">A <see cref="string"/> to truncate or <see langword="null"/>.</param>
    /// <param name="maxLength">The length to which the <paramref name="value"/> should be truncated.</param>
    /// <returns>The original <paramref name="value"/> if it was <see langword="null"/>, empty or had length not greater that
    /// <paramref name="maxLength"/>; otherwise, the string of length <paramref name="maxLength"/>, consisting of the truncated
    /// in the middle <paramref name="value"/> with the <see cref="DefaultEllipsisPostfix"/> inserted.</returns>
    /// <exception cref="ArgumentException"><paramref name="maxLength"/> is less then the length of
    /// <see cref="DefaultEllipsisPostfix"/>.</exception>
    /// <remarks>Changed in v11 to non null-propagating. Use ?. operator instead.</remarks>
    [ContractAnnotation("value:null => null; value:notnull => notnull")]
    [Pure]
    public static string BiEllipsis(this string? value, int maxLength)
    {
        value = value?.Trim() ?? string.Empty;
        if (maxLength <= DefaultEllipsisPostfix.Length + 4)
            throw new ArgumentOutOfRangeException(
                nameof(maxLength),
                "The desired maximal length of the string with an ellipsis must be greater then the length of the ellipsis postfix");

        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;
        var halfLength = (maxLength - DefaultEllipsisPostfix.Length - 4) / 2;
        var head = value.Substring(0, halfLength);
        var tail = value.Substring(value.Length - halfLength);
        return $"{head} {DefaultEllipsisPostfix} {tail}";
    }

    /// <summary>Converts the <see cref="string"/> to camelCase, null-propagating.</summary>
    /// <param name="value">A <see cref="string"/> to convert.</param>
    /// <returns>A <paramref name="value"/>, converted to camelCase (first character in lower case, the rest unchanged). The
    /// value is trimmed before conversion. <see langword="null"/> value is returned unchanged.</returns>
    /// <remarks>The conversion to lower case uses the current culture.</remarks>
    [ContractAnnotation("value:null => null; value:notnull => notnull")]
    [Pure]
    public static string? ToCamelCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        value = value!.Trim();
        return value.Length > 0
            ? value.Substring(0, 1).ToLower(CultureInfo.CurrentCulture) + value.Substring(1)
            : value;
    }

    /// <summary>Capitalizes a <see cref="string"/>, null-propagating.</summary>
    /// <param name="value">The original <see cref="string"/> value.</param>
    /// <returns>A <paramref name="value"/>, converted to Capital (first character in upper case, the rest unchanged). The
    /// value is trimmed before conversion. <see langword="null"/> value is returned unchanged.</returns>
    /// <remarks>The conversion to upper case uses the current culture.</remarks>
    [ContractAnnotation("value:null => null; value:notnull => notnull")]
    [Pure]
    public static string? ToCapitalCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        value = value!.Trim();
        return value.Length > 0
            ? value.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + value.Substring(1)
            : value;
    }

    /// <summary>Replaces in <see cref="string"/> Windows- and Unix-style newlines with single space, null-propagating.</summary>
    /// <param name="value">The original <see cref="string"/>.</param>
    /// <returns>The <paramref name="value"/> with newlines replaced. If <paramref name="value"/> is <see langword="null"/> or
    /// <see cref="string.Empty"/>, the result is <see cref="string.Empty"/>.</returns>
    [ContractAnnotation("value:null => null; value:notnull => notnull")]
    [Pure]
    public static string? CompactNewLines(this string? value)
    {
        return string.IsNullOrEmpty(value)
            ? value
            : value!.Replace("\r\n", " ").Replace("\n", " ");
    }

    /// <summary>Sanitizes a <see cref="string"/> by replacing all invalid characters in it with the specified character.</summary>
    /// <param name="value">A string to sanitize.</param>
    /// <param name="invalidCharacters">An array of invalid characters.</param>
    /// <param name="replacementCharacter">A replacement character.</param>
    /// <returns>A trimmed <paramref name="value"/> with all occurrences of any of <paramref name="invalidCharacters"/> replaced
    /// by the <paramref name="replacementCharacter"/>.</returns>
    /// <remarks>Changed in v11 to non null-propagating. Use ?. operator instead.</remarks>
    [Pure]
    public static string Sanitize(this string value, char[] invalidCharacters, char replacementCharacter)
    {
        value.MustNotBeNull(nameof(value));
        invalidCharacters.MustNotBeNull(nameof(invalidCharacters));

        if (replacementCharacter == 0)
            throw new ArgumentOutOfRangeException(nameof(replacementCharacter),
                "The replacement character cannot be 0.");
        if (Array.IndexOf(invalidCharacters, replacementCharacter) >= 0)
            throw new ArgumentException("The invalid characters array contains the replacement character",
                nameof(replacementCharacter));

        if (string.IsNullOrEmpty(value))
            return value;

        var result = new char[value.Length];
        for (var i = 0; i < result.Length; i++)
        {
            var c = value[i];
            if (Array.IndexOf(invalidCharacters, c) >= 0)
                result[i] = replacementCharacter;
            else
                result[i] = c;
        }

        return new string(result);
    }

    /// <summary>Tokenize the given <see cref="string"/> into a <see cref="string"/> array, trimming the resulting tokens and
    /// removing the empty ones.</summary>
    /// <param name="value">The <see cref="string"/> to tokenize or <see langword="null"/>.</param>
    /// <param name="separators">The <see cref="string"/> of separator characters.</param>
    /// <returns>An array of the tokens.</returns>
    /// <remarks>If <paramref name="value"/> is <see langword="null"/>, returns an empty <see cref="string"/> array. If
    /// <paramref name="separators"/> is <see langword="null"/> or empty, returns a <see cref="string"/> array with one
    /// element: <paramref name="value"/> itself. Tokens are trimmed; empty tokens are removed from the result.</remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string[] SplitAndTrim(this string? value, string separators)
    {
        // TODO: SplitAndTrim re-squeeze is not efficient, handle default case better
        return SplitAndTrim(value, separators, StringSplitAndTrimOptions.Default);
    }

    /// <summary>Tokenize the given <see cref="string"/> into a <see cref="string"/> array according to the specified options.</summary>
    /// <param name="value">The <see cref="string"/> to tokenize or <see langword="null"/>.</param>
    /// <param name="separators">The delimiter characters, assembled as a <see cref="string"/>.</param>
    /// <param name="options">A combination of <see cref="StringSplitAndTrimOptions"/> flags, specifying how the result value
    /// elements are trimmed and whether empty elements are returned.</param>
    /// <returns>An array of the tokens.</returns>
    /// <remarks>If <paramref name="value"/> is <see langword="null"/>, returns an empty <see cref="string"/> array. If
    /// <paramref name="separators"/> is <see langword="null"/> or empty, returns a <see cref="string"/> array with one
    /// element: <paramref name="value"/> itself.</remarks>
    [Pure]
    public static string[] SplitAndTrim(
        this string? value,
        string separators,
        StringSplitAndTrimOptions options)
    {
        value ??= string.Empty;
        if (string.IsNullOrEmpty(value))
            return Array.Empty<string>();

        if (string.IsNullOrEmpty(separators))
            return new[] { value };

        if ((options & StringSplitAndTrimOptions.TrimBoth) == 0)
        {
            if ((options & StringSplitAndTrimOptions.RemoveEmptyEntries) != 0)
                return value.Split(separators.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return value.Split(separators.ToCharArray());
        }

        var result = value.Split(separators.ToCharArray());
        for (var i = 0; i < result.Length; i++)
            if ((options & StringSplitAndTrimOptions.Squeeze) != 0)
            {
                result[i] = result[i].Squeeze() ?? string.Empty;
            }
            else
            {
                if ((options & StringSplitAndTrimOptions.TrimAtStart) != 0)
                    result[i] = result[i].TrimStart();
                if ((options & StringSplitAndTrimOptions.TrimAtEnd) != 0)
                    result[i] = result[i].TrimEnd();
            }

        // get rid of empty items created by extra separators
        // TODO: SplitAndTrim re-squeeze is not efficient, handle empty entries better
        if ((options & StringSplitAndTrimOptions.RemoveEmptyEntries) != 0)
            result = string.Join(separators.Substring(0, 1), result)
                .Split(separators.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        return result;
    }

    /// <summary>Trims the argument and replaces all sequences of one or more whitespaces with a single space.</summary>
    /// <param name="value">A string to squeeze or <see langword="null"/>.</param>
    /// <returns>A squeezed <paramref name="value"/>.</returns>
    [ContractAnnotation("value:null => null")]
    [Pure]
    public static string? Squeeze(this string? value)
    {
        return string.IsNullOrEmpty(value) ? value : SqueezeRegex.Replace(value!.Trim(), " ");
    }

    /// <summary>Trims the argument and replaces result with <see langword="null"/> if it's empty.</summary>
    /// <param name="value">A string to trim or <see langword="null"/>.</param>
    /// <returns>A trimmed <paramref name="value"/> if it is not empty, <see langword="null"/> if it is.</returns>
    [ContractAnnotation("value:null => null")]
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? TrimToNull(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value!.Trim();
    }

    /// <summary>Trims the argument, replaces all sequences of one or more whitespaces with a single space and replaces result
    /// with <see langword="null"/> if it's empty.</summary>
    /// <param name="value">A string to squeeze or <see langword="null"/>.</param>
    /// <returns>A squeezed <paramref name="value"/> if it is not empty, <see langword="null"/> if it is.</returns>
    [ContractAnnotation("value:null => null")]
    [Pure]
    public static string? SqueezeToNull(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = SqueezeRegex.Replace(value!.Trim(), " ");

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    /// <summary>Concatenates a specified separator <see cref="string"/> between each element of a specified
    /// <see cref="string"/> sequence, removing empty elements and yielding a single concatenated string.</summary>
    /// <param name="value">A sequence of <see cref="string"/>.</param>
    /// <param name="separator">A separator <see cref="string"/>.</param>
    /// <param name="squeeze"><see langword="true"/> to squeeze all extra whitespaces out of <paramref name="value"/> elements,
    /// <see langword="false"/> just to trim them.</param>
    /// <returns>A <see cref="string"/> consisting of all squeezed or trimmed nonempty elements of <paramref name="value"/>
    /// interspersed with the <paramref name="separator"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    [Pure]
    public static string Join([InstantHandle] this IEnumerable<string?> value,
        string separator,
        bool squeeze = false)
    {
        value.MustNotBeNull(nameof(value));

        return string.Join(
            separator,
            value.WhereNotEmpty()
                .Select(squeeze ? SqueezeToNull : (Func<string?, string?>)TrimToNull)
                .WhereNotEmpty());
    }

    /// <summary>Concatenates a <see cref="string"/> sequence, removing empty elements, wrapping each element into pre/postfix
    /// <see cref="string"/> pair, using a specified separator <see cref="string"/> between elements and yielding a single
    /// concatenated string.</summary>
    /// <param name="value">A sequence of <see cref="string"/>.</param>
    /// <param name="separator">A separator <see cref="string"/>.</param>
    /// <param name="prefix">A prefix <see cref="string"/>.</param>
    /// <param name="postfix">A postfix <see cref="string"/>.</param>
    /// <param name="squeeze"><see langword="true"/> to squeeze all extra whitespaces out of <paramref name="value"/> elements,
    /// <see langword="false"/> just to trim them.</param>
    /// <returns>A <see cref="string"/> consisting of all squeezed or trimmed nonempty elements of <paramref name="value"/>
    /// interspersed with the <paramref name="separator"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string WrapAndJoin(
        [InstantHandle]
        this IEnumerable<string?> value,
        string separator,
        string prefix,
        string postfix,
        bool squeeze = false)
    {
        value.MustNotBeNull(nameof(value));

        return string.Join(
            separator,
            value.WhereNotEmpty()
                .Select(squeeze ? SqueezeToNull : (Func<string?, string?>)TrimToNull)
                .WhereNotEmpty()
                .Select(x => prefix + x + postfix));
    }

    /// <summary>
    /// Provides a default value for a <see langword="null"/> or empty string.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The original <paramref name="value"/> if it's not empty, otherwise <paramref name="defaultValue"/>.</returns>
    public static string WithDefault(this string? value, string defaultValue)
    {
        return string.IsNullOrWhiteSpace(value)
            ? defaultValue ?? string.Empty
            : value!;
    }

    #region public static constants

    /// <summary>The string that signals that a string was truncated.
    /// <para>Traditionally, it is a <c>" ..."</c>.</para>
    /// </summary>
    public const string DefaultEllipsisPostfix = " ...";

    /// <summary>The regular expression to squeeze extra whitespaces out of argument.</summary>
    public static readonly Regex SqueezeRegex = new(@"\s+");

    #endregion
}