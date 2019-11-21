using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Refactorius
{
    /// <summary>Strongly-typed extension methods to get custom attributes.</summary>
    [PublicAPI]
    public static class ReflectionExtensions
    {
        /// <summary>Returns an array of custom attributes applied to this member and identified by <see cref="T:System.Type"/>.</summary>
        /// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this type are returned. </typeparam>
        /// <returns>An array of custom attributes applied to this member, or an array with zero elements if no attributes
        /// assignable to <typeparamref name="T"/> have been applied.</returns>
        /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
        /// <exception cref="T:System.InvalidOperationException">This member belongs to a type that is loaded into the
        /// reflection-only context. See How to: Load Assemblies into the Reflection-Only Context.</exception>
        [ItemNotNull]
        [NotNull]
        public static T[] GetCustomAttributes<T>([NotNull] this MemberInfo mi)
        {
            mi.MustNotBeNull(nameof(mi));
            return mi.GetCustomAttributes(typeof(T), true)
                .Cast<T>()
                .ToArray();
        }

        /// <summary>Returns an optional custom attribute applied to this member and identified by <see cref="T:System.Type"/>.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <returns>A single custom attribute of type <typeparamref name="T"/> applied to this member, or <see langword="null"/>
        /// if no such attribute has been applied.</returns>
        /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
        /// <exception cref="T:System.InvalidOperationException">This member belongs to a type that is loaded into the
        /// reflection-only context. See How to: Load Assemblies into the Reflection-Only Context.</exception>
        [CanBeNull]
        public static T GetCustomAttribute<T>([NotNull] this MemberInfo mi)
        {
            mi.MustNotBeNull(nameof(mi));
            return mi.GetCustomAttributes(typeof(T), true)
                .Cast<T>()
                .SingleOrDefault();
        }

        /// <summary>Returns an optional custom attribute applied to this member and identified by <see cref="T:System.Type"/>.</summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <returns>A single custom attribute of type <typeparamref name="T"/> applied to this member, or <see langword="null"/>
        /// if no such attribute has been applied.</returns>
        /// <exception cref="T:System.TypeLoadException">A custom attribute type cannot be loaded. </exception>
        /// <exception cref="T:System.InvalidOperationException">This member belongs to a type that is loaded into the
        /// reflection-only context. See How to: Load Assemblies into the Reflection-Only Context.</exception>
        /// <exception cref="FrameworkInitializationException">No such attribute has been applied.</exception>
        [NotNull]
        [ContractAnnotation("mi:null => halt")]
        public static T GetCustomAttributeOrFail<T>([NotNull] this MemberInfo mi, string message = null)
        {
            mi.MustNotBeNull(nameof(mi));
            var attr = mi.GetCustomAttributes(typeof(T), true)
                .Cast<T>()
                .SingleOrDefault();
            if (attr != null)
                return attr;

            var type = mi as Type;
            var name = type != null ? type.FullName : mi.DeclaringType?.FullName + "." + mi.Name;
            message = message ?? "Attribute '{0}' not found";
            throw new FrameworkInitializationException(message.SafeFormat(name));
        }
    }
}