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
            Console.WriteLine("Pokretanje senzora vremenskih prilika za solarni panel...");

            SolarWeatherSensor sensor = new SolarWeatherSensor("127.0.0.1", 54322);
            sensor.Start();
            Console.ReadKey();
        }
    }
}
