using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DER_Generator
{
    public class DERGenerator
    {
        private readonly string _serverAddress;
        private readonly int _serverPort;
        private readonly int _localPortUdp;
        private readonly int _localPortSensorTcp;
        private TcpClient _tcpClient;
        private UdpClient _udpClient;
        private TcpListener _sensorListener;
        private string _generatorType;
        private double _nominalPower;

        public DERGenerator(string serverAddress, int serverPort,int localPortUdp, int localPortSensorTcp)
        {
            _serverAddress = serverAddress;
            _serverPort = serverPort;
            _localPortUdp = localPortUdp;
            _localPortSensorTcp = localPortSensorTcp;
        }

        public void Start()
        {
            ChooseGeneratorType();
            ConnectToServer();
            OpenUdpSocket();
            StartSensorListener();
        }

        private void ChooseGeneratorType()
        {
            while (true) 
            {
                Console.WriteLine("Choose type of generator:\n1 - Solar Panel \n2 - Wind generator");
                string input = Console.ReadLine();

                if (input == "1")
                {
                    _generatorType = "SolarPanel";
                    Console.WriteLine("You choose Solar Panel.");
                    _nominalPower = GetNominalPower(100,500);
                    break;
                }
                else if (input == "2")
                {
                    _generatorType = "WindTurbine";
                    Console.WriteLine("You choose Wind Turbine.");
                    _nominalPower = GetNominalPower(500, 1000);
                    break;
                }
                else
                {
                    Console.WriteLine("Incorrect input. Try again!");
                }
            }
        }
        private double GetNominalPower(double min, double max)
        {
            while (true)
            {
                Console.WriteLine($"Please type nominal power ({min}-{max} kW): ");
                if (double.TryParse(Console.ReadLine(),out double power) && power >= min && power <=max)
                { 
                    return power;
                }
                else 
                {
                    Console.WriteLine("Incorrect input. Please try again!");
                }
                       
            }
        }
        private void ConnectToServer()
        {
            try
            {
                _tcpClient = new TcpClient(_serverAddress, _serverPort);
                Console.WriteLine("Connected with DERM server.");
                //SendPowerDataToServer(125, 0);
                //Slanje pocetnih podataka serveru
                string initialMessage = $"{_generatorType}:{_nominalPower}";
                byte[] messageBytes = Encoding.UTF8.GetBytes(initialMessage);
                //_tcpClient.GetStream().Write(messageBytes, 0, messageBytes.Length);
                Console.WriteLine("Initial data sended to server");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error with connection: {ex.Message}");
            }
        }
        private void OpenUdpSocket()
        {
            try
            {
                _udpClient = new UdpClient(_localPortUdp);
                string localIP = GetLocalIPAddress();
                Console.WriteLine("\n[UDP Socket]");
                Console.WriteLine($"- Control socket opened.");
                Console.WriteLine($"- Address: {localIP}");
                Console.WriteLine($"- Port: {_localPortUdp}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening UDP socket: {ex.Message}");
            }
        }

        private void StartSensorListener()
        {
            try
            {
                _sensorListener = new TcpListener(IPAddress.Any, _localPortSensorTcp);
                _sensorListener.Start();
                string localIP =GetLocalIPAddress();
                Console.WriteLine("\n[TCP Sensor Socket]");
                Console.WriteLine($"- Sensor socket opened.");
                Console.WriteLine($"- Address: {localIP}");
                Console.WriteLine($"- Port: {_localPortSensorTcp}\n");

                //osluskuj senzorske podatke
                while (true)
                {
                    TcpClient sensorClient = _sensorListener.AcceptTcpClient();
                    Console.WriteLine("Sensor connected!");


                    HandleSensorData(sensorClient);
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error during starting TCP sensor: {ex.Message}");
            }
        }

        private void HandleSensorData(TcpClient sensorClient)
        {
            NetworkStream stream = sensorClient.GetStream();
            byte[] buffer = new byte[1024];
            try
            {
                // Čitanje podataka od senzora
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string sensorMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead).Replace("\0", "").Trim();

                // Prvo proveriti "CheckGeneratorType"
                if (sensorMessage == "CheckGeneratorType")
                {
                    // Odgovor senzoru o tipu generatora
                    string response = _generatorType;
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine("Senzor uspešno povezan. Poslat odgovor.");

                    // Čitanje sledeće poruke koja sadrži podatke o insolation i temperaturi
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    sensorMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead).Replace("\0", "").Trim();
                }

                // Nezavisno od toga, obraditi podatke za generatore
                if (_generatorType == "SolarPanel")
                {
                    // Obrada podataka senzora za Solar Panel
                    string[] dataParts = sensorMessage.Split(',');
                    if (dataParts.Length == 2 &&
                        dataParts[0].StartsWith("INS:") &&
                        dataParts[1].StartsWith("Tcell:"))
                    {
                        double insolation = double.Parse(dataParts[0].Split(':')[1]);
                        double temperature = double.Parse(dataParts[1].Split(':')[1]);

                        // Izračunavanje aktivne snage
                        double activePower = CalculateActivePower(insolation, temperature);
                        
                        // Slanje podataka serveru
                        SendPowerDataToServer(activePower, 0); // Reaktivna snaga je 0

                        Console.WriteLine($"Primljeni podaci od senzora: Insolacija={insolation}, Temperatura={temperature}");
                        Console.WriteLine($"Izračunata aktivna snaga: {activePower:F2} kW");
                    }
                    else
                    {
                        Console.WriteLine($"Nepoznata poruka senzora: {sensorMessage}");
                    }
                }
                else if (_generatorType == "WindTurbine")
                {
                    // Obrada podataka senzora za Wind Turbine (ako je tip generatora WindTurbine)
                    // Možete dodati odgovarajući kod za WindTurbine ako je potrebno
                    if (sensorMessage.StartsWith("WIND:"))
                    {
                        double windSpeed = double.Parse(sensorMessage.Split(':')[1]);

                        double activePower = 0;

                        if (windSpeed < 3.5 || windSpeed > 25)
                        {
                            activePower = 0;
                        }
                        else if (windSpeed >= 3.5 && windSpeed < 14)
                        {
                            activePower = (windSpeed - 3.5) * 0.035 * _nominalPower;
                        }
                        else // 14 ≤ V ≤ 25
                        {
                            activePower = _nominalPower;
                        }

                        double reactivePower = activePower * 0.05;

                        SendPowerDataToServer(activePower, reactivePower);

                        Console.WriteLine($"Brzina vetra: {windSpeed:F2} m/s");
                        Console.WriteLine($"Izračunata aktivna snaga: {activePower:F2} kW");
                        Console.WriteLine($"Izračunata reaktivna snaga: {reactivePower:F2} kVar");
                    }
                    else
                    {
                        Console.WriteLine($"Nepoznata poruka senzora: {sensorMessage}");
                    }
                }
                else
                {
                    Console.WriteLine($"Primljeni podaci za generator tipa {_generatorType} nisu podržani.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška u komunikaciji sa senzorom: {ex.Message}");
            }
        }
        private double CalculateActivePower(double insolation, double temperature)
        {
            return _nominalPower * insolation * 0.00095 * (1 - 0.005 * (temperature - 25));
        }
        private void SendPowerDataToServer(double activePower, double reactivePower)
        {
            try
            {
                if (_tcpClient != null && _tcpClient.Connected)
                {
                    // Generiši ID na osnovu tipa generatora i hash koda
                    string prefix = _generatorType == "SolarPanel" ? "SP" : "WT";
                    string id = $"{prefix}{this.GetHashCode()}";  // Može i Guid ako želiš jedinstvenije

                    // Kreiraj poruku u formatu: ID=...;P=...;Q=...
                    string message = $"ID={id};P={activePower:F2};Q={reactivePower:F2}";

                    byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                    _tcpClient.GetStream().Write(messageBytes, 0, messageBytes.Length);

                    Console.WriteLine($"Sent to server: {message}");
                }
                else
                {
                    Console.WriteLine("Error: Cannot send data, TCP connection is not established.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending power data to server: {ex.Message}");
            }
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1"; // Podrazumevana vrednost ako nije pronađena IPv4 adresa
        }
    }
}
