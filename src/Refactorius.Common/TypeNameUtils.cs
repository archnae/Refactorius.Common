using System.Collections.Concurrent;
using System.Text;

namespace Refactorius;

/// <summary>Utility method related to type names.</summary>
[PublicAPI]
public static class TypeNameUtils
{
    //public static readonly string[] NoWellKnownNamespaces = new string[0];

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

    /// <summary>Registers new well known namespace.</summary>
    /// <param name="wellKnownNamespace">A well known namespace.</param>
    [Obsolete("Use TypeUtils.RegisterWellKnownNamespace")]
    public static void RegisterWellKnownNamespace(string wellKnownNamespace)
    {
        wellKnownNamespace.MustHaveText(nameof(wellKnownNamespace));
        WellKnownNamespaces.Add(wellKnownNamespace);
    }

    /// <summary>Creates a readable string for a name of a specified <c>Type</c>.</summary>
    /// <param name="type">A type (possible generic).</param>
    /// <param name="wellKnownNamespaces">The list of well-known namespaces omitted from readable type names.</param>
    /// <returns>An informal string representation of <paramref name="type"/> name.</returns>
    [Pure]
    [Obsolete("Use TypeUtils.GetReadableTypeName")]
    public static string GetReadableTypeName(Type type,
        [InstantHandle] IReadOnlyCollection<string>? wellKnownNamespaces = null)
    {
        type.MustNotBeNull(nameof(type));
        var typeName = type.FullName;
        if (typeName == null)
            return "?UnnamedType?";
        // ReSharper disable once SuspiciousTypeConversion.Global
        wellKnownNamespaces ??= WellKnownNamespaces;
        typeName = RemoveWellKnownNameSpaces(typeName, wellKnownNamespaces);
        if (!type.IsGenericType)
            return typeName;

        var genericPrefix = "<";
        var genericPostfix = ">";

        var ta = type.GenericTypeArguments;
        var sb = new StringBuilder(typeName.Substring(0, typeName.IndexOf("`", StringComparison.Ordinal)));
        sb.Append(genericPrefix);
        // ReSharper disable once PossibleNullReferenceException
        for (var i = 0; i < ta.Length; i++)
        {
            if (i > 0)
                sb.Append(',');
            // ReSharper disable once AssignNullToNotNullAttribute
            sb.Append(GetReadableTypeName(ta[i], wellKnownNamespaces));
        }
        sb.Append(genericPostfix);
        typeName = sb.ToString();
        return typeName;
    }

    /// <summary>Removes well-known namespaces from a type name.</summary>
    /// <param name="typeName">A readable type name string.</param>
    /// <param name="wellKnownNamespaces">A sequence of well-known namespace names.</param>
    /// <returns>The <paramref name="typeName"/> with all <paramref name="wellKnownNamespaces"/> removed.</returns>
    [Pure]
    [Obsolete("Use TypeUtils.RemoveWellKnownNameSpaces")]
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
}