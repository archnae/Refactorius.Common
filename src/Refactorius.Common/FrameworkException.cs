using System.Globalization;
using System.Runtime.Serialization;

namespace Refactorius;

/// <summary>The abstract base class for all domain-level exceptions raised when Refactorius framework code encounters a
/// problem.</summary>
[PublicAPI]
[Serializable]
public abstract class FrameworkException : Exception
{
    #region ErrorCode support

    /// <summary>Gets or sets the error code of this exception.</summary>
    /// <remarks>While I'm not a big fun of numeric error codes it seems to be the only way to reliably pass exception identity
    /// across language/system borders, esp. using REST api. Mapping errors into HTTP codes is a zoo filled with all sorts of
    /// diverse and fascinating manure.</remarks>
    public int ErrorCode
    {
        get => this.GetErrorCode();

        set => this.SetErrorCode(value);
    }

    #endregion

    #region constructors

    /// <inheritdoc/>
    protected FrameworkException()
    {
    }

    /// <inheritdoc/>
    protected FrameworkException(string message)
        : base(message)
    {
    }

    /// <inheritdoc/>
    protected FrameworkException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc/>
    // TODO: add xmldoc
    protected FrameworkException(string message, Exception? innerException, params object?[] args)
        : base(message.SafeFormat(CultureInfo.InvariantCulture, args), innerException)
    {
    }

    /// <inheritdoc/>
    // TODO: add xmldoc
    protected FrameworkException(string message, params object?[] args)
        : base(message.SafeFormat(CultureInfo.InvariantCulture, args))
    {
    }

    /// <inheritdoc/>
    protected FrameworkException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    #endregion
}