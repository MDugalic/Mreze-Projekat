using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sensor
{
    public class SolarWeatherSensor
    {
        private readonly string _generatorAddress;
        private readonly int _generatorPort;

        public SolarWeatherSensor(string generatorAddress, int generatorPort)
        {
            _generatorAddress = generatorAddress;
            _generatorPort = generatorPort;
        }
        public void Start()
        {
            try
            {
                using (TcpClient client = new TcpClient(_generatorAddress, _generatorPort))
                {
                    Console.WriteLine("Senzor povezan sa generatorom.");

                    NetworkStream stream = client.GetStream();

                    // Slanje zahteva za proveru tipa generatora
                    string message = "CheckGeneratorType";
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    stream.Write(messageBytes, 0, messageBytes.Length);

                    // Čitanje odgovora od generatora
                    byte[] responseBuffer = new byte[1024];
                    int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
                    string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
                    Console.WriteLine( $"{response} bilo sta");
                    if (response == "SolarPanel")
                    {
                        Console.WriteLine("Povezano sa solarnim panelom. Počinje slanje podataka...");
                        SimulateAndSendData(stream);
                    }
                    else if(response == "WindTurbine")
                    {
                        Console.WriteLine("Greska: Senzor je povezan sa vetroturbinom.");
                    }
                    else
                    {
                        
                        Console.WriteLine($"Greška: Nepoznat odgovor. {response}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška: {ex.Message}");
            }
        }

        private void SimulateAndSendData(NetworkStream stream) 
        {

            while (true)
            {
                int hour = DateTime.Now.Hour;
                double insolation = SimulateInsolation(hour);//izlaganje suncu
                double temperature = SimulateTemperature(insolation);

                //kreiranje poruke
                string data = $"INS: {insolation:f2},Tcell: {temperature:F2}";
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                stream.Write(dataBytes, 0, dataBytes.Length);

                Console.WriteLine($"Poslati podaci: {data}");

                //Pauza pre sledece simulacije

                Thread.Sleep(5000);
            }
        }

        private double SimulateInsolation(int hour)
        {
            if (hour >= 12 && hour <= 14)
                return 1050;
            else if (hour < 12)
                return 1050 - (12 - hour) * 200;
            else
                return 1050 - (hour - 14) * 200; 
        }

        private double SimulateTemperature(double insolation)
        {
            return insolation >= 25 ? 25 : 25 + 0.025 * insolation;
        }
    }
}
