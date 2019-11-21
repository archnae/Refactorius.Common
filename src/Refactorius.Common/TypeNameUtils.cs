using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Refactorius
{
    /// <summary>Utility method related to type names.</summary>
    public static class TypeNameUtils
    {
        //public static readonly string[] NoWellKnownNamespaces = new string[0];

        /// <summary>Gets the collection of well-known namespaces.</summary>
        public static readonly ConcurrentBag<string> WellKnownNamespaces = new ConcurrentBag<string>
        {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Collections.Concurrent",
            "Microsoft.Practices.ServiceLocation",
            "Refactorius",
            "Refactorius.Extensions",
            "Refactorius.Configuration",
            "DukeNet"
        };

        /// <summary>Registers new well known namespace.</summary>
        /// <param name="wellKnownNamespace">A well known namespace.</param>
        public static void RegisterWellKnownNamespace([NotNull] string wellKnownNamespace)
        {
            wellKnownNamespace.MustHaveText(nameof(wellKnownNamespace));
            WellKnownNamespaces.Add(wellKnownNamespace);
        }

        /// <summary>Creates a readable string for a name of a specified <c>Type</c>.</summary>
        /// <param name="type">A type (possible generic).</param>
        /// <param name="wellKnownNamespaces">The list of well-known namespaces omitted from readable type names.</param>
        /// <returns>An informal string representation of <paramref name="type"/> name.</returns>
        [NotNull]
        [Pure]
        public static string GetReadableTypeName([NotNull] Type type,
            [InstantHandle] IReadOnlyCollection<string> wellKnownNamespaces = null)
        {
            type.MustNotBeNull(nameof(type));
            var typeName = type.FullName;
            if (typeName == null)
                return "?UnnamedType?";
            // ReSharper disable once SuspiciousTypeConversion.Global
            wellKnownNamespaces = wellKnownNamespaces ?? WellKnownNamespaces as IReadOnlyCollection<string>;
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
                    sb.Append(",");
                // ReSharper disable once AssignNullToNotNullAttribute
                sb.Append(GetReadableTypeName(ta[i], wellKnownNamespaces));
            }
            sb.Append(genericPostfix);
            typeName = sb.ToString();
            return typeName;
        }

        /// <summary>Removes well-nown namespaces from a type name.</summary>
        /// <param name="typeName">A stringyfied type name.</param>
        /// <param name="wellKnownNamespaces">A sequence of well-known namespace names.</param>
        /// <returns>The <paramref name="typeName"/> with all <paramref name="wellKnownNamespaces"/> removed.</returns>
        [NotNull]
        [Pure]
        public static string RemoveWellKnownNameSpaces([NotNull] string typeName,
            [ItemNotNull] [CanBeNull] IReadOnlyCollection<string> wellKnownNamespaces)
        {
            if (typeName.IsEmpty() || wellKnownNamespaces == null)
                return typeName;

            foreach (var s in wellKnownNamespaces)
            {
                var prefix = s.EndsWith(".") ? s : s + ".";
                if (typeName.StartsWith(prefix))
                    typeName = typeName.Replace(prefix, string.Empty);

                typeName = typeName.Replace("," + prefix, ",");
                typeName = typeName.Replace("(" + prefix, "(");
                typeName = typeName.Replace("<" + prefix, "<");
            }

            return typeName;
        }
    }
}