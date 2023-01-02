using System.Reflection;
using System.Threading;

namespace Refactorius;

/// <summary>A static class containing some convenient extension methods for the <see cref="Exception"/> class.</summary>
[PublicAPI]
public static class ExceptionExtensions
{
    /// <summary>The backdoor way to imitate a critical exception with a <see cref="NotImplementedException"/> and this
    /// message.</summary>
    public const string NotImplementedCriticalMessage = "::CRITICAL";

    /// <summary>The key on the <see cref="Exception.Data"/> where the ErrorCode is stored.</summary>
    public const string ErrorCodeDataKey = "errorCode";

    private static readonly object _lock = new();

    private static readonly HashSet<Type> _criticalExceptionTypes = new()
    {
        typeof(OutOfMemoryException),
        typeof(InvalidProgramException),
        typeof(AppDomainUnloadedException),
        typeof(CannotUnloadAppDomainException)
    };

    private static readonly HashSet<Type> _outerExceptionTypes = new()
    {
        typeof(TargetInvocationException)
    };

    /// <summary>Registers a new outer exception type.</summary>
    /// <param name="exceptionType">The <c>Type</c> of an outer exception that should be skipped through.</param>
    /// <remarks>Outer exceptions can be skipped over in the log trace and in the rethrow.
    /// <para>We keep them in a hash because we need only to test for exact type equality.</para>
    /// </remarks>
    public static void RegisterOuterExceptionType(Type exceptionType)
    {
        exceptionType.MustNotBeNull(nameof(exceptionType));
        if (!typeof(Exception).IsAssignableFrom(exceptionType))
            throw new ArgumentException(exceptionType.FullName + " is not an Exception.", nameof(exceptionType));

        lock (_lock)
        {
            if (!_outerExceptionTypes.Contains(exceptionType))
                _outerExceptionTypes.Add(exceptionType);
        }
    }

    /// <summary>Registers new critical exception type.</summary>
    /// <param name="exceptionType">The <c>Type</c> of a critical exception.</param>
    /// <remarks>Critical exceptions are never swallowed by the framework and so bubble up to the topmost level
    /// unhindered.
    /// </remarks>
    public static void RegisterCriticalException(Type exceptionType)
    {
        exceptionType.MustNotBeNull(nameof(exceptionType));
        if (!typeof(Exception).IsAssignableFrom(exceptionType))
            throw new ArgumentException(exceptionType.FullName + " is not an Exception.", nameof(exceptionType));

        lock (_lock)
        {
            if (!_criticalExceptionTypes.Contains(exceptionType))
                _criticalExceptionTypes.Add(exceptionType);
        }
    }

    /// <summary>Tests if an <c>Type</c> is a critical exception. Critical exceptions are non-recoverable and must be
    /// re-thrown immediately.</summary>
    /// <param name="exceptionType">An <c>Type</c> to test.</param>
    /// <returns><see langword="true"/> if <paramref name="exceptionType"/> is registered as a non-recoverable
    /// <see cref="Exception"/>; <see langword="false"/> if <paramref name="exceptionType"/> is any other exception or
    /// <see langword="null"/>.</returns>
    internal static bool IsCriticalException(this Type exceptionType)
    {
        exceptionType.MustInherit<Exception>(nameof(exceptionType));
        return _criticalExceptionTypes.Any(exceptionType.IsInstanceOfType);
    }

    /// <summary>Tests if an <see cref="Exception"/> is a critical exception. Critical exceptions are non-recoverable and must
    /// be re-thrown immediately.</summary>
    /// <param name="ex">An <see cref="Exception"/> to test.</param>
    /// <returns><see langword="true"/> if <paramref name="ex"/> is one of <see cref="OutOfMemoryException"/>,
    /// <see cref="AppDomainUnloadedException"/>, <see cref="CannotUnloadAppDomainException"/> or
    /// <see cref="InvalidProgramException"/>; <see langword="false"/> if <paramref name="ex"/> is any other exception or
    /// <see langword="null"/>.</returns>
    /// <remarks><see cref="NotImplementedException"/> with message "::CRITICAL" is a backdoor way to imitate a critical
    /// exception.</remarks>
    [ContractAnnotation("ex:null => false")]
    public static bool IsCritical(this Exception? ex)
    {
        if (ex == null)
            return false;

        var type = ex.GetType();
        return type.IsCriticalException()
               || type.IsInstanceOfType(typeof(NotImplementedException)) &&
               ex.Message == NotImplementedCriticalMessage;
    }

    /// <summary>Tests if an <see cref="Exception"/> is a critical exception or a <see cref="ThreadAbortException"/>. Critical
    /// exceptions are non-recoverable and must be re-thrown immediately. <see cref="ThreadAbortException"/>  and
    /// <see cref="OperationCanceledException"/> may be handled in the <b>catch</b> clause, but they will be re-thrown anyway.</summary>
    /// <param name="ex">An <see cref="Exception"/> to test.</param>
    /// <returns><see langword="true"/> if <paramref name="ex"/> is one of <see cref="OutOfMemoryException"/>,
    /// <see cref="AppDomainUnloadedException"/>, <see cref="CannotUnloadAppDomainException"/>
    /// <see cref="InvalidProgramException"/> or <see cref="ThreadAbortException"/>; <see langword="false"/>
    /// if <paramref name="ex"/> is any other exception or <see langword="null"/>.</returns>
    [ContractAnnotation("ex:null => false")]
    public static bool ShouldRethrow(this Exception ex)
    {
        ex.MustNotBeNull(nameof(ex));

        return ex is OperationCanceledException || ex is ThreadAbortException || ex.IsCritical();
    }

