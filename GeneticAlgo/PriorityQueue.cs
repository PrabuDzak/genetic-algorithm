using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriorityQueue
{
    public sealed class PriorityQueue<T> : IEnumerable<T> where T : IPriorityQueueItem<T>
    {
        private T[] items;
        public int Count { private set; get; }
        public int Capacity { private set; get; }

        private int enqueuedItemCounter = 0;

        public PriorityQueue(int capacity)
        {
            this.items = new T[capacity];
            this.Count = 0;
            this.Capacity = capacity;
        }

        public PriorityQueue(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Capacity++;
            }

            this.items = new T[Capacity];
            this.Count = 0;

            foreach (var item in collection)
            {
                Enqueue(item);
            }
        }

        public void Enqueue(T item)
        {
            if (Count < Capacity)
            {
                items[Count] = item;
                items[Count].incomingIndex = ++enqueuedItemCounter;
                StepUp(Count);
                Count++;
            }
            else
            {
                // D-D-D-ROP 
            }
        }

        public T Pop()
        {
            if (Count <= 0) return default(T);

            Swap(ref items[0], ref items[Count - 1]);
            Count--;
            StepDown(0);
            return items[Count];
        }

        public T Peek()
        {
            return items[0];
        }

        private void StepUp(int currentIndex)
        {
            while (currentIndex > 0)
            {
                int parent = (currentIndex - 1) / 2;
                if (items[currentIndex].CompareTo(items[parent]) > 0)
                {
                    Swap(ref items[currentIndex], ref items[parent]);
                    currentIndex = parent;
                }
                else
                {
                    break;
                }
            }
        }

        private void StepDown(int currentIndex)
        {
            while (true)
            {
                int leftChild = currentIndex * 2 + 1;
                int rightChild = currentIndex * 2 + 2;

                if (leftChild < Count)
                {
                    int selectedChild = leftChild;

                    if (rightChild < Count)
                    {
                        if (items[rightChild].CompareTo(items[leftChild]) > 0)
                        {
                            selectedChild = rightChild;
                        }
                    }

                    if (items[selectedChild].CompareTo(items[currentIndex]) > 0)
                    {
                        Swap(ref items[currentIndex], ref items[selectedChild]);
                        currentIndex = selectedChild;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void Swap(ref T A, ref T B)
        {
            T temp = A;
            A = B;
            B = temp;
        }

        public IEnumerator<T> GetEnumerator()
        {
            T[] temp = new T[Count];
            for (int i = 0; i < Count; i++)
            {
                temp[i] = items[i];
            }
            return ((IEnumerable<T>)temp).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            T[] temp = new T[Count];
            for (int i = 0; i < Count; i++)
            {
                temp[i] = items[i];
            }
            return ((IEnumerable<T>)temp).GetEnumerator();
        }
    }

    public interface IPriorityQueueItem<T> : IComparable<T>
    {
        int incomingIndex { set; get; }
    }
}
