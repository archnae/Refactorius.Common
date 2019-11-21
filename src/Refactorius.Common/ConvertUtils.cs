using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security;
using JetBrains.Annotations;
using Refactorius.Data;

namespace Refactorius
{
    /// <summary>The collection of useful conversion utility methods.</summary>
    [PublicAPI]
    public static class ConvertUtils
    {
        /// <summary>Gets the default combination of <see cref="DateTimeStyles"/>, used to convert <see cref="DateTime"/> to and
        /// from <see cref="string"/>.
        /// <para>It combines <see cref="DateTimeStyles.AssumeLocal"/>, <see cref="DateTimeStyles.AllowWhiteSpaces"/> and
        /// <see cref="DateTimeStyles.NoCurrentDateDefault"/> flags.</para>
        /// </summary>
        /// <value>The default combination of <see cref="DateTimeStyles"/>, used to convert <see cref="DateTime"/> to and from
        /// <see cref="string"/>.</value>
        /// <seealso cref="DateTimeStyles"/>
        public static DateTimeStyles DefaultDateTimeStyleLocal =>
            DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault;

        /// <summary>Gets the default combination of <see cref="DateTimeStyles"/>, used to convert <see cref="DateTime"/> to and
        /// from <see cref="string"/>.
        /// <para>It combines <see cref="DateTimeStyles.AssumeLocal"/>, <see cref="DateTimeStyles.AllowWhiteSpaces"/> and
        /// <see cref="DateTimeStyles.NoCurrentDateDefault"/> flags.</para>
        /// </summary>
        /// <value>The default combination of <see cref="DateTimeStyles"/>, used to convert <see cref="DateTime"/> to and from
        /// <see cref="string"/>.</value>
        /// <seealso cref="DateTimeStyles"/>
        public static DateTimeStyles DefaultDateTimeStyleUtc =>
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.NoCurrentDateDefault;

        /// <summary>Gets the default <see cref="DateTime"/> string format for Utc values.</summary>
        public static string DefaultDateTimeFormatUtc => "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";

        /// <summary>Gets the default <see cref="DateTime"/> string format for local time values.</summary>
        public static string DefaultDateTimeFormatLocal => "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz";

        /// <summary>Returns an <see cref="object"/> with the specified <c>Type</c> whose value is equivalent to the
        /// specified object.</summary>
        /// <typeparam name="TValue">The <c>Type</c> to which value is to be converted.</typeparam>
        /// <param name="value">An <see cref="object"/> to convert.</param>
        /// <returns><see langword="null"/> if <paramref name="value"/> is <see langword="null"/> and <typeparamref name="TValue"/>
        /// is <see cref="Nullable{T}"/> or a reference type. Otherwise, an object whose <c>Type</c> is
        /// <typeparamref name="TValue"/> and whose value is equivalent to <paramref name="value"/>.</returns>
        /// <remarks>Unlike <see cref="M:System.Convert.ChangeType"/> this method always uses
        /// <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        [CanBeNull]
        public static TValue ConvertTo<TValue>([CanBeNull] object value)
        {
            return (TValue)(ChangeType(value, typeof(TValue)) ?? default(TValue));
        }

        /// <summary>Converts a value to the specified type.</summary>
        /// <typeparam name="T">Type of value returned.</typeparam>
        /// <param name="value">A source value.</param>
        /// <param name="name">An optional name of the value for diagnostics.</param>
        /// <returns>The value of variable <paramref name="name"/> converted to <typeparamref name="T"/>.</returns>
        /// <exception cref="FrameworkConfigurationException">if <paramref name="value"/> cannot be converted.</exception>
        public static T ConvertToType<T>(object value, string name = null)
        {
            return (T)(ConvertToType(typeof(T), value, name) ?? default(T));
        }

