using System.Runtime.CompilerServices;

namespace Refactorius;
#pragma warning disable CA1000 // Do not declare static members on generic types

/// <summary>
/// The base Mixin class to add <typeparamref name="TValue"/> extensions to <typeparamref name="TKey"/> instances.
/// </summary>
/// <typeparam name="TKey">The class to extend.</typeparam>
/// <typeparam name="TValue">The class to extend with.</typeparam>
/// <remarks>This is a simple wrapper around <b>ConditionalWeakTable</b> to hide the technical details from the client code.</remarks>
public class MixinBase<TKey, TValue>
    where TKey: class
    where TValue : class
{
    #region private fields
    private static readonly ConditionalWeakTable<TKey, MixinBase<TKey, TValue>> _mixinMap = new();
    private readonly WeakReference<TKey> _key;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="MixinBase{TOwner, TPayload}"/> mixin.
    /// </summary>
    /// <param name="key">The owning <typeparamref name="TKey"/> instance.</param>
    /// <param name="value">The extending <typeparamref name="TValue"/> instance.</param>
    /// <exception cref="ArgumentNullException">if <paramref name="key"/> or <paramref name="value"/> is <see langword="null"/>.</exception>
    protected MixinBase(TKey key, TValue value)
    {
        key.MustNotBeNull(nameof(key));
        value.MustNotBeNull(nameof(value));

        _key = new WeakReference<TKey>(key);
        Value = value;
    }

    #region public methods

    /// <summary>
    /// Gets the existing or newly created <see cref="MixinBase{TKey, TValue}"/> instance for the specified owning <typeparamref name="TKey"/> instance.
    /// </summary>
    /// <param name="key">The owning <typeparamref name="TKey"/> instance.</param>
    /// <param name="createValueCallback">The <typeparamref name="TValue"/> creation delegate.</param>
    /// <returns>The existing or newly created <see cref="MixinBase{TKey, TValue}"/> instance.</returns>
    public static MixinBase<TKey, TValue> Get(TKey key, Func<TKey, TValue> createValueCallback)
        => _mixinMap.GetValue(
                key.MustNotBeNull(nameof(key)),
                k => new MixinBase<TKey, TValue>(k, createValueCallback(k)))
            .CheckDisposed();

    /// <summary>
    /// Gets the existing or newly created <typeparamref name="TValue"/> instance for the specified owning <typeparamref name="TKey"/> instance.
    /// </summary>
    /// <param name="key">The owning <typeparamref name="TKey"/> instance.</param>
    /// <param name="createValueCallback">The <typeparamref name="TValue"/> creation delegate.</param>
    /// <returns>The existing or newly created extending <typeparamref name="TValue"/> instance.</returns>
    public static TValue GetValue(TKey key, Func<TKey, TValue> createValueCallback) 
        => Get(key, createValueCallback).Value;

    /// <summary>
    /// Adds a key to the table.
    /// </summary>
    /// <param name="key">The key to add. <paramref name="key"/> represents the object to which the property is attached.</param>
    /// <param name="value">The key's property value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="key"/> already exists.</exception>
    public static void Add(TKey key, TValue value)
    {
        _mixinMap.Add(key, new MixinBase<TKey, TValue>(key, value));
    }

    /// <summary>
    /// Gets the value of the specified key.
    /// </summary>
    /// <param name="key">The key that represents an object with an attached property.</param>
    /// <param name="value">When this method returns, contains the attached property value.
    ///  If <paramref name="key"/> is not found, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if found, otherwise <see langword="false"/>.</returns>
    public static bool TryGetValue(TKey key, out TValue? value)
    {
        value = default;
        if (_mixinMap.TryGetValue(key, out var mixin))
        {
            value = mixin.Value;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the <typeparamref name="TKey"/> instance this mixin is extending.
    /// </summary>
    public TKey Key => CheckDisposed().Key;

    /// <summary>
    /// Gets the <typeparamref name="TValue"/> extension instance.
    /// </summary>
    public TValue Value { get; }
    #endregion

    #region private methods
    private MixinBase<TKey, TValue> CheckDisposed()
    {
        if (!_key.TryGetTarget(out _))
            throw new ObjectDisposedException(GetType().DisplayName(), $"Owning {typeof(TKey).DisplayName()} instance is already disposed.");

        return this;
    }
    #endregion
}