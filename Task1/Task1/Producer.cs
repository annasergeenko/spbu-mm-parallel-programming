using System;
using System.Collections.Generic;
using System.Threading;

namespace Task1
{
    class Producer : Worker
    {
        public static int ProducerID = 0;
        private int TaskID;

        public Producer(List<String> tasks, Mutex mutex) : base(tasks, mutex, ProducerID)
        {
            ProducerID++;
            TaskID = 0;
        }

        override public void Job()
        {
            Random random = new Random();
            while (IsWorking)
            {
                Mutex.WaitOne();
                Tasks.Add(ID + "." + TaskID);
                Console.WriteLine(WorkerThread.Name + " has produced task " + TaskID);
                TaskID++;
                Mutex.ReleaseMutex();
                Thread.Sleep(random.Next(100, 1000));
            }
        }
    }
}
