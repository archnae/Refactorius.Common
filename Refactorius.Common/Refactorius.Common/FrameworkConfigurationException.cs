using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Refactorius
{
    /// <summary>Exception thrown when an error is detected in the Framework configuration (initialization or run phase).</summary>
    [Serializable]
    public class FrameworkConfigurationException : FrameworkException
    {
        /// <summary>Initializes static members of the <see cref="FrameworkConfigurationException"/> class. Registers
        /// <see cref="FrameworkConfigurationException"/> as a critical exception when the
        /// <see cref="FrameworkConfigurationException"/> class is initialized.</summary>
        static FrameworkConfigurationException()
        {
            ExceptionExtensions.RegisterCriticalException(typeof(FrameworkConfigurationException));
        }

        /// <inheritdoc/>
        public FrameworkConfigurationException()
        {
        }

        /// <inheritdoc/>
        public FrameworkConfigurationException([NotNull] string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public FrameworkConfigurationException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        protected FrameworkConfigurationException([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}