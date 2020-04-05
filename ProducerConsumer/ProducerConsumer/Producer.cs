using System;
using System.Collections.Generic;
using System.Threading;

namespace ProducerConsumer
{
    class Producer
    {
        private List<int> buff;
        private Mutex mutex;
        private bool fl = true;
        private Thread thread;
        private Random r;

        public Producer(List<int> buffer, Mutex mutex, int i)
        {
            r = new Random();
            buff = buffer;
            this.mutex = mutex;
            thread = new Thread(StartWork);
            thread.Name = "Producer_" + i;
            thread.Start();
        }

        private void StartWork()
        {
            while (fl)
            {
                mutex.WaitOne();

                buff.Add(r.Next(100));
                Console.WriteLine($"{Thread.CurrentThread.Name} add; buffer: {buff.Count}");

                mutex.ReleaseMutex();

                Thread.Sleep(r.Next(800)+200); // спит случайное число от 200 до 1000 млс
            }

        }

        public void Delete()
        {
            fl = false;
            thread.Join();
            Console.WriteLine($"{thread.Name} deleted {thread.IsAlive}");
        }

        public bool IsWorking
        {
            get { return thread != null && thread.IsAlive; }
        }

    }
}
