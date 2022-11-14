using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace Refactorius.Data
{
    /// <summary>The collection of useful enumeration-related utility methods.</summary>
    public static class EnumUtils
    {
        private static readonly Dictionary<string, object> _enumerationMap =
            new Dictionary<string, object>(128);

        /// <summary>Ensures that a <c>Type</c> is an enumeration.</summary>
        /// <param name="type">A <c>Type</c> to check.</param>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not an enumeration.</exception>
        public static void EnsureEnumeration([ValidatedNotNull] Type type)
        {
            // can't use generic type constraints on value types, so check at runtime
            type.MustNotBeNull(nameof(type));

            if (type.BaseType != typeof(Enum))
                throw new ArgumentException("Type " + type.FullName + " must be inherited from System.Enum",
                    nameof(type));
        }

        /// <summary>Gets an array of all enumeration members.</summary>
        /// <typeparam name="T">A <see cref="Enum"/> type.</typeparam>
        /// <returns>The array of all enumeration members.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an enumeration.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "method",
            Justification =
                "Static method uses a generic parameter instead of a System.Type parameter for type safety and code readability.")]
        public static T[] GetValues<T>() where T : struct
        {
            EnsureEnumeration(typeof(T));
            return (T[]) Enum.GetValues(typeof(T));
        }

        /// <summary>Gets the value of <see cref="System.ComponentModel.DescriptionAttribute"/> for the specified enumeration
        /// value.</summary>
        /// <param name="value">A enumeration value.</param>
        /// <returns>The <see cref="String"/> value of the <see cref="System.ComponentModel.DescriptionAttribute"/> for the
        /// specified value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static string? GetDescription(Enum value)
        {
            value.MustNotBeNull(nameof(value));

            // get member name (NB: value may be not an enumeration member)
            var name = Enum.GetName(value.GetType(), value);
            if (name == null)
                return null;

            var description = TypeUtils.GetDescription(
                value.GetType(),
                name,
                BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public);

            return description ?? value.ToString();
        }

        /// <summary>Gets the value of <see cref="System.ComponentModel.DescriptionAttribute"/> for the specified enumeration
        /// member.</summary>
        /// <param name="type">The enumeration <c>Type</c>.</param>
        /// <param name="name">The member name.</param>
        /// <returns>The <see cref="String"/> value of the <see cref="System.ComponentModel.DescriptionAttribute"/> for the member
        /// <paramref name="name"/> of type <paramref name="type"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not an enumeration.</exception>
        public static string? GetDescription(Type type, string name)
        {
            EnsureEnumeration(type);
            try
            {
                return TypeUtils.GetDescription(
                           type,
                           name,
                           BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)
                       ?? Enum.Parse(type, name).ToString();
            }
            catch (InvalidOperationException)
            {
                // unknown name returns null 
                return null;
            }
        }

        /// <summary>Gets the value of <see cref="System.ComponentModel.DescriptionAttribute"/> for the specified enumeration
        /// member.</summary>
        /// <typeparam name="T">The enumeration <c>Type</c>.</typeparam>
        /// <param name="name">The member name.</param>
        /// <returns>The <see cref="String"/> value of the <see cref="System.ComponentModel.DescriptionAttribute"/> for the
        /// specified member name.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an enumeration.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "method",
            Justification =
                "Static method uses a generic parameter instead of a System.Type parameter for type safety and code readability.")]
        public static string? GetDescription<T>(string name) where T : struct
        {
            return GetDescription(typeof(T), name);
        }

        /// <summary>Converts the specified string representation of an enumeration member to its value.</summary>
        /// <param name="type">The enumeration <c>Type</c>.</param>
        /// <param name="description">The <see cref="System.ComponentModel.DescriptionAttribute"/> or member name to convert.</param>
        /// <param name="value">On success, the enumeration value, corresponding to the <paramref name="description"/>; on failure,
        /// <see langword="null"/>.</param>
        /// <returns><see langword="true"/>, if <paramref name="description"/> was succesfully converted; otherwise,
        /// <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not an enumeration.</exception>
        public static bool TryGetValue(Type type, string description, out Enum value)
        {
            EnsureEnumeration(type);

            value = default(Enum);
            if (description.IsEmpty())
                return false;

            var key = type.FullName + "$" + description;
            lock (_enumerationMap)
            {
                if (_enumerationMap.ContainsKey(key))
                {
                    value = (Enum) _enumerationMap[key];
                    return value != null;
                }
            }

            object newValue = null;
            foreach (var name in Enum.GetNames(type))
            {
                if (name.Equals(description, StringComparison.OrdinalIgnoreCase))
                {
                    newValue = Enum.Parse(type, name);
                    break;
                }

                var nameDescription = GetDescription(type, name);
                if (nameDescription != null
                    && nameDescription.Equals(description, StringComparison.OrdinalIgnoreCase))
                {
                    newValue = Enum.Parse(type, name);
                    break;
                }
            }

            lock (_enumerationMap)
            {
                if (!_enumerationMap.ContainsKey(key))
                    _enumerationMap.Add(key, newValue);
            }

            if (newValue != null)
                value = (Enum) newValue;

            return newValue != null;
        }

        /// <summary>Converts the specified string representation of an enumeration member to its value.</summary>
        /// <typeparam name="T">The enumeration <c>Type</c>.</typeparam>
        /// <param name="description">The <see cref="System.ComponentModel.DescriptionAttribute"/> or member name to convert.</param>
        /// <param name="value">On success, the enumeration value, corresponding to the <paramref name="description"/>; on failure,
        /// the default enumeration value (integer 0).</param>
        /// <returns><see langword="true"/>, if <paramref name="description"/> was succesfully converted; otherwise,
        /// <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">if <typeparamref name="T"/> is <see langword="null"/>.</exception>
        public static bool TryGetValue<T>(string description, out T value) where T : struct
        {
            Enum intValue;
            var result = TryGetValue(typeof(T), description, out intValue);
            if (result)
                value = (T) (object) intValue;
            else
                value = default(T);
            return result;
        }

        /// <summary>Converts the specified string representation of an enumeration member to its value.</summary>
        /// <param name="type">The enumeration <c>Type</c>.</param>
        /// <param name="description">The <see cref="System.ComponentModel.DescriptionAttribute"/> or member name to convert.</param>
        /// <returns>The enumeration value, corresponding to the <paramref name="description"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not an enumeration.</exception>
        /// <exception cref="FormatException"><paramref name="description"/> is null, empty or contains only whitespaces;
        /// Enumeration <paramref name="type"/> doesn't contain a member with a name or description equal to
        /// <paramref name="description"/>.</exception>
        public static Enum GetValue(Type type, string description)
        {
            type.MustNotBeNull(nameof(type));
            description.MustNotBeEmpty(nameof(description));

            Enum value;
            if (!TryGetValue(type, description, out value))
                throw new FormatException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Enumeration '{0}' doesn't contain a member with a name or description '{1}'.",
                        type.FullName,
                        description));

            return value;
        }

        /// <summary>Converts the specified string representation of an enumeration member to its value.</summary>
        /// <typeparam name="T">The enumeration <c>Type</c>.</typeparam>
        /// <param name="description">The <see cref="System.ComponentModel.DescriptionAttribute"/> or member name to convert.</param>
        /// <returns>The enumeration value, corresponding to the <paramref name="description"/>.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an enumeration.</exception>
        /// <exception cref="FormatException"><paramref name="description"/> is null, empty or contains only whitespaces;
        /// Enumeration <typeparamref name="T"/> doesn't contain a member with a name or description equal to
        /// <paramref name="description"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "method",
            Justification =
                "Static method uses a generic parameter instead of a System.Type parameter for type safety and code readability.")]
        public static T GetValue<T>(string description) where T : struct
        {
            return (T) (object) GetValue(typeof(T), description);
        }

        /// <summary>Converts the specified enumeration value to a string, according to the value of <see cref="XmlEnumAttribute"/>
        /// .</summary>
        /// <param name="value">The enumeration value.</param>
        /// <returns>A <see cref="String"/> specified by <see cref="XmlEnumAttribute"/> for this value.</returns>
        public static string ToXmlString(Enum value)
        {
            value.MustNotBeNull(nameof(value));

            var fi = value.GetType().GetField(value.ToString());
            if (fi == null)
                return value.ToString();

            var attributes = (XmlEnumAttribute[]) fi.GetCustomAttributes(typeof(XmlEnumAttribute), false);

            return attributes.Length > 0
                // ReSharper disable once PossibleNullReferenceException
                ? attributes[0].Name
                : value.ToString();
        }

        /// <summary>Converts the specified XML string representation of an enumeration member to its value.</summary>
        /// <param name="type">The enumeration <c>Type</c>.</param>
        /// <param name="description">The <see cref="XmlEnumAttribute"/> or member name to convert.</param>
        /// <param name="value">On success, the enumeration value, corresponding to the <paramref name="description"/>; on failure,
        /// the default enumeration value (integer 0).</param>
        /// <returns><see langword="true"/>, if <paramref name="description"/> was succesfully converted; otherwise,
        /// <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not an enumeration.</exception>
        public static bool TryGetValueFromXmlString(Type type, string description, out Enum value)
        {
            EnsureEnumeration(type);

            foreach (Enum enumValue in Enum.GetValues(type))
                if (ToXmlString(enumValue).Equals(description, StringComparison.OrdinalIgnoreCase))
                {
                    value = enumValue;
                    return true;
                }

            value = default(Enum);
            return false;
        }

        /// <summary>Converts the specified XML string representation of an enumeration member to its value.</summary>
        /// <typeparam name="T">The enumeration <c>Type</c>.</typeparam>
        /// <param name="description">The <see cref="XmlEnumAttribute"/> or member name to convert.</param>
        /// <param name="value">On success, the enumeration value, corresponding to the <paramref name="description"/>; on failure,
        /// the default enumeration value (integer 0).</param>
        /// <returns><see langword="true"/>, if <paramref name="description"/> was succesfully converted; otherwise,
        /// <see langword="false"/>.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an enumeration.</exception>
        public static bool TryGetValueFromXmlString<T>(string description, out T value) where T : struct
        {
            Enum enumValue;
            var result = TryGetValueFromXmlString(typeof(T), description, out enumValue);
            if (result)
                value = (T) (object) enumValue;
            else
                value = default(T);
            return result;
        }

        /// <summary>Converts the specified XML string representation of an enumeration member to its value.</summary>
        /// <param name="type">The enumeration <c>Type</c>.</param>
        /// <param name="description">The <see cref="XmlEnumAttribute"/> or member name to convert.</param>
        /// <returns>The enumeration value, corresponding to the <paramref name="description"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not an enumeration.</exception>
        public static Enum GetValueFromXmlString(Type type, string description)
        {
            type.MustNotBeNull(nameof(type));

            Enum value;
            if (!TryGetValueFromXmlString(type, description, out value))
                throw new FormatException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Enumeration '{0}' doesn't contain a member with a XmlEnumAttribute '{1}'.",
                        type.FullName,
                        description));

            return value;
        }

        /// <summary>Converts the specified XML string representation of an enumeration member to its value.</summary>
        /// <typeparam name="T">The enumeration <c>Type</c>.</typeparam>
        /// <param name="description">The <see cref="XmlEnumAttribute"/> or member name to convert.</param>
        /// <returns>The enumeration value, corresponding to the <paramref name="description"/>.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an enumeration.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "method",
            Justification =
                "Static method uses a generic parameter instead of a System.Type parameter for type safety and code readability.")]
        public static T GetValueFromXmlString<T>(string description) where T : struct
        {
            return (T) (object) GetValueFromXmlString(typeof(T), description);
        }

        /// <summary>Reparses an enumeration value into a value of another enumeration with the same set of member names.</summary>
        /// <param name="type">The target enumeraten <c>Type</c>.</param>
        /// <param name="value">The original (source) enumeration value.</param>
        /// <returns>The value of type <paramref name="type"/> having the same (combination of) field name(s) as
        /// <paramref name="value"/>, even if integer values of source and target enumeration members differ.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> or <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not an enumeration.</exception>
        /// <exception cref="FormatException"><paramref name="value"/> is specified as a numeric literal or cannot be interpreted
        /// as a (combination of) field name(s) of <paramref name="type"/>.</exception>
        public static Enum Reparse(Type type, Enum value)
        {
            EnsureEnumeration(type);
            value.MustNotBeNull(nameof(value));

            // default value 0 is always mapped to 0
            // 
            ////if (value == default(Enum))
            ////    return value;

            var s = value.ToString();
            int n;
            if (int.TryParse(s, out n))
                throw new FormatException(
                    string.Format(CultureInfo.InvariantCulture,
                        "Value '{0}' is numeric; only enumeration field names are allowed.", s));

            try
            {
                return (Enum) Enum.Parse(type, s, true);
            }
            catch (ArgumentException ex)
            {
                throw new FormatException(ex.Message);
            }
        }

        /// <summary>Reparses an enumeration value into a value of another enumeration with the same set of member names.</summary>
        /// <typeparam name="T">The target enumeraten <c>Type</c>.</typeparam>
        /// <param name="value">The original (source) enumeration value.</param>
        /// <returns>The value of type <typeparamref name="T"/> having the same (combination of) field name(s) as
        /// <paramref name="value"/>, even if integer values of source and target enumeration members differ.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is not an enumeration.</exception>
        /// <exception cref="FormatException"><paramref name="value"/> is specified as a numeric literal or cannot be interpreted
        /// as a (combination of) field name(s) of <typeparamref name="T"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "method",
            Justification =
                "Static method uses a generic parameter instead of a System.Type parameter for type safety and code readability.")]
        public static T Reparse<T>(Enum value) where T : struct
        {
            value.MustNotBeNull(nameof(value));

            return (T) (object) Reparse(typeof(T), value);
        }
    }
}