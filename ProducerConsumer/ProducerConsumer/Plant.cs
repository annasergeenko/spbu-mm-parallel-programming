using System;
using System.Collections.Generic;
using System.Threading;

namespace ProducerConsumer
{
    public class Plant
    {
        private List<int> buffer;
        private Producer[] pr;
        private Consumer[] con;

        public Plant()
        {
            ReadInfo();
            buffer = new List<int>();
        }

        public Plant(int consNum, int prodNum)
        {
            if (consNum < 0 || prodNum < 0) throw new ArgumentOutOfRangeException("Not natural number.");
            pr = new Producer[prodNum];
            con = new Consumer[consNum];
            buffer = new List<int>();
        }

        public void Start()
        {
            Mutex mutex = new Mutex();

            for (int i = 0; i < con.Length; i++)
            {
                con[i] = new Consumer(buffer, mutex, i);
            }

            for (int i = 0; i < pr.Length; i++)
            {
                pr[i] = new Producer(buffer, mutex, i);
            }
                
        }

        public void Stop()
        {

            Console.WriteLine("All processes will be finished, please, wait");

            for (int i = 0; i < con.Length; i++)
            {
                con[i].Delete();
               // con[i] = null; // для удаления объекта из массива
            }
            for (int i = 0; i < pr.Length; i++)
            {
                pr[i].Delete();
              //  pr[i] = null;// для удаления объекта из массива
            }

            Console.WriteLine($"The end. Buffer size: {buffer.Count}");
        }


        /// <summary>
        /// read number of consumers and producers from console
        /// </summary>
        private void ReadInfo()
        {
            int numbOfProd = 0, consNum = 0;
            Console.Write("Input number of Producers: ");

            while (!Int32.TryParse(Console.ReadLine(), out numbOfProd) && numbOfProd < 0)
            {
                Console.WriteLine("Not natural number. Input number of Producers again: ");
            }
            Console.Write("Input number of Consumers: ");

            while (!Int32.TryParse(Console.ReadLine(), out consNum) && consNum < 0)
            {
                Console.WriteLine("Not natural number. Input number of Consumers again: ");
            }

            pr = new Producer[numbOfProd];
            con = new Consumer[consNum];
        }

        public int AliveWorkers()
        {
            int count = 0;
            foreach (Consumer el in con)
            {
                if (el.IsWorking) count++;
            }
            foreach (Producer el in pr)
            {
                if (el.IsWorking) count++;
            }
            return count;
        }
        
    }
}
