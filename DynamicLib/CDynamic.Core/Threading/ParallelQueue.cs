using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Core
{
    public class ParallelQueue<T>
    {
        public object SyncLocker { get; private set; }

        public Queue<T> Queue { get; private set; }


        public ParallelQueue()
        {
            SyncLocker = new object();
            Queue = new Queue<T>();
        }

        public void Enqueue(T entity)
        {
            lock (SyncLocker)
            {
                Queue.Enqueue(entity);
            }
        }

        public void Enqueue(IEnumerable<T> entityList)
        {
            if (entityList == null)
                return;

            lock (SyncLocker)
            {
                foreach (T entity in entityList)
                {
                    Queue.Enqueue(entity);
                }
            }
        }

        public T Dequeue()
        {
            T entity = default(T);
            lock (SyncLocker)
            {
                if (Queue.Count > 0)
                {
                    entity = Queue.Dequeue();
                }
            }

            return entity;
        }

        public T Dequeue(out bool isLast)
        {
            isLast = false;
            T entity = default(T);
            lock (SyncLocker)
            {
                if (Queue.Count > 0)
                {
                    entity = Queue.Dequeue();
                }
                if (Queue.Count == 0)
                {
                    isLast = true;
                }
            }

            return entity;
        }

        public bool IsEmpty()
        {
            bool empty = false;
            lock (SyncLocker)
            {
                if (Queue.Count == 0)
                {
                    empty = true;
                }

            }

            return true;
        }

        public int Count
        {
            get
            {
                int c = 0;
                lock (SyncLocker)
                {
                    c = Queue.Count;
                }

                return c;
            }
        }
    }
}
