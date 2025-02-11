﻿using System.Globalization;
using System.Threading;

namespace Refactorius;

/// <summary>The base class for context-like disposable objects to be used in a thread-owned context stack.</summary>
/// <typeparam name="TInstance">The type of the descendent class itself.</typeparam>
[PublicAPI]
public abstract class AbstractContext<TInstance> : IDisposable
    where TInstance : AbstractContext<TInstance>
{
    private static readonly AsyncLocal<Stack<TInstance>> _stack = new();

    /// <summary>Initializes a new instance of the <see cref="AbstractContext{TInstance}"/> class and registers it as the
    /// current context.</summary>
    protected AbstractContext()
    {
        Register((TInstance)this);
    }

    /// <summary>Gets the current context stack.</summary>
    /// <value>The current context stack.</value>
    public static Stack<TInstance> CurrentStack
    {
        get { return _stack.Value ??= new Stack<TInstance>(); }
    }

    /// <summary>Gets the latest registered scope.</summary>
    /// <value>The current (latest registered) scope.</value>
    public static TInstance Current
    {
        get
        {
            if (!HasCurrent)
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The context stack '{0}' is empty.",
                        _stack.GetType().FullName));

            return CurrentStack.Peek();
        }
    }

    /// <summary>Gets a value indicating whether the active request or thread has initialized scope.</summary>
    /// <value><see langword="true"/> if the active http request or thread has initialized scope; otherwise,
    /// <see langword="false"/>.</value>
    public static bool HasCurrent => CurrentStack.Count > 0;

    /// <summary>Gets or sets a value indicating whether the current <see cref="AbstractContext{TInstance}"/> was disposed.</summary>
    /// <value><see langword="true"/> if already disposed, otherwise <see langword="false"/>.</value>
    protected bool IsDisposed { get; set; }

    /// <summary>Registers the scope.</summary>
    /// <param name="scope">The scope.</param>
    public static void Register(TInstance scope)
    {
        scope.MustNotBeNull(nameof(scope));

        CurrentStack.Push(scope);
    }

    /// <summary>Un-registers the scope.</summary>
    /// <param name="scope">The scope.</param>
    internal static void Unregister(TInstance scope)
    {
        scope.MustNotBeNull(nameof(scope));
        Guard.Assert(
            scope == CurrentStack.Peek(),
            string.Format(
                CultureInfo.InvariantCulture,
                "Cannot unregister the current instance of '{0}' - the instance is not on the top or the context stack is empty.",
                scope.GetType().FullName));

        CurrentStack.Pop();
    }

    #region logging

    ////private static readonly ILog Log = LogManager.GetLogger(typeof(TInstance));

    #endregion

    #region IDisposable implementation

    /// <summary>Closes current <see cref="AbstractContext{TInstance}"/>.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Performs garbage collection of the current <see cref="AbstractContext{TInstance}"/> (closes it).</summary>
    /// <param name="disposing">Indicates whether the method is called during deterministic garbage collection.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !IsDisposed)
        {
            Unregister((TInstance)this);
            IsDisposed = true;
            ////if (Log.IsVerboseEnabled())
            ////    Log.VerboseFormat("closing context {0}", this);
        }
    }

    #endregion
}