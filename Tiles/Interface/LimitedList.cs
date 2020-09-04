using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CoreTiles.Tiles
{
    //todo move to useful.net
    public class LimitedList<T> : IEnumerable<T>, IEnumerable, ICollection, IReadOnlyCollection<T>
    {
        private List<T> internalList;
        private int maxItems;
        public LimitedList(int capacity)
        {
            internalList = new List<T>(capacity);
            maxItems = capacity;
        }

        public int Count
        {
            get
            {
                lock (SyncRoot)
                {
                    return internalList.Count;
                }
            }
        }

        public bool IsSynchronized => true;

        public object SyncRoot { get; } = new object();

        public void CopyTo(Array array, int index)
        {
            lock (SyncRoot)
            {
                var idx = index;
                foreach (var item in internalList)
                {
                    array.SetValue(item, idx);
                    idx++;
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        public bool TryAdd(T item)
        {
            lock (SyncRoot)
            {
                if (internalList.Count == maxItems)
                {
                    internalList.RemoveAt(0);
                }
                internalList.Add(item);
                return true;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return internalList.GetEnumerator();
        }
    }
}
