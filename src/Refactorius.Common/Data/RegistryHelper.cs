//#if NETFULL
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace Refactorius.Data
{
    /// <summary>The utility class encapsulating a <see cref="RegistryKey"/>.</summary>
    public class RegistryHelper
    {
        #region constructor

        /// <summary>Initializes a new instance of the <see cref="RegistryHelper"/> class.</summary>
        /// <param name="root">An <see cref="RegistryKey"/> that is represented by this instance <see cref="RegistryHelper"/>.</param>
        public RegistryHelper([NotNull] RegistryKey root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            Root = root;
        }

        #endregion

        #region private fields

        #endregion

        #region public properties

        /// <summary>Gets or sets the <see cref="RegistryKey"/> wrapped by the current <see cref="RegistryHelper"/>.</summary>
        /// <value>The <see cref="RegistryKey"/> wrapped by the current <see cref="RegistryHelper"/>.</value>
        public RegistryKey Root { get; set; }

        /// <summary>Gets a value indicating whether the <see cref="RegistryKey"/>, specified upon creation, exists.</summary>
        /// <value><see langword="true"/> if the <see cref="RegistryKey"/>, specified upon creation, exists (is not
        /// <see langword="null"/>), <see langword="false"/> otherwise.</value>
        public bool Exists => Root != null;

        /// <summary>Gets the last part of the path for a wrapped <see cref="RegistryKey"/>.</summary>
        /// <value>The last part of the path for a wrapped <see cref="RegistryKey"/>, i.e. the [sub] key name.</value>
        public string Name
        {
            get
            {
                var num = Path.LastIndexOf('\\');
                if (num != -1)
                    return Path.Substring(num + 1);

                return Path;
            }
        }

        /// <summary>Gets the full path for a wrapped <see cref="RegistryKey"/>.</summary>
        /// <value>The full path for a wrapped <see cref="RegistryKey"/>.</value>
        public string Path => Root.Name;

        #endregion

        #region public methods

        /// <summary>Creates a copy of the current <see cref="RegistryKey"/>.</summary>
        /// <returns>A new instance of a <see cref="RegistryKey"/> class taht is a copy of the current one.</returns>
        public RegistryHelper Clone()
        {
            return new RegistryHelper(Root);
        }

        /// <summary>Reads from the <see cref="RegistryKey"/> a <see cref="bool"/> value with the specified name.</summary>
        /// <param name="name">A <see cref="string"/> name of a registry <b>value</b>.</param>
        /// <param name="defaultValue">A default value, returned if the <paramref name="name"/> wasn't found or is unreadable.</param>
        /// <returns>The <see cref="bool"/> value read from the registry or the <paramref name="defaultValue"/>.</returns>
        public bool GetBoolean(string name, bool defaultValue)
        {
            try
            {
                return Root.GetValue(name, defaultValue ? 1 : 0).ToString() != "0";
            }
            catch (SecurityException)
            {
                return defaultValue;
            }
            catch (UnauthorizedAccessException)
            {
                return defaultValue;
            }
        }

        /// <summary>Reads from the <see cref="RegistryKey"/> a <see cref="Guid"/> value with the specified name.</summary>
        /// <param name="name">A <see cref="string"/> name of a registry <b>value</b>.</param>
        /// <returns>The <see cref="Guid"/> value read from the registry or <see langword="null"/> if the <paramref name="name"/>
        /// wasn't found or is unreadable.</returns>
        public Guid? GetGuid(string name)
        {
            try
            {
                var value = GetString(name);
                if (string.IsNullOrEmpty(value))
                    return null;

                return new Guid(value);
            }
            catch (FormatException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }
        }

        /// <summary>Reads from the <see cref="RegistryKey"/> a <see cref="int"/> value with the specified name.</summary>
        /// <param name="name">A <see cref="string"/> name of a registry <b>value</b>.</param>
        /// <returns>The <see cref="Int32"/> value read from the registry or <see langword="null"/> if the <paramref name="name"/>
        /// wasn't found or is unreadable.</returns>
        public int? GetInteger(string name)
        {
            object value;
            try
            {
                value = Root.GetValue(name, null);
            }
            catch (FormatException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }

            if (value == null)
                return null;

            if (value is int)
                return (int) value;

            try
            {
                return int.Parse(value.ToString(), CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>Reads from the <see cref="RegistryKey"/> a <see cref="int"/> value with the specified name.</summary>
        /// <param name="name">A <see cref="string"/> name of a registry <b>value</b>.</param>
        /// <param name="defaultValue">A default value, returned if the <paramref name="name"/> wasn't found or is unreadable.</param>
        /// <returns>The <see cref="int"/> value read from the registry or the <paramref name="defaultValue"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "integer",
            Justification = "Method name must explicitly identify returned type.")]
        public int GetInteger(string name, int defaultValue)
        {
            return GetInteger(name) ?? defaultValue;
        }

        /// <summary>Reads from the <see cref="RegistryKey"/> a <see cref="string"/> value with the specified name.</summary>
        /// <param name="name">A <see cref="string"/> name of a registry <b>value</b>.</param>
        /// <returns>The <see cref="string"/> value read from the registry or the empty string if the <paramref name="name"/>
        /// wasn't found or is unreadable.</returns>
        public string GetString(string name)
        {
            try
            {
                return Root.GetValue(name, string.Empty).ToString();
            }
            catch (SecurityException)
            {
                return string.Empty;
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }
        }

        /// <summary>Reads from the <see cref="RegistryKey"/> a <see cref="string"/> value with the specified name.</summary>
        /// <param name="name">A <see cref="string"/> name of a registry <b>value</b>.</param>
        /// <param name="defaultValue">A default value, returned if the <paramref name="name"/> wasn't found or is unreadable.</param>
        /// <returns>The <see cref="string"/> value read from the registry or the empty string if the <paramref name="name"/>
        /// wasn't found or is unreadable.</returns>
        public string GetString(string name, string defaultValue)
        {
            var value = GetString(name);

            return string.IsNullOrEmpty(value)
                ? defaultValue
                : value;
        }

        /// <summary>Reads from the <see cref="RegistryKey"/> an array of <see cref="string"/> values for the specified name.</summary>
        /// <param name="name">A <see cref="string"/> name of a registry <b>value</b>.</param>
        /// <returns>The <see cref="string"/> array read from the registry or <see langword="null"/> if the <paramref name="name"/>
        /// wasn't found or is unreadable.</returns>
        public string[] GetStringArray(string name)
        {
            try
            {
                var value = Root.GetValue(name);
                if (value == null)
                    return null;

                var stringValues = value as string[];
                if (stringValues != null)
                    return stringValues;

                return new[] {value.ToString()};
            }
            catch (SecurityException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        /// <summary>Navigates to the specified subkey of the current registry key.</summary>
        /// <param name="subkeyName">A subkey name.</param>
        /// <param name="writable"><see langword="true"/> to open the <paramref name="subkeyName"/> for read/write,
        /// <see langword="true"/> to open it for read only.</param>
        /// <returns>A <see cref="RegistryHelper"/> for the <paramref name="subkeyName"/> of the current
        /// <see cref="RegistryHelper"/>.</returns>
        public bool NavigateToSubkey(string subkeyName, bool writable)
        {
            Root = !writable
                ? Root.OpenSubKey(subkeyName)
                : Root.CreateSubKey(subkeyName);

            return Exists;
        }

        /// <summary>Creates a clone of itself and navigates to the specified subkey of the current registry key.</summary>
        /// <param name="subkeyName">A subkey name.</param>
        /// <param name="writable"><see langword="true"/> to open the <paramref name="subkeyName"/> for read/write,
        /// <see langword="true"/> to open it for read only.</param>
        /// <returns>A newly created <see cref="RegistryHelper"/> for the <paramref name="subkeyName"/> of the current
        /// <see cref="RegistryHelper"/>.</returns>
        public RegistryHelper CreateSubkey(string subkeyName, bool writable)
        {
            var result = Clone();
            if (result.NavigateToSubkey(subkeyName, writable))
                return result;
            return null;
        }

        /// <summary>Returns a string array of subkey names.</summary>
        /// <returns>An array of subkey names, existing in the current <see cref="RegistryHelper"/>.</returns>
        public string[] GetSubkeyNames()
        {
            return Root.GetSubKeyNames();
        }

        /// <summary>Stores in the current registry key a <see cref="bool"/> value with the specified name.</summary>
        /// <param name="name">A name for the registry value entry.</param>
        /// <param name="value">A value to store under this name.</param>
        public void SetBool(string name, bool value)
        {
            Root.SetValue(name, value ? 1 : 0);
        }

        /// <summary>Stores in the current registry key a <see cref="Guid"/> value with the specified name.</summary>
        /// <param name="name">A name for the registry value entry.</param>
        /// <param name="value">A <see cref="Guid"/> value to store under this name or <see langword="null"/>.</param>
        /// <remarks>If <paramref name="value"/> is <see langword="null"/> an empty string is stored.</remarks>
        public void SetGuid(string name, Guid? value)
        {
            Root.SetValue(name, !value.HasValue ? string.Empty : value.ToString());
        }

        /// <summary>Stores in the current registry key an <see cref="int"/> value with the specified name.</summary>
        /// <param name="name">A name for the registry value entry.</param>
        /// <param name="value">A value to store under this name.</param>
        public void SetInt(string name, int? value)
        {
            Root.SetValue(name, value ?? 0);
        }

        /// <summary>Stores in the current registry key a <see cref="string"/> value with the specified name.</summary>
        /// <param name="name">A name for the registry value entry.</param>
        /// <param name="value">A string value to store under this name or <see langword="null"/>.</param>
        public void SetString(string name, string value)
        {
            Root.SetValue(name, value ?? string.Empty);
        }

        /// <summary>Stores in the current registry key a <see cref="string"/> array value with the specified name.</summary>
        /// <param name="name">A name for the registry value entry.</param>
        /// <param name="value">A string array value to store under this name or <see langword="null"/>.</param>
        /// <remarks>If <paramref name="value"/> is <see langword="null"/> or has zero length, all values are removed from the
        /// current registry key.</remarks>
        public void SetStringArray(string name, string[] value)
        {
            if (value == null || value.Length == 0)
            {
                var valueNames = Root.GetValueNames();
                if (valueNames != null)
                    foreach (var str in valueNames)
                        if (str.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            Root.DeleteValue(name);
                            return;
                        }
            }
            else
            {
                Root.SetValue(name, value);
            }
        }

        #endregion
    }
}
//#endif