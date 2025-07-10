using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sensor
{
    public class WindWeatherSensor
    {
        private readonly string _generatorAddress;
        private readonly int _generatorPort;
        private readonly Random _random;

        public WindWeatherSensor(string generatorAddress, int generatorPort)
        {
            _generatorAddress = generatorAddress;
            _generatorPort = generatorPort;
            _random = new Random();
        }

        public void Start()
        {
            try
            {
                using (TcpClient client = new TcpClient(_generatorAddress, _generatorPort))
                {
                    Console.WriteLine("Senzor povezan sa generatorom.");

                    NetworkStream stream = client.GetStream();

                    //slanje zahteva za proveru tipa generatora
                    string message = "CheckGeneratorType";
                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    stream.Write(messageBytes,0,messageBytes.Length);

                    byte[] responseBuffer = new byte[1024];
                    int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
                    string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

                    if (response == "WindTurbine")
                    {
                        Console.WriteLine("Povezano sa vetrogeneratorom. Pocinje slanje podataka...");
                        SimulateAndSendData(stream);
                    }    
                    else if (response == "SolarPanel")
                    {
                        Console.WriteLine("Greska : Senzor je povezan sa solarnim panelom");
                    }
                    else
                    { Console.WriteLine($"Nepoznat odgovor: {response}"); };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska: {ex.Message}");
            }
        }

        private void SimulateAndSendData(NetworkStream stream)
        {
            while (true) 
            { 
                double windSpeed = _random.NextDouble() *30;
                string data = $"WIND: {windSpeed:F2}";
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                stream.Write(dataBytes, 0, dataBytes.Length);

                Console.WriteLine($"Poslata brzina vetra: {windSpeed}:F2 m/s");
                Thread.Sleep(5000);
            }
        }
    }
}
