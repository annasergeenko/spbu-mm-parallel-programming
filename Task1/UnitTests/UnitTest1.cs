using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Task1;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test1()
        {
            ProducerConsumerPattern pattern = new ProducerConsumerPattern(10, 0);
            pattern.Start();
            Assert.AreEqual(pattern.GetWorkerNum(ProducerConsumerPattern.WorkerType.Producer), 10);
            pattern.Finish();
        }

        [TestMethod]
        public void Test2()
        {
            ProducerConsumerPattern pattern = new ProducerConsumerPattern(0, 5);
            pattern.Start();
            Assert.AreEqual(pattern.GetWorkerNum(ProducerConsumerPattern.WorkerType.Consumer), 5);
            pattern.Finish();
        }

        [TestMethod]
        public void Test3()
        {
            ProducerConsumerPattern pattern = new ProducerConsumerPattern(10, 5);
            Assert.AreEqual(pattern.GetWorkerNum(ProducerConsumerPattern.WorkerType.Working), 0);
            pattern.Start();
            Assert.AreEqual(pattern.GetWorkerNum(ProducerConsumerPattern.WorkerType.Working), 15);
            pattern.Finish();
            Assert.AreEqual(pattern.GetWorkerNum(ProducerConsumerPattern.WorkerType.Working), 0);
        }
    }
}
