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
                Console.WriteLine($"Primljeni podaci od klijenta: \n{data}");

                //odgovor klijenta

                string response = "Podaci primljeni.";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);

            }
            catch (Exception ex)
            { Console.WriteLine($"Greska u komunikaciji sa klijentom: {ex.Message}"); }
            finally { client.Close(); }
        }
        public void Stop()
        {
            server?.Stop();
            Console.WriteLine("Server zaustavljen.");
        }
    }
}
