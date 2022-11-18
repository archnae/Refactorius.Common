using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace Refactorius.Collections
{
    /// <summary>Concurrent thread-safe wrapper around <see cref="List{T}"/></summary>
    /// <typeparam name="T">The <c>Type</c> of list items.</typeparam>
    public class ConcurrentList<T> : IList<T>, IDisposable
    {
        #region private fields

        private readonly List<T> _list;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        #endregion

        #region constructors

        /// <summary>Initializes a new instance of the <see cref="ConcurrentList{T}"/> class.</summary>
        public ConcurrentList()
        {
            _list = new List<T>();
        }

        /// <summary>Initializes a new instance of the <see cref="ConcurrentList{T}"/> class woith specified capacity.</summary>
        public ConcurrentList(int capacity)
        {
            _list = new List<T>(capacity);
        }

        /// <summary>Initializes a new instance of the <see cref="ConcurrentList{T}"/> class with a specifiried content.</summary>
        public ConcurrentList(IEnumerable<T> items)
        {
            _list = new List<T>(items);
        }

        #endregion

        #region public properties

        /// <inheritdoc/>
        public T this[int index]
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();
                    return _list[index];
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }

            set
            {
                try
                {
                    _lock.EnterWriteLock();
                    _list[index] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                try
                {
                    _lock.EnterReadLock();
                    return _list.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        #endregion

        #region public methods

        /// <inheritdoc/>
        public void Add(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                _list.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            try
            {
                _lock.EnterWriteLock();
                _list.Insert(index, item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                return _list.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            try
            {
                _lock.EnterWriteLock();
                _list.RemoveAt(index);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                return _list.IndexOf(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            try
            {
                _lock.EnterWriteLock();
                _list.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                return _list.Contains(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            try
            {
                _lock.EnterWriteLock();
                _list.CopyTo(array, arrayIndex);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        #endregion

        #region IEnumerable implementation

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            // very, VERY quick & dirty, for better attempt see http://stackoverflow.com/questions/6601611/no-concurrentlistt-in-net-4-0
            // NB: concurrent enumerators are Bad-By-Design anyway
            try
            {
                // TODO: share this r/o copy between all threads, clear it on all update ops
                _lock.EnterReadLock();
                var copy = new List<T>(_list);
                return copy.GetEnumerator();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IDisposable implementation

        /// <summary>Finalizes an instance of the <see cref="ConcurrentList{T}"/> class.</summary>
        ~ConcurrentList()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
            _lock.Dispose();
        }

        #endregion
    }
}