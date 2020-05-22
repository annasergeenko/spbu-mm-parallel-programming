using System;
using Task2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test1()
        {
            Task2.ThreadPool tp = new Task2.ThreadPool(2);
            CustomTask<int> t1 = new CustomTask<int>(() => Program.Sum(5, 125)),
                t2 = new CustomTask<int>(() => Program.Sum(-16, 72)),
                t3 = new CustomTask<int>(() => Program.Sum(42, 42));
            tp.Enqueue<int>(t1);
            Assert.AreEqual(tp.RemainingTasks(), 1);
            tp.Enqueue<int>(t2);
            tp.Enqueue<int>(t3);
            Assert.AreEqual(tp.RemainingTasks(), 3);
            tp.Dispose();
            Assert.AreEqual(tp.RemainingTasks(), 0);
        }

        [TestMethod]
        public void Test2()
        {
            for (uint N = 1; N <= 8; N++)
            {
                Task2.ThreadPool tp = new Task2.ThreadPool(N);
                for (int n = 0; n < N; n++)
                {
                    tp.Enqueue<int>(new CustomTask<int>(() => Program.Sum(n, 220)));
                }
                tp.Start();
                Thread.Sleep(10);
                tp.Finish();
                Assert.AreEqual(tp.RemainingTasks(), 0);
                tp.Dispose();
            }
        }

        [TestMethod]
        public void Test3()
        {
            int x = 0;
            CustomTask<int> t = new CustomTask<int>(() => (12 / x));
            Assert.ThrowsException<AggregateException>(() => t.Result());
        }

        [TestMethod]
        public void Test4()
        {
            Task2.ThreadPool tp = new Task2.ThreadPool(25);
            for (int n = 0; n < 25; n++)
            {
                tp.Enqueue<int>(new CustomTask<int>(() => Program.Sum(n, n)));
            }
            Assert.AreEqual(tp.RemainingTasks(), 25);
            tp.Start();
            Thread.Sleep(10);
            tp.Finish();
            Assert.AreEqual(tp.RemainingTasks(), 0);
        }

        [TestMethod]
        public void Test5()
        {
            CustomTask<double> t1 = new CustomTask<double>(() => 0.5);
            CustomTask<int> t2 = (CustomTask<int>)t1.ContinueWith<int>(x => (int)(x * 10));
            t1.Result();
            Assert.AreEqual(t2.Result(), 5);
        }

        [TestMethod]
        public void Test6()
        {
            CustomTask<int> t = new CustomTask<int>(() => 5);
            CustomTask<int> t1 = (CustomTask<int>)t.ContinueWith<int>(x => x + 10),
                t2 = (CustomTask<int>)t.ContinueWith<int>(x => x - 10),
                t3 = (CustomTask<int>)t.ContinueWith<int>(x => x * 2);
            t.Result();
            Assert.AreEqual(t1.Result(), 15);
            Assert.AreEqual(t2.Result(), -5);
            Assert.AreEqual(t3.Result(), 10);
        }

        [TestMethod]
        public void Test7()
        {
            CustomTask<int> t1 = new CustomTask<int>(() => 100);
            CustomTask<int> t2 = (CustomTask<int>)t1.ContinueWith<int>(x => x / 2),
                t3 = (CustomTask<int>)t2.ContinueWith<int>(x => x - 10),
                t4 = (CustomTask<int>)t3.ContinueWith<int>(x => x + 2),
                t5 = (CustomTask<int>)t4.ContinueWith<int>(x => x * 3);
            t1.Result();
            t2.Result();
            t3.Result();
            t4.Result();
            Assert.AreEqual(t5.Result(), 126);
        }
    }
}
