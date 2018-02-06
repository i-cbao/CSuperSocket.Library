using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dynamic.Core
{
    public class ParallelExecutor<TEntity,TResult>
    {
        public ParallelQueue<TEntity> Queue
        {
            get;
            private set
            ;
        }

        public int ThreadCount { get; private set; }

        private int actualThreadCount = 0;

        public Func<TEntity,TResult> Processor { get; private set; }

        public ParallelExecutor(int threadCount, Func<TEntity, TResult> processor)
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

        List<ThreadItem<TResult>> threadList = null;
        public List<TResult> Start()
        {
            actualThreadCount = ThreadCount;
            if (actualThreadCount > Queue.Count)
            {
                actualThreadCount = Queue.Count;
            }
            
            threadList = new List<ThreadItem<TResult>>();
            
            for (int i = 0; i < actualThreadCount; i++)
            {
                ThreadItem<TResult> item = new ThreadItem<TResult>();
                item.Result = new List<TResult>();
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

            List<TResult> r = new List<TResult>();

            threadList.ForEach(t =>
            {
                r.AddRange(t.Result);
            });


            return r;
        }

        private void proc(object state)
        {
            ThreadItem<TResult> item = state as ThreadItem<TResult>;
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

                TResult r =  Processor(entity);
                item.Result.Add(r);

                item.ProcessedCount++;
            }
            item.EndTime = DateTime.Now;
            item.waiter.Set();
           
        }

        public class ThreadItem<TResult> 
        {
            public int Index { get; set; }
            public Thread Thread { get; set; }
            public  ManualResetEvent waiter { get; set; }
            public int ProcessedCount { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }

            public List<TResult> Result { get; set; }

            public bool IsCanceled { get; set; }
        }
    }
}
