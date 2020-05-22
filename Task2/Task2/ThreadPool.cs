using System;
using System.Collections.Generic;
using System.Threading;

namespace Task2
{
    public class ThreadPool : IDisposable
    {
        private List<Thread> Threads;
        private Dictionary<Thread, Queue> Queues;
        private int NextQueue;
        private bool Working;
        private bool Disposed;

        public ThreadPool(uint threads, uint queuesSize = 20)
        {
            Working = false;
            Disposed = false;
            NextQueue = 0;
            Threads = new List<Thread>();
            Queues = new Dictionary<Thread, Queue>();
            for (int i = 0; i < threads; i++)
            {
                Thread thread = new Thread(Work);
                thread.Name = "Pool Thread " + i;
                Threads.Add(thread);
                Queues.Add(thread, new Queue(queuesSize));
            }
        }

        public void EnqueueToOther(ITask task, Thread thread)
        {
            Queues[thread].Push(task);
        }

        public void Enqueue<TResult>(ITask<TResult> task)
        {
            if (Threads.Count == 0)
            {
                return;
            }
            Queues[Threads[NextQueue]].Push(task);
            NextQueue = (NextQueue + 1) % Threads.Count;
        }

        public void Work()
        {
            Random random = new Random();
            while(Working)
            {
                bool isStolen = false;
                ITask task = Queues[Thread.CurrentThread].GetFromTail();
                if (task == null)
                {
                    Thread toSteal = Threads[random.Next(0, Threads.Count)];
                    if (!Queues[toSteal].IsEmpty())
                    {
                        task = Queues[toSteal].GetFromHead();
                        isStolen = true;
                    }
                }
                if (task == null)
                {
                    Thread.Sleep(1);
                }
                else
                {
                    if (isStolen || task.CanBeExecuted())
                    {
                        task.Execute();
                    }
                    else
                    {
                        EnqueueToOther(task, Threads[random.Next(0, Threads.Count)]);
                        Thread.Sleep(1);
                    }
                }
            }
        }

        public void Start()
        {
            if (Working || Disposed)
            {
                return;
            }
            Working = true;
            foreach (Thread thread in Threads)
            {
                thread.Start();
            }
        }

        public void Finish()
        {
            if (!Working || Disposed)
            {
                return;
            }
            Working = false;
            foreach (Thread thread in Threads)
            {
                thread.Join();
            }
        }

        ~ThreadPool()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                Finish();
                Threads.Clear();
                Queues.Clear();
            }
            Disposed = true;
            GC.SuppressFinalize(this);
        }

        public int RemainingTasks()
        {
            int count = 0;
            foreach (KeyValuePair<Thread, Queue> pair in Queues)
            {
                count += pair.Value.RemainingTasks();
            }
            return count;
        }
    }
}