    /// <summary>Recursively concatenates the message of the specified <see cref="Exception"/> and all its
    /// <see cref="Exception.InnerException"/> descendants.</summary>
    /// <param name="ex">An instance of the <see cref="Exception"/> or <see langword="null"/>.</param>
    /// <returns>The concatenation of messages of the specified <paramref name="ex"/> and all its
    /// <see cref="Exception.InnerException"/> descendants, separated by a newline. If <paramref name="ex"/> is
    /// <see langword="null"/>, <see langword="null"/> is returned.</returns>
    [ContractAnnotation("ex:null => null")]
    public static string? ConcatMessages(this Exception? ex)
    {
        if (ex == null)
            return null;

        var messages = new List<string> {ex.Message};
        for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            if (!(messages[messages.Count - 1] ?? string.Empty)
                .EndsWith(inner.Message, StringComparison.OrdinalIgnoreCase))
                messages.Add(inner.Message);

        return string.Join(Environment.NewLine, messages);
    }

    /// <summary>Extracts the single inner <see cref="Exception"/> from a <see cref="AggregateException"/>.</summary>
    /// <param name="ex">An <see cref="AggregateException"/> instance containing (hopefully) a single inner exception.</param>
    /// <returns>The inner exception of <paramref name="ex"/> if it's the only one, otherwise the <paramref name="ex"/> itself.</returns>
    public static Exception ExtractInnerException(this AggregateException ex)
    {
        ex.MustNotBeNull(nameof(ex));

        ex = ex.Flatten();
        return ex.InnerExceptions.Count == 1 
            ? ex.InnerExceptions[0].ExtractInnerException() 
            : ex;
    }

    /// <summary>Extracts the single inner <see cref="Exception"/> from a possibly skippable outer exception.</summary>
    /// <param name="ex">An outer exception containing (hopefully) an inner exception.</param>
    /// <returns>The inner exception of <paramref name="ex"/> if there is one, otherwise the <paramref name="ex"/> itself.</returns>
    /// <remarks>Used to minimize boilerplate code in catch clauses.</remarks>
    public static Exception ExtractInnerException(this Exception ex)
    {
        ex.MustNotBeNull(nameof(ex));

        while (true)
        {
            var inner = ex;
            if (ex is AggregateException)
                inner = ((AggregateException)inner).ExtractInnerException();
            else if (_outerExceptionTypes.Contains(ex.GetType())) // NB: exact type match
                inner = ex.InnerException ?? ex;
            if (inner == ex)
                return inner;
            ex = inner;
        }
    }

    #region ErrorCode support

    /// <summary>Gets out of <see cref="Exception.Data"/> the named item with a specified <c>Type</c>.</summary>
    /// <typeparam name="T">The <c>Type</c> of item to get.</typeparam>
    /// <param name="ex">The <see cref="Exception"/> containing the data.</param>
    /// <param name="name">The item name.</param>
    /// <returns></returns>
    public static T? GetData<T>(this Exception ex, string name)
    {
        ex.MustNotBeNull(nameof(ex));
        if (ex.Data.Contains(name))
            return (T?)ex.Data[name] ?? default;
        return default;
    }

    /// <summary>Adds to the <see cref="Exception.Data"/> a name-value pair.</summary>
    /// <param name="ex">The <see cref="Exception"/> containing the data.</param>
    /// <param name="name">The item name.</param>
    /// <param name="value">The item value.</param>
    /// <returns>The original <paramref name="ex"/> for call chaining.</returns>
    public static Exception SetData(this Exception ex, string name, object value)
    {
        ex.MustNotBeNull(nameof(ex));
        ex.Data[name] = value;
        return ex;
    }

    /// <summary>Gets the <b>ErrorCode</b> stored in the <see cref="Exception"/> instance.</summary>
    /// <param name="ex">The <see cref="Exception"/> containing the data.</param>
    /// <returns>The <b>ErrorCode</b> stored in the <paramref name="ex"/>.
    /// <para>NB: it is <b>NOT</b> the <see cref="Exception.HResult"/>!</para>
    /// </returns>
    /// <remarks>While I'm not a big fun of numeric error codes it seems to be the only way to reliably pass exception identity
    /// across language/system borders, esp. using REST api.</remarks>
    public static int GetErrorCode(this Exception ex)
    {
        return ex.GetData<int>(ErrorCodeDataKey);
    }

    /// <summary>Sets an <b>ErrorCOde</b> in the <see cref="Exception"/> instance.</summary>
    /// <param name="ex">The <see cref="Exception"/> containing the data.</param>
    /// <param name="errorCode">The value of <b>ErrorCode</b>.</param>
    /// <returns>The original <paramref name="ex"/> for call chaining.</returns>
    /// <remarks>While I'm not a big fun of numeric error codes it seems to be the only way to reliably pass exception identity
    /// across language/system borders, esp. using REST api.</remarks>
    public static Exception SetErrorCode(this Exception ex, int errorCode)
    {
        return ex.SetData(ErrorCodeDataKey, errorCode);
    }

    #endregion
}