using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sensor
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Unesite TCP port DER Generatora: ");
            int port = int.Parse(Console.ReadLine());

            // Pokreni zajednički senzor koji zna da odluči
            SmartSensor smartSensor = new SmartSensor("127.0.0.1", port);
            smartSensor.Start();

            Console.ReadKey();
        }
    }
}