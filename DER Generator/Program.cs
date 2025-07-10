using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DER_Generator
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pokretanje DER Generatora...");

            DERGenerator generator = new DERGenerator("127.0.0.1", 12345, 54323, 54324);
            generator.Start();
            Console.ReadKey();
        }
    }
}
