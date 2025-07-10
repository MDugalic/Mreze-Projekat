using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Sensor
{
    public class SmartSensor
    {
        private readonly string _address;
        private readonly int _port;

        public SmartSensor(string address, int port)
        {
            _address = address;
            _port = port;
        }

        public void Start()
        {
            try
            {
                using (TcpClient client = new TcpClient(_address, _port))
                {
                    NetworkStream stream = client.GetStream();
                    byte[] check = Encoding.UTF8.GetBytes("CheckGeneratorType");
                    stream.Write(check, 0, check.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                    client.Close();

                    if (response == "SolarPanel")
                    {
                        new SolarWeatherSensor(_address, _port).Start();
                    }
                    else if (response == "WindTurbine")
                    {
                        new WindWeatherSensor(_address, _port).Start();
                    }
                    else
                    {
                        Console.WriteLine("Nepoznat tip generatora.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom konekcije: {ex.Message}");
            }
        }
    }
}