        /// <summary>Converts a value to the specified type.</summary>
        /// <param name="targetType">Type of value returned.</param>
        /// <param name="value">A source value.</param>
        /// <param name="name">An optional name of the value for diagnostics.</param>
        /// <returns>The value of variable <paramref name="name"/> converted to <paramref name="targetType"/>.</returns>
        /// <exception cref="FrameworkConfigurationException">if <paramref name="value"/> cannot be converted.</exception>
        public static object ConvertToType([NotNull] Type targetType, [CanBeNull] object value, string name = null)
        {
            try
            {
                return ChangeType(value, targetType);
            }
            catch (InvalidCastException ex)
            {
                if (name.IsEmpty())
                    throw;
                throw new InvalidCastException("Cannot convert {0}: {1}".SafeFormat(name, ex.Message));
            }
            catch (FormatException ex)
            {
                if (name.IsEmpty())
                    throw;
                throw new InvalidCastException("Cannot convert {0}: {1}".SafeFormat(name, ex.Message));
            }
        }

        /// <summary>Returns an <see cref="object"/> with the specified <c>Type</c> whose value is equivalent to the
        /// specified object.</summary>
        /// <param name="value">An <see cref="object"/> to convert.</param>
        /// <param name="requiredType">The <c>Type</c> to which value is to be converted.</param>
        /// <returns><see langword="null"/> if <paramref name="value"/> is <see langword="null"/> and
        /// <paramref name="requiredType"/> is <see cref="Nullable{T}"/> or a reference type. Otherwise, an object whose
        /// <c>Type</c> is <paramref name="requiredType"/> and whose value is equivalent to <paramref name="value"/>.</returns>
        /// <remarks>Unlike <see cref="M:System.Convert.ChangeType"/> this method always uses
        /// <see cref="CultureInfo.InvariantCulture"/>.</remarks>
        [CanBeNull]
        public static object ChangeType([CanBeNull] object value, [NotNull] Type requiredType)
        {
            requiredType.MustNotBeNull(nameof(requiredType));

            if (requiredType == typeof(object))
                return value;

            // can return null for nullables and non-value types
            var underlyingRequiredType = Nullable.GetUnderlyingType(requiredType);

            var stringValue = value as string;
            if (value == null || stringValue != null && string.IsNullOrWhiteSpace(stringValue))
            {
                if (underlyingRequiredType != null || !requiredType.IsValueType)
                    return null;
                throw new InvalidCastException("Can not convert null or empty string to an {0} instance."
                    .SafeFormat(TypeNameUtils.GetReadableTypeName(requiredType)));
            }

            //if (stringValue != null && underlyingRequiredType == null && requiredType.IsValueType)
            //    throw new InvalidCastException("Can not convert empty string to an {0} instance."
            //        .SafeFormat(TypeNameUtils.GetReadableTypeName(requiredType)));

            // factualType is never Nullable because only underlying value is boxed, 
            // see http://msdn.microsoft.com/en-us/library/ms228597.aspx
            var factualType = value.GetType();

            // if already right type (except maybe nullability), do not convert
            if (requiredType.IsAssignableFrom(factualType))
                return value;

            requiredType = underlyingRequiredType ?? requiredType;

            // if already right type except for nullability - SHOULD NOT HAPPEN
            //if (factualType == requiredType)
            //    return value;

            // a string can be converted to anything
            if (stringValue != null)
                return FromString(requiredType, stringValue);

            // anything can be converted to string,
            if (requiredType == typeof(string))
                return ToString(value, true);

            // but very few other conversions are explicitly supported
            if (requiredType == typeof(bool) && factualType == typeof(int))
                return (int)value != 0;

            // Convert.ChangeType can cast Enum to int, but not the other way round
            if (requiredType.IsEnum && factualType == typeof(int))
                return Enum.ToObject(requiredType, (int)value);

            // try to find a TypeConverter
            var converter = TypeDescriptor.GetConverter(requiredType);
            if (converter.CanConvertTo(requiredType))
                return converter.ConvertTo(value, requiredType);

