using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Refactorius.Data
{
    /// <summary>The exception thrown when an error is detected in or while working with the entity data.</summary>
    /// <remarks>TODO: inherit from some standard .Net exception like System.Data.DataException someday.</remarks>
    [Serializable]
    public class FrameworkTestException : FrameworkException
    {
        /// <summary>The default error message for the <see cref="FrameworkTestException"/>.</summary>
        private const string DEFAULT_MESSAGE = "This exception is thrown as a part of a test.";

        #region constructors

        /// <summary>Initializes a new instance of the <see cref="FrameworkTestException"/> class with the default mesage.</summary>
        public FrameworkTestException()
            : base(DEFAULT_MESSAGE)
        {
        }

        /// <inheritdoc/>
        /// <summary>Initializes a new instance of the <see cref="Refactorius.Data.FrameworkTestException"/> class with a
        /// specified error message.</summary>
        /// <param name="message">The message.</param>
        public FrameworkTestException(string message)
            : this(message, null, false)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FrameworkTestException"/> class. with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/>
        /// reference if no inner exception is specified.</param>
        public FrameworkTestException(string message, Exception innerException)
            : base(message, innerException, false)
        {
        }

        /// <inheritdoc/>
        /// <summary>Initializes a new instance of the <see cref="Refactorius.Data.FrameworkTestException"/> class with a
        /// specified error message.</summary>
        /// <param name="message">The message.</param>
        /// <param name="ignoreDefaultMessage">A value specifying whether the default error message should NOT be appended to the
        /// <paramref name="message"/>.</param>
        public FrameworkTestException(string message, bool ignoreDefaultMessage)
            : this(message, null, ignoreDefaultMessage)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FrameworkTestException"/> class. with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/>
        /// reference if no inner exception is specified.</param>
        /// <param name="ignoreDefaultMessage">A value specifying whether the default error message should NOT be appended to the
        /// <paramref name="message"/>.</param>
        public FrameworkTestException(string message, Exception innerException, bool ignoreDefaultMessage)
            : base(message ?? (ignoreDefaultMessage ? string.Empty : "\n" + DEFAULT_MESSAGE), innerException)
        {
        }

        /// <inheritdoc/>
        protected FrameworkTestException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}