using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace Task1
{
    public class ProducerConsumerPattern
    {
        public enum WorkerType { Consumer, Producer, Working };

        private List<String> Tasks;
        private List<Producer> Producers;
        private List<Consumer> Consumers;
        private Mutex Mutex;

        public ProducerConsumerPattern(uint producers, uint consumers)
        {
            Producers = new List<Producer>();
            Consumers = new List<Consumer>();
            Tasks = new List<String>();
            Mutex = new Mutex();
            while (producers != 0)
            {
                Producers.Add(new Producer(Tasks, Mutex));
                producers--;
            }
            while (consumers != 0)
            {
                Consumers.Add(new Consumer(Tasks, Mutex));
                consumers--;
            }
        }

        public void Start()
        {
            foreach (Producer p in Producers)
            {
                p.Start("Producer");
            }
            foreach (Consumer c in Consumers)
            {
                c.Start("Consumer");
            }
        }

        public void Finish()
        {
            foreach (Producer p in Producers)
            {
                p.Finish();
            }
            foreach (Consumer c in Consumers)
            {
                c.Finish();
            }
        }

        public int GetWorkerNum(WorkerType type)
        {
            switch (type)
            {
                case WorkerType.Consumer:
                    return Consumers.Count;
                case WorkerType.Producer:
                    return Producers.Count;
                case WorkerType.Working:
                    return (Producers.Count(x => x.IsRunning()) + Consumers.Count(x => x.IsRunning()));
                default:
                    return 0;
            }
        }
    }
}
