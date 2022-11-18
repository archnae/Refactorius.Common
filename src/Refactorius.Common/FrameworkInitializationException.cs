using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Refactorius
{
    /// <summary>Exception thrown when an error is detected on the Framework initialization phase.</summary>
    [Serializable]
    public class FrameworkInitializationException : FrameworkException
    {
        /// <summary>Initializes static members of the <see cref="FrameworkInitializationException"/> class. Registers
        /// <see cref="FrameworkInitializationException"/> as a critical exception when the
        /// <see cref="FrameworkInitializationException"/> class is initialized.</summary>
        static FrameworkInitializationException()
        {
            ExceptionExtensions.RegisterCriticalException(typeof(FrameworkInitializationException));
        }

        /// <inheritdoc/>
        public FrameworkInitializationException()
        {
        }

        /// <inheritdoc/>
        public FrameworkInitializationException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public FrameworkInitializationException(string message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        protected FrameworkInitializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}