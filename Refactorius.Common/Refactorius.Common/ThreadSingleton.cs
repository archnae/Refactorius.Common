using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Refactorius
{
    /// <summary>The thread singleton accessor for the type <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The interface <see cref="Type"/>, exposing the singleton.</typeparam>
    /// <remarks>
    /// <para>The most awful piece of code. Needs to be redone someday.</para>
    /// <para>The thread sigleton implements the "instance-per-thread" pattern.</para>
    /// <para>There is only one instance of type <typeparamref name="T"/> per thread for every given <b>ThreadSingleton</b>
    /// type, but every thread has its own instance. The instance is stored in thread local storage or http context.</para>
    /// <para>To have several singletons derived from the same base class, first subclass the base class:</para>
    /// <para>
    /// <code>
    /// class Base
    /// {
    ///     public void DoIt() {}
    /// }
    /// class FirstSingletonType: Base {}
    /// class SecondSingletonType: Base {}
    /// class FirstSingleton: ThreadSingleton&lt;FirstSingletonType&gt; {}
    /// class SecondSingleton: ThreadSingleton&lt;SecondSingletonType&gt; {}
    /// ...
    /// FirstSingleton.DoIt();
    /// SecondSingleton.DoIt();
    /// </code>
    /// </para>
    /// </remarks>
    public abstract class ThreadSingleton<T>
        where T : class
    {
        #region private fields

        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once InconsistentNaming

        [ThreadStatic] private static T _instance;

        #endregion

        #region protected constructor

        /// <summary>Initializes a new instance of the <see cref="ThreadSingleton{T}"/> class. Always throws an exception, as this
        /// class is not supposed to be instantiated.</summary>
        /// <exception cref="InvalidOperationException">new instance of <see cref="ThreadSingleton{T}"/> is created.</exception>
        /// <remarks>This class needs a protected constructor to be inheritable.</remarks>
        protected ThreadSingleton()
        {
            throw new InvalidOperationException("ThreadSingleton class is not supposed to be instantiated.");
        }

        #endregion

        #region public static methods

        /// <summary>Sets the singleton instance.</summary>
        /// <param name="value">The thread singleton instance of <typeparamref name="T"/>.</param>
        public static void SetInstance([NotNull] T value)
        {
            _instance = value;
        }

        #endregion

        #region private methods

        /// <summary>Gets the singleton instance, creating it if necessary.</summary>
        /// <param name="force">Specifies if an instance is created if it wasn't created before.</param>
        /// <returns>The thread singleton instance of <typeparamref name="T"/>.</returns>
        [ContractAnnotation("force:true => notnull")]
        protected static T GetSingletonInstance(bool force)
        {
            if (_instance == null && force)
            {
                if (InstanceCreator != null)
                    lock (Lock)
                    {
                        if (GetSingletonInstance(false) == null)
                            _instance = InstanceCreator();
                    }

                if (_instance == null)
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "ThreadSingleton<{0}> has no initialized instance. If you want auto-initialization, use ThreadAutoSingleton<{0}> class instead.",
                            typeof(T).FullName));
            }

            return _instance;
        }

        #endregion

        #region public static properties

        /// <summary>Gets the lock object to synchronize access to the thread singleton.</summary>
        /// <value>The lock <see langword="object"/> to synchronize access to the thread singleton.</value>
        [NotNull]
        // ReSharper disable once StaticMemberInGenericType
        public static object Lock { get; } = new object();

        /// <summary>Gets a value indicating whether the thread singleton instance of <b>T</b> already exists.</summary>
        /// <value><see langword="true"/>, if the instance of the thread singleton already exists; <see langword="false"/>
        /// otherwise.</value>
        /// <seealso cref="Instance"/>
        public static bool HasInstance => _instance != null;

        /// <summary>Gets the thread singleton instance of <b>T</b>.</summary>
        /// <value>The thread singleton instance of <b>T</b> exposed as <b>TI</b>.</value>
        public static T Instance => _instance;

        /// <summary>Gets or sets the instance-creating delegate.</summary>
        /// <value>The instance-creating delegate.</value>
        [CLSCompliant(false)]
        protected static Func<T> InstanceCreator { get; set; }

        #endregion
    }

    /// <summary>The thread singleton accessor for the interface <typeparamref name="TI"/>, implemented by the type
    /// <typeparamref name="T"/>.</summary>
    /// <typeparam name="TI">The interface <see cref="Type"/>, exposing the singleton.</typeparam>
    /// <typeparam name="T">The implementing <see cref="Type"/> of the singleton.</typeparam>
    /// <remarks>
    /// <para>The thread sigleton implements the "instance-per-thread" pattern.</para>
    /// <para>There is only one instance of type <typeparamref name="TI"/> per thread for every given
    /// <b>ThreadAutoSingleton</b> type, but every thread has its own instance. The instance is stored in thread local storage
    /// or http context.</para>
    /// <para>The instance is created on demand through its default constructor.</para>
    /// <para>To have several singletons derived from the same base class, first subclass the base class:</para>
    /// <para>
    /// <code>
    /// interface IBase
    /// {
    ///     public void DoIt() {}
    /// }
    /// class Base : IBase {}
    /// class FirstSingletonType: Base {}
    /// class SecondSingletonType: Base {}
    /// class FirstSingleton: ThreadAutoSingleton&lt;IBase, FirstSingletonType&gt; {}
    /// class SecondSingleton: ThreadAutoSingleton&lt;IBase, SecondSingletonType&gt; {}
    /// ...
    /// FirstSingleton.DoIt();
    /// SecondSingleton.DoIt()
    /// </code>
    /// </para>
    /// </remarks>
    public abstract class ThreadAutoSingleton<TI, T> : ThreadSingleton<TI>
        where T : TI, new()
        where TI : class
    {
        /// <summary>Initializes static members of the <see cref="ThreadAutoSingleton{TI,T}"/> class.</summary>
        static ThreadAutoSingleton()
        {
            InstanceCreator = CreateInstance;
        }

        /// <summary>Gets the thread singleton instance of <b>T</b>.</summary>
        /// <value>The thread singleton instance of <b>T</b> exposed as <b>TI</b>. The instance of <b>T</b> is created on the first
        /// access to this property.</value>
        [NotNull]
        public new static TI Instance => GetSingletonInstance(true);

        /// <summary>Creates a singleton instance of <typeparamref name="T"/> upon request. Override this method in inherited class
        /// if you need a create-on-demand behaviour.</summary>
        /// <returns>A  newly created singleton instance of <typeparamref name="T"/>, implementing <typeparamref name="TI"/>.</returns>
        private static TI CreateInstance()
        {
            return new T();
        }
    }

    /// <summary>The thread singleton accessor for the type <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The <see cref="Type"/> of the singleton.</typeparam>
    /// <seealso cref="ThreadSingleton&lt;T&gt;"/>
    /// <remarks>The sigleton is stored in thread local storage or http context.</remarks>
    public abstract class ThreadAutoSingleton<T> : ThreadAutoSingleton<T, T>
        where T : class, new()
    {
    }
}