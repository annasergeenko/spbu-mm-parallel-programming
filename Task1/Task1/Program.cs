using System;

namespace Task1
{
    class Program
    {
        static void Main(string[] args)
        {
            ProducerConsumerPattern pattern = new ProducerConsumerPattern(10, 5);
            pattern.Start();
            Console.ReadKey();
            pattern.Finish();
            Console.WriteLine("Work is finished");
        }
    }
}
