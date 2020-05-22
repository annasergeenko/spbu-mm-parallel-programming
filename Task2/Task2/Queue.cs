using System;
using System.Threading;

namespace Task2
{
    class Queue
    {
        private ITask[] Tasks;
        private Tuple<int, int> headIndex;
        private volatile int tailIndex;

        public Queue(uint queueSize)
        {
            Tasks = new ITask[queueSize];
            headIndex = new Tuple<int, int>(0, 0);
            tailIndex = 0;
        }

        public bool IsEmpty()
        {
            int head = Volatile.Read(ref headIndex).Item1;
            int tail = tailIndex;
            return (tail <= head);
        }
        
        public void Push(ITask task)
        {
            Tasks[tailIndex] = task;
            tailIndex++;
        }

        public ITask GetFromHead()
        {
            Tuple<int, int> oldPair = Volatile.Read(ref headIndex);
            Tuple<int, int> newPair = new Tuple<int, int>(oldPair.Item1 + 1, oldPair.Item2 + 1);
            if (tailIndex <= oldPair.Item1)
            {
                return null;
            }
            ITask task = Tasks[oldPair.Item1];
            if (oldPair == Interlocked.CompareExchange<Tuple<int, int>>(ref headIndex, newPair, oldPair))
            {
                return task;
            }
            return null;
        }

        public ITask GetFromTail()
        {
            if (tailIndex == 0)
            {
                return null;
            }
            tailIndex--;
            ITask task = Tasks[tailIndex];
            Tuple<int, int> oldPair = Volatile.Read(ref headIndex);
            Tuple<int, int> newPair = new Tuple<int, int>(0, oldPair.Item2 + 1);
            if (tailIndex > oldPair.Item1)
            {
                return task;
            }
            if (tailIndex == oldPair.Item1)
            {
                tailIndex = 0;
                if (oldPair == Interlocked.CompareExchange<Tuple<int, int>>(ref headIndex, newPair, oldPair))
                {
                    return task;
                }
            }
            Volatile.Write(ref headIndex, newPair);
            return null;
        }

        public int RemainingTasks()
        {
            return (tailIndex - Volatile.Read(ref headIndex).Item1);
        }
    }
}
