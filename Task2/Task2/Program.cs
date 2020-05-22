using System;
using System.Threading;

namespace Task2
{
    public class Program
    {
        static public int Sum(int x, int y)
        {
            Thread.Sleep(1000);
            return x + y;
        }

        static void Main(string[] args)
        {
            ThreadPool tp = new ThreadPool(3);
            CustomTask<int> t1 = new CustomTask<int>(() => Sum(10, 218)),
                t2 = new CustomTask<int>(() => Sum(300, 22)),
                t3 = (CustomTask<int>)t1.ContinueWith<int>(y => Sum(y, -200)),
                t4 = new CustomTask<int>(() => Sum(606, 60)),
                t5 = (CustomTask<int>)t1.ContinueWith<int>(y => Sum(y, -28)),
                t6 = (CustomTask<int>)t3.ContinueWith<int>(y => Sum(y, -15));
            tp.Enqueue<int>(t1);
            tp.Enqueue<int>(t2);
            tp.Enqueue<int>(t3);
            tp.Enqueue<int>(t4);
            tp.Enqueue<int>(t5);
            tp.Enqueue<int>(t6);
            tp.Start();
            Thread.Sleep(5000);
            tp.Dispose();
            Console.WriteLine("Finished");
        }
    }
}