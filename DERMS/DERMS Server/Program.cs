using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DERMS_Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pokretanje dispecerskog programa...");
            DispatcherServer dispatcherServer = new DispatcherServer(12345);
            dispatcherServer.Start();
        }
    }
}
