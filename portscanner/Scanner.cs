using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Drawing;
using System.Windows.Forms;

namespace PortScanner
{
    class Scanner
    {
        public static bool scanning = false;
        public static int timeoutMS = 100;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public static IPInfo[] Scan(string[] address, int[] port)
        {
            List<IPInfo> returnList = new List<IPInfo>();

            scanning = true;

            for (int y = 0; y < address.Length; y++)
            {
                for (int i = 0; i < port.Length; i++)
                {
                    TcpClient client = new TcpClient();
                    try
                    {
                        var result = client.BeginConnect(address[y], port[i], null, null);

                        if (result.AsyncWaitHandle.WaitOne(timeoutMS))
                        {
                            returnList.Add(new IPInfo(address[y], port[i].ToString(), "True"));
                        } else
                        {
                            returnList.Add(new IPInfo(address[y], port[i].ToString(), "False"));
                        }
                        
                    }
                    catch (Exception)
                    {
                        returnList.Add(new IPInfo(address[y], port[i].ToString(), "False"));
                    }
                }
            }

            scanning = false;
            return returnList.ToArray();
        }

        public struct IPInfo
        {
            public IPInfo(string address, string port, string status)
            {
                Address = address;
                Port = port;
                Status = status;
            }

            public string Address { get; }
            public string Port { get; }
            public string Status { get; }
        }
    }
}
