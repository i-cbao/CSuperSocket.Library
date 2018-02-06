using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dynamic.Core
{
    public class ParallelExecutor<TEntity>
    {
        public ParallelQueue<TEntity> Queue
        {
            get;
            private set
            ;
        }

        public int ThreadCount { get; private set; }

        private int actualThreadCount = 0;

        public Action<TEntity> Processor { get; private set; }

        public ParallelExecutor(int threadCount, Action<TEntity> processor)
        {
            if (threadCount == -1)
            {
                threadCount = Environment.ProcessorCount;
                if (threadCount == 1)
                {
                    threadCount = 4;
                }
            }
            ThreadCount = threadCount;
            Processor = processor;
            Queue = new ParallelQueue<TEntity>();
           
            
        }


        List<ThreadItem> threadList = null;
        public List<ThreadItem> Start()
        {
            actualThreadCount = ThreadCount;
            if (actualThreadCount > Queue.Count)
            {
                actualThreadCount = Queue.Count;
            }
            
            threadList = new List<ThreadItem>();
            
            for (int i = 0; i < actualThreadCount; i++)
            {
                ThreadItem item = new ThreadItem();
                item.Index = i;
                item.waiter = new ManualResetEvent(false);
                Thread thread = new Thread(proc);
                thread.IsBackground = true;
                thread.Name = "处理线程_"+ i.ToString();
                item.Thread = thread;
                item.IsCanceled =false;

                threadList.Add(item);
            }

            for (int i = 0; i < actualThreadCount; i++)
            {
                threadList[i].Thread.Start(threadList[i]);
            }

            WaitHandle.WaitAll(threadList.Select(x => x.waiter).ToArray());

            return threadList.ToList();
        }

        private void proc(object state)
        {
            ThreadItem item = state as ThreadItem;
            item.StartTime = DateTime.Now;
            while (true)
            {
                if (item.IsCanceled)
                {
                    break;
                }

                TEntity entity = Queue.Dequeue();
                if (entity == null)
                {
                    break;
                }

                Processor(entity);

                item.ProcessedCount++;
            }
            item.EndTime = DateTime.Now;
            item.waiter.Set();
           
        }

        public class ThreadItem
        {
            public int Index { get; set; }
            public Thread Thread { get; set; }
            public  ManualResetEvent waiter { get; set; }
            public int ProcessedCount { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public bool IsCanceled { get; set; }
        }
    }
}
