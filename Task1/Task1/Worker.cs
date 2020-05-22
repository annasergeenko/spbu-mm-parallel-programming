using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Task1
{
    abstract class Worker
    {
        protected List<String> Tasks;
        protected Mutex Mutex;
        protected int ID;
        protected Thread WorkerThread;
        protected bool IsWorking;

        protected Worker(List<String> tasks, Mutex mutex, int id)
        {
            Mutex = mutex;
            Tasks = tasks;
            ID = id;
        }

        abstract public void Job();

        public void Start(String prefix)
        {
            if (IsWorking)
            {
                return;
            }
            IsWorking = true;
            WorkerThread = new Thread(Job);
            WorkerThread.Name = prefix + "-" + ID;
            WorkerThread.Start();
        }

        public void Finish()
        {
            if (!IsWorking)
            {
                return;
            }
            IsWorking = false;
            WorkerThread.Join();
        }

        public bool IsRunning()
        {
            return ((WorkerThread != null) && WorkerThread.IsAlive);
        }
    }
}
