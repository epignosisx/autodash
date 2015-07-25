using System.Collections.Generic;
using System.Threading;

namespace Autodash.Core
{
    public class BoundedUpdatableCollection<T>
    {
        private readonly List<T> _list = new List<T>(5);
        private int _currentCapacity;

        public bool IsFull
        {
            get
            {
                lock (_list)
                {
                    return _list.Count >= _currentCapacity;
                }
            }
        }

        public void UpdateCapacity(int newCapacity)
        {
            Interlocked.Exchange(ref _currentCapacity, newCapacity);
        }

        public bool TryAdd(T item)
        {
            lock(_list)
            {
                if (_list.Count < _currentCapacity)
                {
                    _list.Add(item);
                    return true;
                }
            }
            return false;
        }

        public void Remove(T item)
        {
            lock(_list)
            {
                _list.Remove(item);
            }
        }

        public T[] ToArray()
        {
            lock(_list)
            {
                return _list.ToArray();
            }
        }
    }
}