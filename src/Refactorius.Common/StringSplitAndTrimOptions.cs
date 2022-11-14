using JetBrains.Annotations;
using System;

namespace Refactorius
{
    /// <summary>Specifies how <see cref="StringUtils.SplitAndTrim(string,string,StringSplitAndTrimOptions)"/> method trims the
    /// return value tokens and whether it includes or omits from the return value empty tokens.
    /// <para>This enumeration has a <see cref="FlagsAttribute"/> attribute that allows a bitwise combination of its member
    /// values.</para>
    /// </summary>
    /// <seealso cref="StringUtils"/>
    [PublicAPI]
    [Flags]
    public enum StringSplitAndTrimOptions
    {
        /// <summary>The return value includes array elements that contain an empty string. The elements are not trimmed.</summary>
        None = 0,

        /// <summary>The return value does not include array elements that contain an empty string.</summary>
        RemoveEmptyEntries = 1,

        /// <summary>The elements of the return value are trimmed at the start.</summary>
        /// <seealso cref="string.TrimStart"/>
        TrimAtStart = 2,

        /// <summary>The elements of the return value are trimmed at the end.</summary>
        /// <seealso cref="string.TrimEnd"/>
        TrimAtEnd = 4,

        /// <summary>The elements of the return value are trimmed and
        /// all sequences of one or more whitespaces replaced with a single space.</summary>
        /// <seealso cref="Squeeze"/>
        Squeeze = 8,

        /// <summary>The elements of the return value are trimmed at both start and end.</summary>
        /// <seealso cref="string.Trim()"/>
        TrimBoth = TrimAtStart | TrimAtEnd,

        /// <summary>The return value does not include array elements that contain an empty string. The elements of the return
        /// value are trimmed at both start and end.</summary>
        /// <seealso cref="string.Trim()"/>
        Default = RemoveEmptyEntries | TrimBoth
    }
}