            // latch ditch fallback - .net handles conversions between numeric (and hopefully, date) types
            return Convert.ChangeType(value, requiredType, CultureInfo.InvariantCulture);
        }

        /// <summary>Replaces invalid XML characters in a <see cref="string"/> with their valid XML equivalent.</summary>
        /// <param name="value">The <see cref="string"/> within which to escape invalid characters.</param>
        /// <returns>The <paramref name="value"/> with invalid characters replaced.</returns>
        public static string EscapeXml(string value)
        {
            // TODO: find a better way to escape XML than SecurityElement.Escape; add EscapeJson.
            return SecurityElement.Escape(value);
        }

        #region nested classes

        /// <summary>The custom <see cref="DateTimeFormatInfo"/> provider cloned from <see cref="CultureInfo.InvariantCulture"/>
        /// that handles short data format correctly as "dd/MM/yyyy".</summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible",
            Justification = "Should be reusable in friendly assemblies, but Utils namespace is already too cluttered.")]
        public class InvariantDateTimeFormatProvider: IFormatProvider
        {
            private static InvariantDateTimeFormatProvider _instance;

            /// <summary>Gets the singleton instance of <see cref="InvariantDateTimeFormatProvider"/>.</summary>
            /// <value>The singleton instance of <see cref="InvariantDateTimeFormatProvider"/>.</value>
            public static InvariantDateTimeFormatProvider Instance =>
                _instance ?? (_instance = new InvariantDateTimeFormatProvider());

            /// <summary>Returns an object that provides formatting services for the specified type.</summary>
            /// <param name="formatType">An object that specifies the type of format object to return. </param>
            /// <returns>An <see cref="InvariantDateTimeFormatProvider"/> if the <paramref name="formatType"/> is
            /// <see cref="DateTimeFormatInfo"/>, otherwise the default <see cref="CultureInfo.InvariantCulture"/>.</returns>
            public object GetFormat(Type formatType)
            {
                if (formatType != typeof(DateTimeFormatInfo))
                    return CultureInfo.InvariantCulture;

                var dfi = (DateTimeFormatInfo)CultureInfo.InvariantCulture.DateTimeFormat.Clone();
                dfi.ShortDatePattern = "dd/MM/yyyy";
                return dfi;
            }
        }

        #endregion

        #region Nullable<T> (and some other types) .ToString() equivalents returning null instead of String.Empty

        /// <summary>Converts <b>Nullable&lt;bool&gt;</b> to string.</summary>
        /// <param name="value">A value to convert.</param>
        /// <returns>A canonical string representation of <paramref name="value"/> if it has a value or <see langword="null"/> if
        /// it doesn't.</returns>
        /// <seealso cref="Nullable&lt;T&gt;"/>
        public static string ToString(bool? value)
        {
            return value.HasValue ? (value.Value ? "true" : "false") : null;
        }

        /// <summary>Converts <b>Nullable&lt;int&gt;</b> to string.</summary>
        /// <param name="value">A value to convert.</param>
        /// <returns>A canonical string representation of <paramref name="value"/> if it has a value <see langword="null"/> if it
        /// doesn't.</returns>
        public static string ToString(int? value)
        {
            return value?.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts <b>Nullable&lt;uint&gt;</b> to string.</summary>
        /// <param name="value">A value to convert.</param>
        /// <returns>A canonical string representation of <paramref name="value"/> if it has a value or <see langword="null"/> if
        /// it doesn't.</returns>
        [CLSCompliant(false)]
        public static string ToString(uint? value)
        {
            return value?.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Converts <b>Nullable&lt;DateTime&gt;</b> to string.</summary>
        /// <param name="value">A value to convert.</param>
        /// <returns>A canonical string representation of <paramref name="value"/> if it has a value or <see langword="null"/> if
        /// it doesn't.</returns>
        public static string ToString(DateTime? value)
        {
            if (!value.HasValue)
                return null;

            var dt = value.Value;
            if (dt.Kind == DateTimeKind.Utc)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                return dt.ToString(DefaultDateTimeFormatUtc, CultureInfo.InvariantCulture);
                ////, CultureInfo.InvariantCulture, ConvertUtils.DefaultDateTimeStyleUtc);
            }

            dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            return dt.ToString(DefaultDateTimeFormatLocal, CultureInfo.InvariantCulture);
            ////, CultureInfo.InvariantCulture, ConvertUtils.DefaultDateTimeStyleLocal);
        }

        /// <summary>Converts <b>Nullable&lt;Double&gt;</b> to string.</summary>
        /// <param name="value">A value to convert.</param>
        /// <returns>A canonical string representation of <paramref name="value"/> if it has a value or <see langword="null"/> if
        /// it doesn't.</returns>
        public static string ToString(double? value)
        {
            return value?.ToString(NumberFormatInfo.InvariantInfo);
        }

        /// <summary>Converts <b>Nullable&lt;Decimal&gt;</b> to string.</summary>
        /// <param name="value">A value to convert.</param>
        /// <returns>A canonical string representation of <paramref name="value"/> if it has a value or <see langword="null"/> if
        /// it doesn't.</returns>
        public static string ToString(decimal? value)
        {
            return value?.ToString(NumberFormatInfo.InvariantInfo);
        }

        /// <summary>Converts <b>Nullable&lt;Guid&gt;</b> to string.</summary>
        /// <param name="value">A value to convert.</param>
        /// <returns>A canonical string representation of <paramref name="value"/> if it has a value or <see langword="null"/> if
        /// it doesn't.</returns>
        public static string ToString(Guid? value)
        {
            return value?.ToString();
        }

        /// <summary>Converts <b>byte[]</b> to string.</summary>
        /// <param name="value">A value to convert.</param>
        /// <returns>A canonical string representation (base64) of <paramref name="value"/> if it has a value or
        /// <see langword="null"/> if it doesn't.</returns>
        public static string ToString(byte[] value)
        {
            if (value == null)
                return null;
            return value.Length == 0
                ? string.Empty
                : Convert.ToBase64String(value);
        }

        /// <summary>Converts an object to string.</summary>
        /// <param name="value">An <see langword="object"/> to convert.</param>
        /// <param name="strict"><see langword="true"/> indicates that lossless conversion is required, <see langword="false"/>
        /// (the default) allows to use whatever string representation is available.</param>
        /// <returns>A canonical representation of <paramref name="value"/>, depending on its factual <c>Type</c>.</returns>
        /// <remarks>This method handles well-known value and <see cref="Nullable{T}"/> types via corresponding <b>ToString()</b>
        /// methods of <see cref="ConvertUtils"/>. Byte aray (<b>byte[]</b>) is converted to base64 string and all other types are
        /// converted via <see cref="TypeConverter"/> when possible.</remarks>
        public static string ToString(object value, bool strict = false)
        {
            if (value == null)
                return null;
            var s = value as string;
            if (s != null)
                return s;

            var type = value.GetType();
            if (type.IsValueType)
            {
                var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
                if (underlyingType == typeof(bool))
                    return ToString((bool)value);
                if (underlyingType == typeof(int))
                    return ToString((int)value);
                if (underlyingType == typeof(uint))
                    return ToString((uint)value);
                if (underlyingType == typeof(DateTime))
                    return ToString((DateTime)value);
                if (underlyingType == typeof(double))
                    return ToString((double)value);
                if (underlyingType == typeof(decimal))
                    return ToString((decimal)value);
                if (underlyingType == typeof(Guid))
                    return ToString((Guid)value);
            }

            if (type == typeof(byte[]))
                return ToString((byte[])value);

            var converter = TypeDescriptor.GetConverter(type);
            if (converter.GetType() != typeof(TypeConverter) || !strict)
                if (converter.CanConvertTo(typeof(string)))
                    return converter.ConvertToInvariantString(value);

            if (strict)
                throw new InvalidCastException(
                    "Cannot convert an instance of {0} to a string."
                        .SafeFormat(TypeNameUtils.GetReadableTypeName(type)));

            return value.ToString().TrimToNull();
        }

        #endregion

        #region Nullable<T>.Parse() equivalents returning null for null or empty string

        /// <summary>Converts a string to <b>Nullable&lt;bool&gt;</b>.</summary>
        /// <param name="value">A string containing a boolean to convert, may be <see langword="null"/>.</param>
        /// <returns>The <b>Nullable&lt;bool&gt;</b>, represented by <paramref name="value"/>. If <paramref name="value"/> is
        /// <see langword="null"/>, returned value represents <see langword="null"/>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> doesn't contain a valid string representation of a boolean.</exception>
        /// <remarks>
        /// <para>The valid representation of <see langword="true"/> are case-insensitive "<b>true</b>", "<b>yes</b>", " <b>1</b>"
        /// and the representation of <see langword="true"/>, defined by the current thread culture.</para>
        /// <para>The valid representation of <see langword="false"/> are case-insensitive "<b>false</b>", "<b>no</b>", " <b>0</b>"
        /// and the representation of <see langword="false"/>, defined by the current thread culture.</para>
        /// </remarks>
        public static bool? ToBoolean(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Trim().ToUpperInvariant();

            if (string.IsNullOrEmpty(value) || value == "FALSE" || value == "NO" || value == "0")
                return false;

            if (value == "TRUE" || value == "YES" || value == "1")
                return true;

            // probably FormatException will be thrown
            return Convert.ToBoolean(value, CultureInfo.CurrentCulture);
        }

        /// <summary>Converts a string to <b>Nullable&lt;int&gt;</b>.</summary>
        /// <param name="value">A string containing an integer to convert, may be <see langword="null"/>.</param>
        /// <returns>If <paramref name="value"/> is <see langword="null"/>, returned value represents <see langword="null"/>. The
        /// <b>Nullable{int}</b>, represented by <paramref name="value"/>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> doesn't contain a valid string representation of an integer.</exception>
        public static int? ToInt32(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        /// <summary>Converts a string to <b>Nullable&lt;uint&gt;</b>.</summary>
        /// <param name="value">A string containing an unsigned integer to convert, may be <see langword="null"/>.</param>
        /// <returns>The <b>Nullable&lt;uint&gt;</b>, represented by <paramref name="value"/>. If <paramref name="value"/> is
        /// <see langword="null"/>, returned value represents <see langword="null"/>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/>doesn't contain a valid string representation of an unsigned
        /// integer.</exception>
        [CLSCompliant(false)]
        public static uint? ToUInt32(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return uint.Parse(value, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                CultureInfo.InvariantCulture);
        }

        /// <summary>Converts a string to a value of the most appropriate type.</summary>
        /// <param name="value">A string to convert.</param>
        /// <returns>An int, bool, Guid or DataTime if <paramref name="value"/> is convertable to any of them, otherwise the
        /// original string <paramref name="value"/>.</returns>
        public static object FromString([CanBeNull] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
                return i;
            try
            {
                return ToBoolean(value);
            }
            catch (FormatException)
            {
            }

            if (value.Length >= 10)
                try
                {
                    return ToDateTime(value);
                }
                catch (FormatException)
                {
                }

            if (value.Length >= 32)
                try
                {
                    return ToGuid(value);
                }
                catch (FormatException)
                {
                }

            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                return f;

            return value;
        }

        /// <summary>Converts a string to <b>Nullable&lt;DateTime&gt;</b>.</summary>
        /// <param name="value">A string containing date and time to convert, may be <see langword="null"/>. If timezone or Utc
        /// flag is not present, it is assumed to be a local time.</param>
        /// <returns>The <b>Nullable&lt;DateTime&gt;</b>, represented by <paramref name="value"/>. If <paramref name="value"/> is
        /// <see langword="null"/>, returned value represents <see langword="null"/>. If <paramref name="value"/> specifies a UTC
        /// time (ends with 'Z') or has an explicit timezone, returned value is UTC, otherwise it is local time.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> doesn't contain a valid string representation of a date and
        /// time.</exception>
        /// <remarks>The conversion uses <see cref="DefaultDateTimeStyleLocal"/>.</remarks>
        public static DateTime? ToDateTime(string value)
        {
            value = value.TrimToNull();
            if (value == null)
                return null;

            var dt = DateTime.Parse(value, InvariantDateTimeFormatProvider.Instance, DefaultDateTimeStyleLocal);
            if (value.Length > 10)
            {
                var tail = value.Trim().Substring(10);
                if (tail.EndsWith("Z", StringComparison.OrdinalIgnoreCase)
                    || tail.Contains("+") || tail.Contains("-"))
                    dt = dt.ToUniversalTime();
            }

            return dt;
        }

        /// <summary>Converts a string to <b>Nullable&lt;DateTime&gt;</b> assuming UTC date and time.</summary>
        /// <param name="value">A string containing date and time to convert, may be <see langword="null"/>. If timezone or Utc
        /// flag is not present, it is assumed to be a Utc time.</param>
        /// <returns>The UTC <b>Nullable&lt;DateTime&gt;</b>, represented by <paramref name="value"/>. If <paramref name="value"/>
        /// is <see langword="null"/>, returned value represents <see langword="null"/>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> doesn't contain a valid string representation of a date and
        /// time.</exception>
        /// <remarks>The conversion uses <see cref="DefaultDateTimeStyleLocal"/>. Afterwards the value is converted to Utc.</remarks>
        public static DateTime? ToUtcDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return
                DateTime.Parse(value, InvariantDateTimeFormatProvider.Instance, DefaultDateTimeStyleUtc)
                    .ToUniversalTime();
        }

        /// <summary>Converts a string to <b>Nullable&lt;DateTime&gt;</b> assuming UTC date and time.</summary>
        /// <param name="value">A string containing date and time to convert, may be <see langword="null"/>. If timezone or Utc
        /// flag is not present, it is assumed to be a local time.</param>
        /// <returns>The local <b>Nullable&lt;DateTime&gt;</b>, represented by <paramref name="value"/>. If
        /// <paramref name="value"/> is <see langword="null"/>, returned value represents <see langword="null"/>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> doesn't contain a valid string representation of a date and
        /// time.</exception>
        /// <remarks>The conversion uses <see cref="DefaultDateTimeStyleLocal"/>. Afterwards the value is converted to local time.</remarks>
        public static DateTime? ToLocalDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return
                DateTime.Parse(value, InvariantDateTimeFormatProvider.Instance, DefaultDateTimeStyleLocal)
                    .ToLocalTime();
        }

        /// <summary>Converts a string to <b>Nullable&lt;double&gt;</b>.</summary>
        /// <param name="value">A string containing double to convert, may be <see langword="null"/>.</param>
        /// <returns>The <b>Nullabl&lt;ouble&gt;</b>, represented by <paramref name="value"/>. If <paramref name="value"/> is
        /// <see langword="null"/>, returned value represents <see langword="null"/>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> doesn't contain a valid string representation of a double.</exception>
        public static double? ToDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        /// <summary>Converts a string to <b>Nullable&lt;decimal&gt;</b>.</summary>
        /// <param name="value">A string containing decimal to convert, may be <see langword="null"/>containing Guid .</param>
        /// <returns>The <b>Nullable&lt;decimal&gt;</b>, represented by <paramref name="value"/>. If <paramref name="value"/> is
        /// <see langword="null"/>, returned value represents <see langword="null"/>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> doesn't contain a valid string representation of a decimal.</exception>
        public static decimal? ToDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return decimal.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        /// <summary>Converts a string to <b>Nullable&lt;Guid&gt;</b>.</summary>
        /// <param name="value">A string containing Guid to convert, may be <see langword="null"/>.</param>
        /// <returns>The <b>Nullable&lt;Guid&gt;</b>, represented by <paramref name="value"/>. If <paramref name="value"/> is
        /// <see langword="null"/>, returned value represents <see langword="null"/>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> doesn't contain a valid string representation of a
        /// <see cref="Guid"/>.</exception>
        public static Guid? ToGuid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return new Guid(value.Trim());
        }

        /// <summary>Converts a string to byte array.</summary>
        /// <param name="value">A string containing a Base64 representation of a byte array, may be <see langword="null"/>.</param>
        /// <returns>The byte array, represented by <paramref name="value"/>. If <paramref name="value"/> is <see langword="null"/>
        /// , returned value has zero length.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> doesn't contain a valid Base64 string.</exception>
        public static byte[] ToBytes(string value)
        {
            if (value == null)
                return null;

            if (string.IsNullOrWhiteSpace(value))
                return new byte[0];

            return Convert.FromBase64String(value);
        }

        /// <summary>Converts a string to an object of the specified type.</summary>
        /// <param name="requiredType">The desired type.</param>
        /// <param name="value">The <see cref="string"/> containing a representation of the <paramref name="requiredType"/>to
        /// convert, may be <see langword="null"/>.</param>
        /// <returns>The value of type <paramref name="requiredType"/>, represented by <paramref name="value"/>.
        /// <see langword="null"/> if <paramref name="value"/> is <see langword="null"/>.</returns>
        /// <exception cref="FormatException"><paramref name="value"/> doesn't contain a valid string representation of the
        /// <paramref name="requiredType"/>.</exception>
        /// <remarks>If the <paramref name="requiredType"/> is a (possibly nullable) value type or a byte array, this method uses
        /// the corresponding method of <see cref="ConvertUtils"/>; otherwise it tries to find and use the appropriate
        /// <see cref="TypeConverter"/>.</remarks>
        public static object FromString(Type requiredType, string value)
        {
            requiredType.MustNotBeNull(nameof(requiredType));
            if (requiredType == typeof(string) || requiredType == typeof(object))
                return value;

            if (requiredType.IsValueType)
            {
                var underlyingRequiredType = Nullable.GetUnderlyingType(requiredType) ?? requiredType;

                if (requiredType == underlyingRequiredType && string.IsNullOrWhiteSpace(value))
                    throw new InvalidCastException("Can not convert null or empty string to a value type {0}."
                        .SafeFormat(TypeNameUtils.GetReadableTypeName(requiredType)));

                // we know that underlyingType is not a string
                if (underlyingRequiredType == typeof(bool))
                    return ToBoolean(value);
                if (underlyingRequiredType == typeof(int))
                    return ToInt32(value);
                if (underlyingRequiredType == typeof(uint))
                    return ToUInt32(value);
                if (underlyingRequiredType == typeof(Guid))
                    return ToGuid(value);
                if (underlyingRequiredType == typeof(DateTime))
                    return ToDateTime(value);
                if (underlyingRequiredType == typeof(decimal))
                    return ToDecimal(value);
                if (underlyingRequiredType == typeof(double))
                    return ToDouble(value);
                if (underlyingRequiredType.IsEnum)
                    // ReSharper disable once AssignNullToNotNullAttribute
                    return EnumUtils.GetValue(underlyingRequiredType, value);
            }

            if (requiredType == typeof(byte[]))
                return ToBytes(value);
            var converter = TypeDescriptor.GetConverter(requiredType);
            if (converter.CanConvertFrom(typeof(string)))
                return converter.ConvertFromInvariantString(value);
            throw new InvalidCastException(
                "Cannot convert a string starting with '{0}' to {1}."
                    .SafeFormat(value.Ellipsis(32), requiredType.FullName));
        }

        #endregion
    }
}
