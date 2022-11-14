using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Refactorius
{
    /// <summary>The collection of useful string formatting methods.</summary>
    public static class FormatUtils
    {
        /// <summary>Replaces the format items in a specified <see cref="string"/> with the text equivalents of the value of a
        /// corresponding <b>object</b> instance in a specified array.</summary>
        /// <param name="format">A format string (see <see cref="string.Format(string, object[])"/>).</param>
        /// <param name="args">An <b>object</b> array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="string"/>
        /// equivalent of the corresponding instances of <b>object</b> in <paramref name="args"/>.</returns>
        /// <remarks>This method is a culture-invariant alias for <see cref="string.Format(string,object[])"/>.</remarks>
        /// <seealso cref="System.String.Format(string,object[])"/>
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames",
            MessageId = "0#", Justification = "Imitates String.Format().")]
        [StringFormatMethod("format")]
        [ContractAnnotation("format:null=>null;format:notnull=>notnull")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Deprecated, use FormatWith instead")]
        public static string Format(string format, params object[] args)
        {
            return string.IsNullOrEmpty(format) || args.Length == 0
                ? format
                : string.Format(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>Replaces the format items and Ant-style expressions in a specified <see cref="string"/> with the text
        /// equivalents of the value of a corresponding <b>object</b> instance in a specified array or dictionary.</summary>
        /// <param name="format">A format string (see <see cref="string.Format(string,object[])"/>).</param>
        /// <param name="namedArgs">A <b>StringUtils.Format(string,IDictionary&lt;string, object&gt;</b> of name/value pairs.</param>
        /// <param name="args">An <b>object</b> array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format and Ant expressions have been replaced by the
        /// <see cref="string"/> equivalent of the corresponding values in <paramref name="args"/> and <paramref name="namedArgs"/>
        /// .</returns>
        /// <remarks>This method is culture-invariant.</remarks>
        /// <seealso cref="string.Format(string,object[])"/>
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames",
            MessageId = "0#", Justification = "Imitates String.Format().")]
        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Deprecated, use FormatWith instead")]
        public static string? Format(
            string format,
            IDictionary<string, object> namedArgs,
            params object[] args)
        {
            // TODO: provide an overload with a IFormatProvider parameter
            // TODO: it is a very stupid implementation
            return string.IsNullOrEmpty(format) ? format : Format(format.AntFormat(namedArgs), args);
        }

        /// <summary>Replaces the format items in a specified <see cref="string"/> with the text equivalents of the value of a
        /// corresponding <b>object</b> instance in a specified array.</summary>
        /// <param name="format">A format string (see <see cref="string.Format(string, object[])"/>).</param>
        /// <param name="args">An <b>object</b> array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="string"/>
        /// equivalent of the corresponding instances of <b>object</b> in <paramref name="args"/>.</returns>
        /// <remarks>This method differs from <see cref="string.Format(string,object[])"/> in that it is culture-invariant.</remarks>
        /// <seealso cref="string.Format(string,object[])"/>
        [StringFormatMethod("format")]
        [ContractAnnotation("format:null=>null;format:notnull=>notnull")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatWith(this string format, params object[] args)
        {
            return format.FormatWith(CultureInfo.InvariantCulture, args);
        }

        /// <summary>Replaces the format items in a specified <see cref="string"/> with the text equivalents of the value of a
        /// corresponding <b>object</b> instance in a specified array.</summary>
        /// <param name="format">A format string (see <see cref="string.Format(string, object[])"/>).</param>
        /// <param name="namedArgs">A <b>StringUtils.Format(string,IDictionary&lt;string, object&gt;</b> of name/value pairs.</param>
        /// <param name="args">An <b>object</b> array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="string"/>
        /// equivalent of the corresponding instances of <b>object</b> in <paramref name="args"/>.</returns>
        /// <remarks>This method differs from <see cref="string.Format(string,object[])"/> in that it is culture-invariant.</remarks>
        /// <seealso cref="string.Format(string,object[])"/>
        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string? FormatWith(
            this string format,
            IDictionary<string, object> namedArgs,
            params object[] args)
        {
            return format.AntFormat(namedArgs).FormatWith(CultureInfo.InvariantCulture, args);
        }

        /// <summary>Replaces the format items in a specified <see cref="string"/> with the text equivalents of the value of a
        /// corresponding <b>object</b> instance in a specified array.</summary>
        /// <param name="format">A format string (see <see cref="string.Format(string, object[])"/>).</param>
        /// <param name="provider">An <see cref=" System.IFormatProvider"/> that supplies culture-specific formatting information.</param>
        /// <param name="args">An <b>object</b> array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="string"/>
        /// equivalent of the corresponding instances of <b>object</b> in <paramref name="args"/>.</returns>
        /// <seealso cref="String.Format(string,object[])"/>
        [StringFormatMethod("format")]
        [ContractAnnotation("format:null=>null;format:notnull=>notnull")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FormatWith(this string format, IFormatProvider provider,
            params object[] args)
        {
            if (string.IsNullOrEmpty(format) || args.Length == 0)
                return format;

            // ReSharper disable once ConstantNullCoalescingCondition
            return string.Format(provider ?? CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>Replaces the format items in a specified <see cref="string"/> with the text equivalents of the value of a
        /// corresponding <b>object</b> instance in a specified array, protecting the caller from possible exceptions.</summary>
        /// <param name="provider">An <see cref=" System.IFormatProvider"/> that supplies culture-specific formatting information.</param>
        /// <param name="format">A format string (see <see cref="string.Format(string, object[])"/>).</param>
        /// <param name="args">An <b>object</b> array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="string"/>
        /// equivalent of the corresponding instances of <b>object</b> in <paramref name="args"/>.</returns>
        /// <remarks>This method differs from <see cref="string.Format(string,object[])"/> in that it <b>never</b> throws an
        /// exception, which makes it safe to use in exception constructors and other error-reporting code. It is also
        /// culture-invariant.</remarks>
        /// <seealso cref="String.Format(string,object[])"/>
        [StringFormatMethod("format")]
        [ContractAnnotation("format:null=>null;format:notnull=>notnull")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SafeFormat(IFormatProvider provider, string format,
            params object[] args)
        {
            return format.SafeFormat(provider, args);
        }

        /// <summary>Replaces the format items in a specified <see cref="string"/> with the text equivalents of the value of a
        /// corresponding <b>object</b> instance in a specified array, protecting the caller from possible exceptions.</summary>
        /// <param name="format">A format string (see <see cref="string.Format(string, object[])"/>).</param>
        /// <param name="args">An <b>object</b> array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="string"/>
        /// equivalent of the corresponding instances of <b>object</b> in <paramref name="args"/>.</returns>
        /// <remarks>This method differs from <see cref="string.Format(string,object[])"/> in that it <b>never</b> throws an
        /// exception, which makes it safe to use in exception constructors and other error-reporting code. It is also
        /// culture-invariant.</remarks>
        /// <seealso cref="String.Format(string,object[])"/>
        [ContractAnnotation("format:null=>null;format:notnull=>notnull")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SafeFormat(this string format, params object[] args)
        {
            return format.SafeFormat(CultureInfo.InvariantCulture, args);
        }

        /// <summary>Replaces the format items in a specified <see cref="string"/> with the text equivalents of the value of a
        /// corresponding <b>object</b> instance in a specified array, protecting the caller from possible exceptions.</summary>
        /// <param name="format">A format string (see <see cref="string.Format(string, object[])"/>).</param>
        /// <param name="provider">An <see cref=" System.IFormatProvider"/> that supplies culture-specific formatting information.</param>
        /// <param name="args">An <b>object</b> array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="string"/>
        /// equivalent of the corresponding instances of <b>object</b> in <paramref name="args"/>.</returns>
        /// <remarks>This method differs from <see cref="string.Format(string,object[])"/> in that it <b>never</b> throws an
        /// exception, which makes it safe to use in exception constructors and other error-reporting code. It is also
        /// culture-invariant.</remarks>
        /// <seealso cref="String.Format(string,object[])"/>
        [StringFormatMethod("format")]
        [ContractAnnotation("format:null=>null;format:notnull=>notnull")]
        public static string SafeFormat(this string format, IFormatProvider provider,
            params object?[] args)
        {
            if (string.IsNullOrEmpty(format) || args.Length == 0)
                return format;

            try
            {
                // ReSharper disable once ConstantNullCoalescingCondition
                return string.Format(provider ?? CultureInfo.InvariantCulture, format, args);
            }
            catch (Exception ex)
            {
                if (ex.ShouldRethrow())
                    throw;

                return string.Format(CultureInfo.InvariantCulture, "Cannot format '{0}'- {1}", format, ex.Message);
            }
        }

        /// <summary>Replaces the format items in a specified <see cref="string"/> with the text equivalents of the value of a
        /// corresponding <b>object</b> instance in a specified array, protecting the caller from possible exceptions.</summary>
        /// <param name="format">A format string (see <see cref="string.Format(string, object[])"/>).</param>
        /// <param name="namedArgs">A <b>StringUtils.Format(string,IDictionary&lt;string, object&gt;</b> of name/value pairs.</param>
        /// <param name="args">An <b>object</b> array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format items have been replaced by the <see cref="string"/>
        /// equivalent of the corresponding instances of <b>object</b> in <paramref name="args"/>.</returns>
        /// <remarks>This method differs from <see cref="string.Format(string,object[])"/> in that it <b>never</b> throws an
        /// exception, which makes it safe to use in exception constructors and other error-reporting code. It is also
        /// culture-invariant.</remarks>
        /// <seealso cref="String.Format(string,object[])"/>
        [StringFormatMethod("format")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string SafeFormat(this string format, IDictionary<string, object> namedArgs,
            params object[] args)
        {
            if (string.IsNullOrEmpty(format) || args.Length == 0)
                return format;

            // TODO: provide an overload with a IFormatProvider parameter
            try
            {
                format = format.AntFormat(namedArgs);

                // ReSharper disable once ConstantNullCoalescingCondition
                return string.Format(CultureInfo.InvariantCulture, format, args);
            }
            catch (Exception ex)
            {
                if (ex.ShouldRethrow())
                    throw;

                return string.Format(CultureInfo.InvariantCulture, "Cannot format '{0}'- {1}", format, ex.Message);
            }
        }
    }
}