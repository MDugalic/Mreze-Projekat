using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DERMS_Server
{
    public class DispatcherServer
    {
        private TcpListener server;
        private readonly int port;
        private List<Production> productions = new List<Production>();
        public DispatcherServer(int port)
        {
            this.port = port;
            server = new TcpListener(IPAddress.Any, port);
        }
        public void Start()
        {
            try
            {
                server.Start();
                Console.WriteLine($"Server je pokrenut na portu {port}. Ceka se veza...");

                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Klijent povezan!");

                    Task.Run(() => HandleClient(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greska: {ex.Message}");
            }
            finally { Stop(); }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received data from client:\n{data}");

                // Expected format: ID=SP123;P=100.5;Q=0.0
                if (data.StartsWith("ID="))
                {
                    string[] parts = data.Split(';');
                    string id = parts[0].Split('=')[1];
                    double activePower = double.Parse(parts[1].Split('=')[1]);
                    double reactivePower = double.Parse(parts[2].Split('=')[1]);

                    var record = new Production
                    {
                        Id = id,
                        ActivePower = activePower,
                        ReactivePower = reactivePower
                    };

                    productions.Add(record); // Add to the list

                    Console.WriteLine($"Saved production: {id} | P = {activePower} kW | Q = {reactivePower} kVar");

                    // Send response to client
                    string response = "Production data received and saved.";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
                else
                {
                    Console.WriteLine("Unrecognized message format.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error communicating with client: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }
        public void Stop()
        {
            try
            {
                Console.WriteLine("\nServer zaustavljen. Prikaz statistike proizvodnje:");

                var solar = productions.Where(p => p.Type == "Solar");
                var wind = productions.Where(p => p.Type == "Wind");

                double avgSolar = solar.Any() ? solar.Average(p => p.ActivePower) : 0;
                double avgWind = wind.Any() ? wind.Average(p => p.ActivePower) : 0;
                double totalReactive = productions.Sum(p => p.ReactivePower);

                Console.WriteLine($"Prosečna aktivna snaga - Solarni: {avgSolar:F2} kW");
                Console.WriteLine($"Prosečna aktivna snaga - Vetrogeneratori: {avgWind:F2} kW");
                Console.WriteLine($"Ukupna reaktivna snaga svih generatora: {totalReactive:F2} kVar");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greška prilikom prikaza statistike: {ex.Message}");
            }
            finally
            {
                server?.Stop();
            }
        }
    }
}
