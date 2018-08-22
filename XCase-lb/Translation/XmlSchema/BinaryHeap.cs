using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XCase.Translation.XmlSchema
{
    public class BinaryHeap<T> : ICollection<T> where T : IComparable<T>
    {
        private const int DEFAULT_SIZE = 8;

        private T[] _data = new T[DEFAULT_SIZE];
        private int _count = 0;
        private int _capacity = DEFAULT_SIZE;
        private bool _sorted;
        
        public int Count
        {
            get { return _count; }
        }
        public int Capacity
        {
            get { return _capacity; }
            set
            {
                int previousCapacity = _capacity;
                _capacity = Math.Max(value, _count);
                if (_capacity != previousCapacity)
                {
                    T[] temp = new T[_capacity];
                    Array.Copy(_data, temp, _count);
                    _data = temp;
                }
            }
        }

        public BinaryHeap()
        {
        }
        private BinaryHeap(T[] data, int count)
        {
            Capacity = data.Length;
            _count = count;
            Array.Copy(data, _data, count);
        }

        public void Add(T item)
        {
            if (_count == _capacity)
            {
                Capacity *= 2;
            }
            _data[_count] = item;
            UpHeap();
            _count++;
        }

        public T Peek()
        {
            return _data[0];
        }

        public T RemovePeek()
        {
            if (this._count == 0)
            {
                throw new InvalidOperationException("Cannot remove item, heap is empty.");
            }
            T v = _data[0];
            _count--;
            _data[0] = _data[_count];
            _data[_count] = default(T); //Clears the Last Node
            DownHeap();
            return v;
        }

        public BinaryHeap<T> Copy()
        {
            return new BinaryHeap<T>(_data, _count);
        }

        public bool CompareItems(BinaryHeap<T> heap)
        {
            BinaryHeap<T> heapCopy = this.Copy();
            BinaryHeap<T> heapCopy2 = heap.Copy();

            while (heapCopy.Count > 0 && heapCopy2.Count > 0)
            {
                T peek1 = heapCopy.RemovePeek();
                T peek2 = heapCopy2.RemovePeek();

                if (peek1.CompareTo(peek2) != 0)
                    return false;
            }
            if (heapCopy.Count > 0 || heapCopy2.Count > 0)
                return false;
            return true;
        }
        
        #region helpers
        private void UpHeap()
        {
            _sorted = false;
            int p = _count;
            T item = _data[p];
            int par = Parent(p);
            while (par > -1 && item.CompareTo(_data[par]) < 0)
            {
                _data[p] = _data[par]; //Swap nodes
                p = par;
                par = Parent(p);
            }
            _data[p] = item;
        }
        private void DownHeap()
        {
            _sorted = false;
            int n;
            int p = 0;
            T item = _data[p];
            while (true)
            {
                int ch1 = Child1(p);
                if (ch1 >= _count) break;
                int ch2 = Child2(p);
                if (ch2 >= _count)
                {
                    n = ch1;
                }
                else
                {
                    n = _data[ch1].CompareTo(_data[ch2]) < 0 ? ch1 : ch2;
                }
                if (item.CompareTo(_data[n]) > 0)
                {
                    _data[p] = _data[n]; //Swap nodes
                    p = n;
                }
                else
                {
                    break;
                }
            }
            _data[p] = item;
        }
        private static int Parent(int index)
        {
            return (index - 1) >> 1;
        }
        private static int Child1(int index)
        {
            return (index << 1) + 1;
        }
        private static int Child2(int index)
        {
            return (index << 1) + 2;
        }
        #endregion

        #region required by ICollection<T>
        private void EnsureSort()
        {
            if (_sorted) return;
            Array.Sort(_data, 0, _count);
            _sorted = true;
        }

        public bool Remove(T item)
        {
            EnsureSort();
            int i = Array.BinarySearch<T>(_data, 0, _count, item);
            if (i < 0) return false;
            Array.Copy(_data, i + 1, _data, i, _count - i);
            _data[_count] = default(T);
            _count--;
            return true;
        }

        public void Clear()
        {
            this._count = 0;
            _data = new T[_capacity];
        }

        public bool Contains(T item)
        {
            EnsureSort();
            return Array.BinarySearch<T>(_data, 0, _count, item) >= 0;
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            EnsureSort();
            Array.Copy(_data, array, _count);
        }
        #endregion

        #region required by IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            EnsureSort();
            for (int i = 0; i < _count; i++)
            {
                yield return _data[i];
            }
        }
        #endregion
    }
}
