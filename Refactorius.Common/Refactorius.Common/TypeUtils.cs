using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Refactorius
{
    /// <summary>The collection of useful type-related utility methods.</summary>
    public static class TypeUtils
    {
        #region TypeComparerInstance

        /// <summary>Gets an instance of <see cref="TypeComparer"/>.</summary>
        /// <value>A static instance of <see cref="TypeComparer"/>.</value>
        public static TypeComparer TypeComparer { get; } = new TypeComparer();

        #endregion

        /// <summary>Gets the value of <see cref="System.ComponentModel.DescriptionAttribute"/> for the specified type member.</summary>
        /// <param name="type">The containing <see cref="Type"/>.</param>
        /// <param name="name">The member name.</param>
        /// <param name="bindingAttr">The combination of <see cref="BindingFlags"/> to use for <paramref name="name"/> lookup.</param>
        /// <returns>The <see cref="String"/> value of the <see cref="System.ComponentModel.DescriptionAttribute"/> for the member
        /// <paramref name="name"/> of type <paramref name="type"/>, or <see langword="null"/> if it has no description.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="type"/> is <see langword="null"/> .</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is an empty string.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="type"/> has either no members named <paramref name="name"/>
        /// or more than one.</exception>
        [CanBeNull]
        public static string GetDescription([NotNull] Type type, [NotNull] string name, BindingFlags bindingAttr)
        {
            type.MustNotBeNull(nameof(type));
            name.MustHaveText(nameof(name));

            var memberInfos = type.GetMember(name, bindingAttr);
            if (memberInfos.Length == 0)
                throw new InvalidOperationException(name + " is not a member of " + type.FullName);
            if (memberInfos.Length > 1)
                throw new InvalidOperationException(name + " is ambiguous in " + type.FullName);

            // get System.ComponentMode.Description for the value
            var attributes =
                (DescriptionAttribute[]) memberInfos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 ? attributes[0].Description : null;
        }

        /// <summary>Tests if the specified type is a <see cref="Nullable&lt;T&gt;"/>.</summary>
        /// <param name="type">The <see cref="Type"/> to test.</param>
        /// <returns><see langword="true"/> if <paramref name="type"/> is a <see cref="Nullable&lt;T&gt;"/>, otherwise
        /// <see langword="false"/>.</returns>
        public static bool IsNullableType([NotNull] this Type type)
        {
            type.MustNotBeNull(nameof(type));

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        /// <summary>Tests if the specified type can be assigned a <see langword="null"/> value.</summary>
        /// <param name="type">The <see cref="Type"/> to test.</param>
        /// <returns><see langword="true"/> if <paramref name="type"/> is a <see cref="Nullable&lt;T&gt;"/> or a reference type,
        /// otherwise <see langword="false"/>.</returns>
        public static bool IsNullAssignableType([NotNull] this Type type)
        {
            type.MustNotBeNull(nameof(type));

            return !type.IsValueType || IsNullableType(type);
        }

        /// <summary>Gets the underlying <see cref="Type"/> of a value type.</summary>
        /// <param name="type">A value <see cref="Type"/>.</param>
        /// <returns>The underlying <see cref="Type"/> <b>T</b> if <paramref name="type"/> is a <see cref="Nullable&lt;T&gt;"/>,
        /// the <paramref name="type"/> itself if it's a non-nullable value type or <see langword="null"/> if
        /// <paramref name="type"/> is a reference type.</returns>
        [CanBeNull]
        public static Type UnwrapNullableType([NotNull] Type type)
        {
            type.MustNotBeNull(nameof(type));

            if (IsNullableType(type))
                return type.GetGenericArguments()[0];
            if (type.IsValueType)
                return type;
            return null;
        }

        /// <summary>Tests if the specified full type name is a <b>Nullable&lt;T&gt;</b> or a <b>T?</b>.</summary>
        /// <param name="typeName">The full type name to test.</param>
        /// <returns><see langword="true"/> if <paramref name="typeName"/> is a <b>Nullable&lt;T&gt;</b> or a <b>T?</b>, otherwise
        /// <see langword="false"/>.</returns>
        public static bool IsNullableTypeName([NotNull] string typeName)
        {
            typeName.MustHaveText(nameof(typeName));

            if (typeName.StartsWith(NULLABLE_TYPENAME_PREFIX, StringComparison.Ordinal))
                return true;
            if (typeName.EndsWith("?", StringComparison.Ordinal))
                return true;
            return false;
        }

        /// <summary>Gets the name of the underlying <see cref="Type"/> for a given type name.</summary>
        /// <param name="typeName">The full type name.</param>
        /// <returns>The underlying <b>T</b> name if <paramref name="typeName"/> is a <b>Nullable&lt;T&gt;</b> or a <b>T?</b>,
        /// otherwise <see langword="null"/>.</returns>
        /// <remarks>Unlike <see cref="UnwrapNullableType"/>, this method returns <see langword="null"/> for all not explicitly
        /// nullable types.</remarks>
        /// <seealso cref="UnwrapNullableType"/>
        [CanBeNull]
        public static string UnwrapNullableTypeName([NotNull] string typeName)
        {
            typeName.MustHaveText(nameof(typeName));

            if (typeName.StartsWith(NULLABLE_TYPENAME_PREFIX, StringComparison.Ordinal))
            {
                typeName = typeName.Substring(NULLABLE_TYPENAME_PREFIX.Length + 1);
                var bracketPos = typeName.IndexOf(']');
                if (bracketPos <= 0)
                    throw new ArgumentException("The type name is in a wrong format", nameof(typeName));
                var commaPos = typeName.IndexOf(',', 0, bracketPos);
                return typeName.Substring(0, commaPos > 0 ? commaPos : bracketPos);
            }

            if (typeName.EndsWith("?", StringComparison.Ordinal))
                return typeName.Substring(0, typeName.Length - 1);
            return null;
        }

        /// <summary>Tests if the specified type is an anonymous type.</summary>
        /// <param name="type">The <see cref="Type"/> to test.</param>
        /// <returns><see langword="true"/> if <paramref name="type"/> is an anonymous type, otherwise <see langword="false"/>.</returns>
        /// <remarks>See http://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type .</remarks>
        public static bool IsAnonymousType([NotNull] this Type type)
        {
            type.MustNotBeNull(nameof(type));

            var name = type.Name;
            return type.Namespace == null
                   && type.IsSealed && !type.IsPublic
                   && name.StartsWith("<>", StringComparison.OrdinalIgnoreCase)
                   && name.IndexOf("AnonymousType", StringComparison.Ordinal) > 0;
        }

        /// <summary>Returns nice readable type name with generic type parameters resolved.</summary>
        /// <param name="type">The current <see cref="Type"/> instance.</param>
        /// <returns>The readable name for the <paramref name="type"/> (type name only, namespace not included).</returns>
        [NotNull]
        public static string DisplayName([NotNull] this Type type)
        {
            type.MustNotBeNull(nameof(type));

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
                return underlyingType.Name + "?";

            return type.FullName
                .SplitAndTrim("+")
                .Select(
                    nested =>
                    {
                        var genericBracket = nested.IndexOf("[", StringComparison.Ordinal);
                        if (genericBracket > 0)
                            nested = nested.Substring(0, genericBracket);
                        var parts = nested.SplitAndTrim(".");
                        var nestedName = parts[parts.Length - 1];
                        var genericPos = nestedName.IndexOf("`", StringComparison.Ordinal);
                        if (genericPos > 0)
                            nestedName = nestedName.Substring(0, genericPos) + "<"
                                         + type.GenericTypeArguments.Select(t => t.DisplayName()).Join(",")
                                         + ">";

                        return nestedName;
                    })
                .Join("+");
        }

        /// <summary>Calculates total hash code of a collection of objects.</summary>
        /// <param name="args">A collection of objects (some may be null).</param>
        /// <returns>The total hash code.</returns>
        public static int CalculateHash(params object[] args)
        {
            unchecked
            {
                var hash = 0;
                for (var i = 0; i < args.Length; i++)
                {
                    hash = (hash * 37 + 1) << (i + i);
                    if (args[i] != null)
                        hash += args[i].GetHashCode();
                }

                return hash;
            }
        }

        #region private static fields

        private const string NULLABLE_TYPENAME_PREFIX = "System.Nullable`1[";

        #endregion

        #region private consructor

        /////// <summary>
        /////// Prevents a default instance of the <see cref="TypeUtils"/> class from being created.
        /////// </summary>
        ////private TypeUtils()
        ////{
        ////}

        #endregion
    }

    /// <summary>A <see cref="T:Comparer{T}"/> for <see cref="Type"/> objects, using for comparison
    /// <see cref="M:System.Type.FullName"/> property.</summary>
    public class TypeComparer : Comparer<Type>
    {
        /// <summary>Compares two <see cref="Type"/> objects.</summary>
        /// <param name="x">Left side <see cref="Type"/> object.</param>
        /// <param name="y">Right side <see cref="Type"/> object.</param>
        /// <returns>The comparison result, defined by comparing <paramref name="x"/> and <paramref name="y"/>'s
        /// <see cref="M:System.Type.FullName"/> properties.</returns>
        public override int Compare(Type x, Type y)
        {
            if (x == y)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;
            return string.Compare(x.FullName, y.FullName, StringComparison.OrdinalIgnoreCase);
        }
    }
}