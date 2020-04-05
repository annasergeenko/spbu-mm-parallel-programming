using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Plant plant = new Plant();
            plant.Start();

            Console.ReadKey();

            plant.Stop();
            Console.ReadKey();

        }
    }
}
