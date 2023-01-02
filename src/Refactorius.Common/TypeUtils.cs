using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace Refactorius;

/// <summary>The collection of useful type-related utility methods.</summary>
[PublicAPI]
public static class TypeUtils
{
    /// <summary>Gets the collection of well-known namespaces.</summary>
    public static readonly ConcurrentBag<string> WellKnownNamespaces = new()
    {
        "System",
        "System.Collections",
        "System.Collections.Generic",
        "System.Collections.Concurrent",
        "Microsoft.Extensions.Logging",
        "Refactorius",
        "Refactorius.Data"
    };

    #region TypeComparerInstance

    /// <summary>Gets an instance of <see cref="TypeComparer"/>.</summary>
    /// <value>A static instance of <see cref="TypeComparer"/>.</value>
    public static TypeComparer TypeComparer { get; } = new();

    #endregion

    /// <summary>Gets the value of <see cref="DescriptionAttribute"/> for the specified type member.</summary>
    /// <param name="type">The containing <c>Type</c>.</param>
    /// <param name="name">The member name.</param>
    /// <param name="bindingAttr">The combination of <see cref="BindingFlags"/> to use for <paramref name="name"/> lookup.</param>
    /// <returns>The <see cref="String"/> value of the <see cref="DescriptionAttribute"/> for the member
    /// <paramref name="name"/> of type <paramref name="type"/>, or <see langword="null"/> if it has no description.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="type"/> is <see langword="null"/> .</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is an empty string.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="type"/> has either no members named <paramref name="name"/>
    /// or more than one.</exception>
    public static string GetDescription(Type type, string name, BindingFlags bindingAttr)
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

        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }

    /// <summary>Tests if the specified type is a <see cref="Nullable&lt;T&gt;"/>.</summary>
    /// <param name="type">The <c>Type</c> to test.</param>
    /// <returns><see langword="true"/> if <paramref name="type"/> is a <see cref="Nullable&lt;T&gt;"/>, otherwise
    /// <see langword="false"/>.</returns>
    public static bool IsNullableType(this Type type)
    {
        type.MustNotBeNull(nameof(type));

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>Tests if the specified type can be assigned a <see langword="null"/> value.</summary>
    /// <param name="type">The <c>Type</c> to test.</param>
    /// <returns><see langword="true"/> if <paramref name="type"/> is a <see cref="Nullable&lt;T&gt;"/> or a reference type,
    /// otherwise <see langword="false"/>.</returns>
    public static bool IsNullAssignableType(this Type type)
    {
        type.MustNotBeNull(nameof(type));

        return !type.IsValueType || IsNullableType(type);
    }

    /// <summary>Gets the underlying <c>Type</c> of a value type.</summary>
    /// <param name="type">A value <c>Type</c>.</param>
    /// <returns>The underlying <c>Type</c> <b>T</b> if <paramref name="type"/> is a <see cref="Nullable&lt;T&gt;"/>,
    /// the <paramref name="type"/> itself if it's a non-nullable value type or <see langword="null"/> if
    /// <paramref name="type"/> is a reference type.</returns>
    public static Type? UnwrapNullableType(Type type)
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
    public static bool IsNullableTypeName(string typeName)
    {
        typeName.MustHaveText(nameof(typeName));

        if (typeName.StartsWith(NULLABLE_TYPENAME_PREFIX, StringComparison.Ordinal))
            return true;
        if (typeName.EndsWith("?", StringComparison.Ordinal))
            return true;
        return false;
    }

    /// <summary>Gets the name of the underlying <c>Type</c> for a given type name.</summary>
    /// <param name="typeName">The full type name.</param>
    /// <returns>The underlying <b>T</b> name if <paramref name="typeName"/> is a <b>Nullable&lt;T&gt;</b> or a <b>T?</b>,
    /// otherwise <see langword="null"/>.</returns>
    /// <remarks>Unlike <see cref="UnwrapNullableType"/>, this method returns <see langword="null"/> for all not explicitly
    /// nullable types.</remarks>
    /// <seealso cref="UnwrapNullableType"/>
    public static string? UnwrapNullableTypeName(string typeName)
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
    /// <param name="type">The <c>Type</c> to test.</param>
    /// <returns><see langword="true"/> if <paramref name="type"/> is an anonymous type, otherwise <see langword="false"/>.</returns>
    /// <remarks>See http://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type .</remarks>
    public static bool IsAnonymousType(this Type type)
    {
        type.MustNotBeNull(nameof(type));

        var name = type.Name;
        return type.Namespace == null
               && type.IsSealed && !type.IsPublic
               && name.StartsWith("<>", StringComparison.OrdinalIgnoreCase)
               && name.IndexOf("AnonymousType", StringComparison.Ordinal) > 0;
    }

    /// <summary>Returns nice readable type name with generic type parameters resolved.</summary>
    /// <param name="type">The current <c>Type</c> instance.</param>
    /// <returns>The readable name for the <paramref name="type"/> (type name only, namespace not included).</returns>
    public static string DisplayName(this Type type)
    {
        type.MustNotBeNull(nameof(type));

        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
            return underlyingType.Name + "?";

        var typeName = type.FullName;
        if (typeName == null)
          return "?UnnamedType?";
        typeName = RemoveWellKnownNameSpaces(typeName, WellKnownNamespaces);

        return typeName
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
    public static int CalculateHash(params object?[] args)
    {
        unchecked
        {
            var hash = 0;
            for (var i = 0; i < args.Length; i++)
            {
                hash = (hash * 37 + 1) << (i + i);
                if (args[i] != null)
                    hash += args[i]!.GetHashCode();
            }

            return hash;
        }
    }

    /// <summary>Registers new well known namespace.</summary>
    /// <param name="wellKnownNamespace">A well known namespace.</param>
    public static void RegisterWellKnownNamespace(string wellKnownNamespace)
    {
        wellKnownNamespace.MustHaveText(nameof(wellKnownNamespace));
        WellKnownNamespaces.Add(wellKnownNamespace);
    }

    /// <summary>Removes well-known namespaces from a type name.</summary>
    /// <param name="typeName">A readable type name string.</param>
    /// <param name="wellKnownNamespaces">A sequence of well-known namespace names.</param>
    /// <returns>The <paramref name="typeName"/> with all <paramref name="wellKnownNamespaces"/> removed.</returns>
    [Pure]
    public static string RemoveWellKnownNameSpaces(string? typeName,
        IReadOnlyCollection<string>? wellKnownNamespaces)
    {
        if (string.IsNullOrEmpty(typeName) || wellKnownNamespaces == null)
            return typeName ?? string.Empty;

        foreach (var s in wellKnownNamespaces)
        {
            var prefix = s.EndsWith(".", StringComparison.Ordinal) ? s : s + ".";
            if (typeName!.StartsWith(prefix, StringComparison.Ordinal))
                typeName = typeName.Replace(prefix, string.Empty);

            typeName = typeName.Replace("," + prefix, ",");
            typeName = typeName.Replace("(" + prefix, "(");
            typeName = typeName.Replace("<" + prefix, "<");
        }

        return typeName!;
    }

    #region private static fields

    private const string NULLABLE_TYPENAME_PREFIX = "System.Nullable`1[";

    #endregion
}

/// <summary>A <see cref="Comparer{T}"/> for <c>Type</c> objects, using for comparison
/// <see cref="Type.FullName"/> property.</summary>
public class TypeComparer : Comparer<Type>
{
    /// <summary>Compares two <c>Type</c> objects.</summary>
    /// <param name="x">Left side <c>Type</c> object.</param>
    /// <param name="y">Right side <c>Type</c> object.</param>
    /// <returns>The comparison result, defined by comparing <paramref name="x"/> and <paramref name="y"/>'s
    /// <see cref="Type.FullName"/> properties.</returns>
    public override int Compare(Type? x, Type? y)
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