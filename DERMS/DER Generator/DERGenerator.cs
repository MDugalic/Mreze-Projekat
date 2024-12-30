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

                //Slanje pocetnih podataka serveru
                string initialMessage = $"{_generatorType}:{_nominalPower}";
                byte[] messageBytes = Encoding.UTF8.GetBytes(initialMessage);
                _tcpClient.GetStream().Write(messageBytes, 0, messageBytes.Length);
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
                Console.WriteLine($"UDP socket open on port {_localPortUdp} for control message.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Message during opening UDP socket: {ex.Message}");
            }
        }

        private void StartSensorListener()
        {
            try
            {
                _sensorListener = new TcpListener(IPAddress.Any, _localPortSensorTcp);
                _sensorListener.Start();
                Console.WriteLine($"TCP socket opened on port {_localPortSensorTcp} for sensor data!");

                //osluskuj senzorske podatke
                while (true)
                {
                    TcpClient sensorClient = _sensorListener.AcceptTcpClient();
                    Console.WriteLine("Connected sensor!");


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
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string sensorData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Recivied data from sensor: {sensorData}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in communication with sensor: {ex.Message}");
            }
            finally 
            {
                sensorClient.Close();

            }
        }
    }
}
