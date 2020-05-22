using System;
using System.Collections.Generic;
using System.Threading;

namespace Task1
{
    class Consumer : Worker
    {
        public static int ConsumerID = 0;

        public Consumer(List<String> tasks, Mutex mutex) : base(tasks, mutex, ConsumerID)
        {
            ConsumerID++;
        }

        override public void Job()
        {
            Random random = new Random();
            while (IsWorking)
            {
                bool consumed = false;
                Mutex.WaitOne();
                if (Tasks.Count > 0)
                {
                    Console.WriteLine(WorkerThread.Name + " has consumed task " + Tasks[0]);
                    Tasks.RemoveAt(0);
                    consumed = true;
                }
                Mutex.ReleaseMutex();
                if (consumed)
                {
                    Thread.Sleep(random.Next(250, 1000));
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
