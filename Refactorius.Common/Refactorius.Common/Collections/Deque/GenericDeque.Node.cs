using System;

namespace Refactorius.Collections
{
    /// <content>Represents a node in the deque.</content>
    public partial class Deque<T>
    {
        #region Node Class

        // Represents a node in the deque.
        [Serializable]
        private class Node
        {
            private Node _next;
            private Node _previous;

            public Node(T value)
            {
                Value = value;
            }

            public T Value { get; }

            public Node Previous
            {
                get { return _previous; }
                set { _previous = value; }
            }

            public Node Next
            {
                get { return _next; }
                set { _next = value; }
            }
        }

        #endregion
    }
}