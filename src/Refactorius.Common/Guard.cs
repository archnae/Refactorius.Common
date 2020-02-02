using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Refactorius
{
    /// <summary>The collection of useful assertion utility methods that simplify things such as argument checks.</summary>
    /// <remarks>
    ///     <para>Boldly stolen from <b>Spring.Net</b> framework <b>Spring.Util.AssertUtils</b> and many other places</para>
    ///     <para>Not intended to be used directly by applications.</para>
    /// </remarks>
    /// <author>Aleksandar Seovic</author>
    [PublicAPI]
    public static class Guard
    {
        #region message constants (to be moved into resource)

#if NEVER
        private const string ArgumentIsNull0 = "Argument is null.";
        private const string ArgumentNotAnInstanceOfType1 =
"Argument is not an instance of '{0}' or its descendant type.";
        private const string ArgumentIsAnEmptyString0 = "Argument is an empty string.";
        private const string ArgumentContainsOnlyWhitespaces0 = "Argument contains only whitespace characters.";
        private const string ArgumentIsEmptyCollection0 = "Argument is an empty collection.";
        private const string ArgumentOutsideRange0 = "Argument is not within the allowed range.";
#endif
        private const string ArgumentIsNull1 = "Argument '{0}' is null.";

        private const string ArgumentNotAnInstanceOfType2 =
            "Argument '{0}' is not an instance of '{1}' or its descendant type.";

        private const string ArgumentIsAnEmptyString1 = "Argument '{0}' is an empty string.";
        private const string ArgumentContainsOnlyWhitespaces1 = "Argument '{0}' contains only whitespace characters.";
        private const string ArgumentIsEmptyCollection1 = "Argument '{0}' is an empty collection.";

        private const string ArgumentHasDefaultValue2 =
            "Argument '{0}' has a default value of type {1} (zero, empty or null).";

        private const string ArgumentOutsideRange1 = "Argument '{0}' is not within the allowed range.";
        private const string ArgumentDoesNotInheritType2 = "Type argument '{0}' is not inherited from '{1}'.";

        #endregion

        #region validation methods

        /// <summary>
        ///     Checks the value of the supplied <paramref name="invariant" /> and throws an <see cref="System.Exception" /> if
        ///     it is <see langword="false" />.
        /// </summary>
        /// <param name="invariant">The invariant to check.</param>
        /// <exception cref="System.Exception">If the supplied <paramref name="invariant" /> is <see langword="false" />.</exception>
        public static void Assert(bool invariant)
        {
            Assert(invariant, "Assertion failed.");
        }

        /// <summary>
        ///     Checks the value of the supplied <paramref name="invariant" /> and throws an <see cref="System.Exception" /> if
        ///     it is <see langword="false" />.
        /// </summary>
        /// <param name="invariant">The invariant to check.</param>
        /// <param name="message">The exception message.</param>
        /// <exception cref="System.Exception">If the supplied <paramref name="invariant" /> is <see langword="false" />.</exception>
        public static void Assert(bool invariant, string message)
        {
            if (!invariant)
                throw new InvalidOperationException(message);
        }

        #endregion

        #region validation extension methods (recommended)

        #region MustNotBeNull

        /// <summary>
        ///     Checks the value of the supplied <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentNullException" /> if it is <see langword="null" />.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="argument" />.</typeparam>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentNullException">If the supplied <paramref name="argument" /> is <see langword="null" />.</exception>
        [NotNull]
        [ContractAnnotation("argument:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustNotBeNull<T>(
            [CanBeNull] [NoEnumeration] [ValidatedNotNull]
            this T argument,
            [NotNull] [InvokerParameterName] string name) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(name, ArgumentIsNullMessage(name));

            return argument;
        }

        /// <summary>
        ///     Checks the value of the supplied <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentNullException" /> if it is <see langword="null" />.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="argument" />.</typeparam>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">
        ///     An arbitrary message that will be passed to any thrown <see cref="System.ArgumentNullException" />
        ///     .
        /// </param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentNullException">If the supplied <paramref name="argument" /> is <see langword="null" />.</exception>
        [NotNull]
        [ContractAnnotation("argument:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MustNotBeNull<T>(
            [CanBeNull] [NoEnumeration] [ValidatedNotNull]
            this T argument,
            [NotNull] [InvokerParameterName] string name,
            [NotNull] string message) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(name, message);

            return argument;
        }

        #endregion

        #region MustImplement

        /// <summary>Checks whether the supplied <paramref name="argument" /> can be cast into the <typeparamref name="T" />.</summary>
        /// <typeparam name="T">The type of <paramref name="argument" />.</typeparam>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <remarks>
        ///     This method is intended for the validation of a constructor argument before calling the base constructor with
        ///     the argument's property as a parameter.
        /// </remarks>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is not <see langword="null" />
        ///     and of a wrong type.
        /// </exception>
        [SuppressMessage("Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "method",
            Target = "Refactorius.Commons.Collections",
            Justification =
                "Argument type validating assertion method should be able to accept parameter of any type.")
        ]
        [ContractAnnotation("argument:null => null; argument:notnull => notnull")]
        public static T MustImplement<T>(
            [CanBeNull] [NoEnumeration] this object argument,
            [NotNull] string name) where T : class
        {
            if (argument != null && !(argument is T))
                throw new ArgumentException(ArgumentNotAnInstanceOfTypeMessage(name, typeof(T).FullName), name);

            return (T)argument;
        }

        /// <summary>
        ///     Checks whether the supplied <paramref name="argument" /> can be cast into the <paramref name="requiredType" />
        ///     .
        /// </summary>
        /// <param name="argument">The object to check.</param>
        /// <param name="requiredType">The required <c>Type</c>.</param>
        /// <param name="name">The argument name.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     If the supplied <paramref name="requiredType" /> is
        ///     <see langword="null" />.
        /// </exception>
        /// <remarks>
        ///     This method is intended for the validation of a constructor argument before calling the base constructor with
        ///     the argument's property as a parameter.
        /// </remarks>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is not <see langword="null" />
        ///     and of a wrong type.
        /// </exception>
        [CanBeNull]
        [SuppressMessage("Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "method",
            Target = "Refactorius.Commons.Collections",
            Justification =
                "Argument type validating assertion method should be able to accept parameter of any type.")]
        [ContractAnnotation("argument:null => null; argument:notnull => notnull; requiredType:null => halt")]
        public static object MustImplement(
            [CanBeNull] [NoEnumeration] this object argument,
            Type requiredType,
            [NotNull] string name)
        {
            if (requiredType == null)
                throw new ArgumentNullException(nameof(requiredType));

            if (argument != null && !requiredType.IsInstanceOfType(argument))
                throw new ArgumentException(ArgumentNotAnInstanceOfTypeMessage(name, requiredType.FullName), name);

            return argument;
        }

        /// <summary>Checks whether the specified <paramref name="argument" /> can be cast into the <typeparamref name="T" />.</summary>
        /// <typeparam name="T">The required type for the argument.</typeparam>
        /// <param name="argument">The argument to check.</param>
        /// <param name="argumentName">The name of the argument to check.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.ArgumentException" />.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is not <see langword="null" /> of
        ///     a wrong type.
        /// </exception>
        [SuppressMessage("Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "method",
            Target = "Refactorius.Commons.Collections",
            Justification =
                "Argument type validating assertion method should be able to accept parameter of any type.")
        ]
        [ContractAnnotation("argument:null => null; argument:notnull => notnull")]
        public static T MustImplement<T>(
            [CanBeNull] [NoEnumeration] this object argument,
            [NotNull] string argumentName,
            [NotNull] string message)
        {
            if (argument != null && !(argument is T))
                throw new ArgumentException(message, argumentName);

            return (T)argument;
        }

        /// <summary>
        ///     Checks whether the specified <paramref name="argument" /> can be cast into the
        ///     <paramref name="requiredType" />.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <param name="requiredType">The required type for the argument.</param>
        /// <param name="argumentName">The name of the argument to check.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.ArgumentException" />.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     If the supplied <paramref name="requiredType" /> is
        ///     <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is not <see langword="null" />
        ///     and of a wrong type.
        /// </exception>
        [ContractAnnotation("argument:null => null; argument:notnull => notnull; requiredType:null => halt")]
        public static object MustImplement(
            [CanBeNull] [NoEnumeration] this object argument,
            [NotNull] Type requiredType,
            [NotNull] string argumentName,
            [NotNull] string message)
        {
            if (requiredType == null)
                throw new ArgumentNullException(nameof(requiredType));

            if (argument != null && !requiredType.IsInstanceOfType(argument))
                throw new ArgumentException(message, argumentName);

            return argument;
        }

        #endregion

        #region MustInherit

        /// <summary>
        ///     Checks whether an instance of the specified <paramref name="type" /> can be cast into the
        ///     <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The required type for the argument.</typeparam>
        /// <param name="type">The argument to check.</param>
        /// <param name="argumentName">The name of the argument to check.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.ArgumentException" />.</param>
        /// <returns>The original <paramref name="type" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="type" /> is not <see langword="null" /> of a
        ///     wrong type.
        /// </exception>
        [SuppressMessage("Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter", Scope = "method",
            Target = "Refactorius.Commons.Collections",
            Justification =
                "Argument type validating assertion method should be able to accept parameter of any type.")]
        [NotNull]
        [ContractAnnotation("type:null => halt")]
        public static Type MustInherit<T>(this Type type, [NotNull] string argumentName, string message = null)
        {
            type.MustNotBeNull(nameof(type));

            if (type != null && !typeof(T).IsAssignableFrom(type))
                throw new ArgumentException(
                    message ?? ArgumentDoesNotInheritTypeMessage(type.FullName, typeof(T).FullName),
                    argumentName);

            return type;
        }

        /// <summary>
        ///     Checks whether an instance of the specified <paramref name="argument" /> can be cast into the
        ///     <paramref name="requiredType" />.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <param name="requiredType">The required type for the argument.</param>
        /// <param name="argumentName">The name of the argument to check.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.ArgumentException" />.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     If the supplied <paramref name="requiredType" /> is
        ///     <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is not <see langword="null" />
        ///     and of a wrong type.
        /// </exception>
        [CanBeNull]
        [ContractAnnotation("argument:null => null; requiredType:null => halt")]
        public static Type MustInherit(
            [CanBeNull] this Type argument,
            Type requiredType,
            [NotNull] string argumentName,
            string message = null)
        {
            requiredType.MustNotBeNull(nameof(requiredType));

            if (argument != null && !requiredType.IsAssignableFrom(argument))
                throw new ArgumentException(message, argumentName);

            return argument;
        }

        #endregion

        #region MustNotBeEmpty

        /// <summary>
        ///     Checks the value of the supplied string <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentNullException" /> if it is <see langword="null" /> or
        ///     <see cref="System.ArgumentException" /> if
        ///     it is an empty (zero-length) string.
        /// </summary>
        /// <param name="argument">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <returns>The original <paramref name="argument" /> value.</returns>
        /// <exception cref="System.ArgumentNullException">If the supplied <paramref name="argument" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is an empty (zero-length)
        ///     string.
        /// </exception>
        [NotNull]
        [ContractAnnotation("argument:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MustNotBeEmpty(this string argument, [NotNull] [InvokerParameterName] string name)
        {
            if (argument == null)
                throw new ArgumentNullException(name);

            if (string.IsNullOrEmpty(argument))
                throw new ArgumentException(ArgumentIsEmptyMessage(name), name);

            return argument;
        }

        /// <summary>
        ///     Checks the value of the supplied string <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentNullException" /> if it is <see langword="null" /> or
        ///     <see cref="System.ArgumentException" /> if
        ///     it is an empty (zero-length) string.
        /// </summary>
        /// <param name="argument">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.Exception" />.</param>
        /// <returns>The original <paramref name="argument" /> value.</returns>
        /// <exception cref="System.ArgumentNullException">If the supplied <paramref name="argument" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is an empty (zero-length)
        ///     string.
        /// </exception>
        [NotNull]
        [ContractAnnotation("argument:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MustNotBeEmpty(
            this string argument,
            [NotNull] [InvokerParameterName] string name,
            [NotNull] string message)
        {
            if (argument == null)
                throw new ArgumentNullException(name, message);

            if (string.IsNullOrEmpty(argument))
                throw new ArgumentException(message, name);

            return argument;
        }

        #endregion

        #region MustHaveText

        /// <summary>
        ///     Checks the value of the supplied string <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentNullException" /> if it is <see langword="null" /> or
        ///     <see cref="System.ArgumentException" /> if
        ///     it contains only whitespace character(s).
        /// </summary>
        /// <param name="argument">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <returns>The original <paramref name="argument" /> value.</returns>
        /// <exception cref="System.ArgumentNullException">If the supplied <paramref name="argument" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> contains only whitespace
        ///     character(s).
        /// </exception>
        [NotNull]
        [ContractAnnotation("argument:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MustHaveText(this string argument, [NotNull] [InvokerParameterName] string name)
        {
            argument.MustNotBeEmpty(name);

            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                    ArgumentContainsOnlyWhitespaces1, name));

            return argument;
        }

        /// <summary>
        ///     Checks the value of the supplied string <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentNullException" /> if it is <see langword="null" /> or
        ///     <see cref="System.ArgumentException" /> if
        ///     it contains only whitespace character(s).
        /// </summary>
        /// <param name="argument">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.Exception" />.</param>
        /// <returns>The original <paramref name="argument" /> value.</returns>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentNullException">If the supplied <paramref name="argument" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> contains only whitespace
        ///     character(s).
        /// </exception>
        [NotNull]
        [ContractAnnotation("argument:null => halt")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string MustHaveText(
            this string argument,
            [NotNull] [InvokerParameterName] string name,
            [NotNull] string message)
        {
            argument.MustNotBeEmpty(name, message);

            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentException(message, name);

            return argument;
        }

        #endregion

        #region MustNotBeDefault

        /// <summary>
        ///     Checks the value of the supplied <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentException" /> if it is the default(zero) value for <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="argument">The object to check.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is the default(zero) value for
        ///     <typeparamref name="T" />.
        /// </exception>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly",
            Justification = "Using default exception message.")]
        public static T MustNotBeDefault<T>(this T argument) where T : struct
        {
            if (argument.Equals(default(T)))
                throw new ArgumentNullException(nameof(argument));

            return argument;
        }

        /// <summary>
        ///     Checks the value of the supplied <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentException" /> if it is the default(zero) value for <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is the default(zero) value for
        ///     <typeparamref name="T" />.
        /// </exception>
        public static T MustNotBeDefault<T>(this T argument, string name) where T : struct
        {
            if (argument.Equals(default(T)))
                throw new ArgumentException(ArgumentHasDefaultValueMessage(name, typeof(T).FullName), name);

            return argument;
        }

        /// <summary>
        ///     Checks the value of the supplied <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentException" /> if it is the default(zero) value for <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.ArgumentException" />.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is the default(zero) value for
        ///     <typeparamref name="T" />.
        /// </exception>
        public static T MustNotBeDefault<T>(this T argument, string name, string message) where T : struct
        {
            if (argument.Equals(default(T)))
                throw new ArgumentException(name, message);

            return argument;
        }

        #endregion

        #region MustHaveElements

        /// <summary>
        ///     Checks the value of the supplied <see cref="ICollection" /> <paramref name="argument" /> and throws an
        ///     <see cref="ArgumentNullException" /> if it is <see langword="null" /> or <see cref="ArgumentException" /> if it
        ///     contains
        ///     no elements.
        /// </summary>
        /// <typeparam name="T">The required type for the elements of <paramref name="argument" />.</typeparam>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentNullException">If the supplied <paramref name="argument" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">If the supplied <paramref name="argument" /> contains no elements.</exception>
        [NotNull]
        [ContractAnnotation("argument:null => halt")]
        public static ICollection<T> MustHaveElements<T>(this ICollection<T> argument, [NotNull] string name)
        {
            if (argument == null)
                throw new ArgumentNullException(name, ArgumentIsNullMessage(name));

            if (argument.Count == 0)
                throw new ArgumentException(ArgumentIsEmptyCollectionMessage(name), name);

            return argument;
        }

        /// <summary>
        ///     Checks the value of the supplied <see cref="ICollection" /> <paramref name="argument" /> and throws an
        ///     <see cref="ArgumentNullException" /> if it is <see langword="null" /> or <see cref="ArgumentException" /> if it
        ///     contains
        ///     no elements.
        /// </summary>
        /// <typeparam name="T">The required type for the elements of <paramref name="argument" />.</typeparam>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.Exception" />.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentNullException">If the supplied <paramref name="argument" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException">If the supplied <paramref name="argument" /> contains no elements.</exception>
        [NotNull]
        [ContractAnnotation("argument:null => halt")]
        public static ICollection<T> MustHaveElements<T>(
            this ICollection<T> argument,
            [NotNull] string name,
            [NotNull] string message)
        {
            if (argument == null)
                throw new ArgumentNullException(name, message);

            if (argument.Count == 0)
                throw new ArgumentException(message, name);

            return argument;
        }

        #endregion

        #region MustBeInRange

        /// <summary>
        ///     Checks the value of the supplied <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentException" /> if it is outside the <paramref name="range" />.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="argument">The object to check.</param>
        /// <param name="range">The allowed <see cref="Range{T}" /> of <paramref name="argument" /> values.</param>
        /// <param name="name">The argument name.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is s outside the
        ///     <paramref name="range" />.
        /// </exception>
        [NotNull]
        [ContractAnnotation("range:null => halt")]
        public static T MustBeInRange<T>([NotNull] this T argument, Range<T> range, [NotNull] string name)
        {
            range.MustNotBeNull(nameof(argument));

            if (!range.Contains(argument))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, ArgumentOutsideRange1, name),
                    name);

            return argument;
        }

        /// <summary>
        ///     Checks the value of the supplied <paramref name="argument" /> and throws an
        ///     <see cref="System.ArgumentException" /> if it is outside the <paramref name="range" />.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="argument">The object to check.</param>
        /// <param name="range">The allowed <see cref="Range{T}" /> of <paramref name="argument" /> values.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.ArgumentException" />.</param>
        /// <returns>The original <paramref name="argument" />.</returns>
        /// <exception cref="System.ArgumentException">
        ///     If the supplied <paramref name="argument" /> is s outside the
        ///     <paramref name="range" />.
        /// </exception>
        [NotNull]
        [ContractAnnotation("range:null => halt")]
        public static T MustBeInRange<T>(
            [NotNull] this T argument,
            Range<T> range,
            [NotNull] string name,
            [NotNull] string message)
        {
            range.MustNotBeNull(nameof(argument));

            if (!range.Contains(argument))
                throw new ArgumentException(message, name);

            return argument;
        }

        #endregion

        /// <summary>
        /// Verifies that parameter is an anonymous object.
        /// </summary>
        /// <param name="anon">A parameter to test.</param>
        /// <param name="name">A parameter name.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="anon"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">if <paramref name="anon"/> is not an anonymous object.</exception>
        public static void MustBeAnonymousObject(this object anon, string name)
        {
            anon.MustNotBeNull(name);
            var type = anon.GetType();
            if (!type.IsAnonymousType())
                throw new ArgumentException("Argument must be of an anonymous type.", name);
        }

        #endregion

        #region private message formatters

        // ReSharper disable InconsistentNaming
        private static string ArgumentIsNullMessage(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, ArgumentIsNull1, name);
        }

        private static string ArgumentIsEmptyMessage(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, ArgumentIsAnEmptyString1, name);
        }

        private static string ArgumentIsEmptyCollectionMessage(string name)
        {
            return string.Format(CultureInfo.InvariantCulture, ArgumentIsEmptyCollection1, name);
        }

#if NEVER
        private static string ArgumentNotAnInstanceOfTypeMessage(string typeName)
        {
            return string.Format(CultureInfo.CurrentUICulture, ArgumentNotAnInstanceOfType1, typeName);
        }
#endif

        private static string ArgumentNotAnInstanceOfTypeMessage(string name, string typeName)
        {
            return string.Format(CultureInfo.InvariantCulture, ArgumentNotAnInstanceOfType2, name, typeName);
        }

        private static string ArgumentHasDefaultValueMessage(string name, string typeName)
        {
            return string.Format(CultureInfo.InvariantCulture, ArgumentHasDefaultValue2, name, typeName);
        }

        private static string ArgumentDoesNotInheritTypeMessage(string name, string typeName)
        {
            return string.Format(CultureInfo.InvariantCulture, ArgumentDoesNotInheritType2, name, typeName);
        }
        // ReSharper enable InconsistentNaming

        #endregion
    }
}