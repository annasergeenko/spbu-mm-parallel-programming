using ProducerConsumer;
using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProducerConsumer.Tests
{
    [TestClass]
    public class PlantTest
    {
        
        [TestMethod]

        public void ThreadsCreatesAndRemoves_C2_P3_5() // создает и удаляет правильное количество потоков
        {            
            Plant plant = new Plant(2, 3);
            plant.Start();
            Thread.Sleep(1000);
            Assert.AreEqual(2 + 3, plant.AliveWorkers());
            plant.Stop();
            Thread.Sleep(1000); //даем возмодможность потокам завершиться             
            Assert.AreEqual(0, plant.AliveWorkers());
        }

        
        [TestMethod()]
        public void PlantException_C2_Pnot8()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Plant(2, -8));
        }

        [TestMethod()]
        public void PlantException_Cnot2_P8()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Plant(-2, 8));
        }

        [TestMethod()]
        public void PlantException_Cnot2_Pnot8()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Plant(-2, -8));
        }
    }
}
