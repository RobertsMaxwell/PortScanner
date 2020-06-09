using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Threading;
using System.Net.Sockets;

namespace PortScanner
{
    class Scanner
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();

            Console.Write("Enter an IP Address:");
            string address = Console.ReadLine();
            
            Console.Write("Enter a port to be scanned:");
            int port = Convert.ToInt32(Console.ReadLine());

            try
            {
                client.Connect(address, port);
                Console.WriteLine($"Port {port} on {address} is open");
            }
            catch (Exception)
            {
                Console.WriteLine($"Port {port} on {address} is closed");
            }
        }
    }
}
