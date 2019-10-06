using System;
using System.Collections.Generic;
using System.Text;

namespace Refactorius
{
    public static class Guard
    {
#if NEVER /// <summary>
/// Checks the value of the supplied <paramref name="argument"/> and throws an
/// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>.
/// </summary>
/// <param name="argument">The object to check.</param>
/// <param name="name">The argument name.</param>
/// <exception cref="System.ArgumentNullException">
/// If the supplied <paramref name="argument"/> is <see langword="null"/>.
/// </exception>
        public static void ArgumentNotNull([ValidatedNotNullAttribute] object argument, string name)
        {
            if (argument == null)
                throw new ArgumentNullException(name, ArgumentIsNullMessage(name));
        }

        /// <summary>
        /// Checks the value of the supplied <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>.
        /// </summary>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentNullException"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        public static void ArgumentNotNull([ValidatedNotNullAttribute] object argument, string name, string message)
        {
            if (argument == null)
                throw new ArgumentNullException(name, message);
        }

        /// <summary>
        /// Checks the value of the supplied string <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/> 
        /// or <see cref="ArgumentException"/> if it contains only whitespace character(s).
        /// </summary>
        /// <param name="argument">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="argument"/> contains only whitespace character(s).
        /// </exception>
        public static void ArgumentHasText([ValidatedNotNullAttribute] string argument, string name)
        {
            if (argument == null)
                throw new ArgumentNullException(name, ArgumentIsNullMessage(name));

            if (StringUtils.IsEmpty(argument))
                throw new ArgumentException(ArgumentIsEmptyMessage(name), name);
        }

        /// <summary>
        /// Checks the value of the supplied string <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentNullException"/> if it is <see langword="null"/>
        /// or <see cref="System.ArgumentException"/> if it contains only whitespace character(s).
        /// </summary>
        /// <param name="argument">The string to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown <see cref="System.Exception"/>.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="argument"/> contains only whitespace character(s).
        /// </exception>
        public static void ArgumentHasText([ValidatedNotNullAttribute] string argument, string name, string message)
        {
            if (argument == null)
                throw new ArgumentNullException(name, message);

            if (StringUtils.IsEmpty(argument))
                throw new ArgumentException(message, name);
        }

        /// <summary>
        /// Checks the value of the supplied <see cref="ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentNullException"/> if it is <see langword="null"/>
        /// or <see cref="ArgumentException"/> if it contains no elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="argument"/> contains no elements.
        /// </exception>
        public static void ArgumentHasLength([ValidatedNotNullAttribute] ICollection argument, string name)
        {
            if (argument == null)
                throw new ArgumentNullException(name, ArgumentIsNullMessage(name));

            if (argument.Count == 0)
                throw new ArgumentException(ArgumentIsEmptyCollectionMessage(name), name);
        }

        /// <summary>
        /// Checks the value of the supplied <see cref="ICollection"/> <paramref name="argument"/> and throws
        /// an <see cref="ArgumentNullException"/> if it is <see langword="null"/>
        /// or <see cref="ArgumentException"/> if it contains no elements.
        /// </summary>
        /// <param name="argument">The array or collection to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">An arbitrary message that will be passed to any thrown <see cref="System.Exception"/>.</param>
        /// <exception cref="System.ArgumentNullException">
        /// If the supplied <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="argument"/> contains no elements.
        /// </exception>
        public static void ArgumentHasLength([ValidatedNotNullAttribute] ICollection argument, string name, string message)
        {
            if (argument == null)
                throw new ArgumentNullException(name, message);

            if (argument.Count == 0)
                throw new ArgumentException(message, name);
        }

        /// <summary>
        /// Checks whether the specified <paramref name="argument"/> can be cast 
        /// into the <paramref name="requiredType"/>.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <param name="requiredType">The required type for the argument.</param>
        /// <param name="argumentName">The name of the argument to check.</param>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="argument"/> is of a wrong type.
        /// </exception>
        public static void ArgumentHasType(object argument, Type requiredType, string argumentName)
        {
            if (argument != null && requiredType != null && !requiredType.IsAssignableFrom(argument.GetType()))
                throw new ArgumentException(ArgumentNotAnInstanceOfTypeMessage(argumentName), argumentName);
        }

        /// <summary>
        /// Checks whether the specified <paramref name="argument"/> can be cast 
        /// into the <paramref name="requiredType"/>.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <param name="requiredType">The required type for the argument.</param>
        /// <param name="argumentName">The name of the argument to check.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown <see cref="System.ArgumentException"/>.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="argument"/> is of a wrong type.
        /// </exception>
        public static void ArgumentHasType(object argument, Type requiredType, string argumentName, string message)
        {
            if (argument != null && requiredType != null && !requiredType.IsAssignableFrom(argument.GetType()))
            {
                throw new ArgumentException(message, argumentName);
            }
        }

        /// <summary>
        /// Checks whether the specified <paramref name="argument"/> can be cast 
        /// into the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The required type for the argument.</typeparam>
        /// <param name="argument">The argument to check.</param>
        /// <param name="argumentName">The name of the argument to check.</param>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="argument"/> is of a wrong type.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope
= "method", Target = "Refactorius.Commons.Collections", Justification =
"Argument type validating assertion method should be able to accept parameter of any type.")]
        public static void ArgumentHasType<T>(object argument, string argumentName)
        {
            if (argument != null && typeof(T) != null && !typeof(T).IsAssignableFrom(argument.GetType()))
                throw new ArgumentException(ArgumentNotAnInstanceOfTypeMessage(argumentName), argumentName);
        }

        /// <summary>
        /// Checks whether the specified <paramref name="argument"/> can be cast 
        /// into the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The required type for the argument.</typeparam>
        /// <param name="argument">The argument to check.</param>
        /// <param name="argumentName">The name of the argument to check.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown <see cref="System.ArgumentException"/>.
        /// </param>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="argument"/> is of a wrong type.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Scope
= "method", Target = "Refactorius.Commons.Collections", Justification =
"Argument type validating assertion method should be able to accept parameter of any type.")]
        public static void ArgumentHasType<T>(object argument, string argumentName, string message)
        {
            if (argument != null && typeof(T) != null && !typeof(T).IsAssignableFrom(argument.GetType()))
                throw new ArgumentException(message, argumentName);
        }

        /// <summary>
        /// Checks the value of the supplied <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentException"/> if it is the default(zero) value for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="argument"/> is the default(zero) value for <typeparamref name="T"/>.
        /// </exception>
        public static void ArgumentNotEmpty<T>(T argument, string name)
        {
            if (argument.Equals(default(T)))
                throw new ArgumentException(ArgumentHasDefaultValueMessage(name, typeof(T).FullName), name);
        }

        /// <summary>
        /// Checks the value of the supplied <paramref name="argument"/> and throws an
        /// <see cref="System.ArgumentException"/> if it is the default(zero) value for <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="argument">The object to check.</param>
        /// <param name="name">The argument name.</param>
        /// <param name="message">
        /// An arbitrary message that will be passed to any thrown
        /// <see cref="System.ArgumentException"/>.</param>
        /// <exception cref="System.ArgumentException">
        /// If the supplied <paramref name="argument"/> is the default(zero) value for <typeparamref name="T"/>.
        /// </exception>
        public static void ArgumentNotEmpty<T>(T argument, string name, string message)
        {
            if (argument.Equals(default(T)))
                throw new ArgumentException(message, name);
        }
#endif
    }
}
