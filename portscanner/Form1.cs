﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static PortScanner.Scanner;

namespace PortScanner
{
    public partial class Form1 : Form
    {
        public string addressExampleText;
        public string portExampleText;

        string[] ipAddresses;
        int[] ports;

        string localIP;

        public int barMaximum = 100;

        public int barProgress
        {
            get { return barProgress; }
            set { UpdateProgress(barProgress, barMaximum); }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            localIP = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .First()
                .ToString();

            comboBox1.SelectedItem = "100";
            addressExampleText = $"e.g. {localIP}, 192.168.10.1 - 192.168.10.255";
            portExampleText = $"e.g. 80, 1 - 255";
            IPAddressStart_Leave();
            PortStart_Leave();
        }

        //Initiate Scan
        private void ScanStart_Click(object sender, EventArgs e)
        {
            if (!scanning)
            {
                try
                {
                    Scanner scanner = new Scanner();
                    var addresses = IPAddress_Update(IPAddressStart.Text);
                    var ports = Port_Update(PortStart.Text);

                    PopulateList(Scan(addresses, ports, progressBar, ETA));
                }
                catch (ArgumentException err)
                {
                    MessageBox.Show(err.Message, "Input Error", MessageBoxButtons.OK);
                }
            }
        }

        //Verify that it is a valid IP range and return a list of ips
        private string[] IPAddress_Update(string ipString)
        {
            List<string> ipStringList = new List<string>();
            ipString = ipString.Replace(" ", "");

            if (!ipString.Contains(',') && !ipString.Contains('-'))
            {
                if (ValidIP(ipString))
                {
                    return new string[] { ipString };
                }
            }
            else if (ipString.Contains(','))
            {
                string[] ipList = ipString.Split(',');

                foreach (string ip in ipList)
                {
                    var li = IPAddress_Update(ip);
                    if (li.Length != 0)
                    {
                        foreach (var sub in li)
                        {
                            ipStringList.Add(sub);
                        }
                    }
                }
                return ipStringList.ToArray();
            }
            else if (ipString.Contains('-'))
            {
                string ipStringLeft = ipString.Split('-')[0];
                string ipStringRight = ipString.Split('-')[1];

                if (ValidIP(ipStringLeft) && ValidIP(ipStringRight))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (ipStringLeft.Split('.')[i] != ipStringRight.Split('.')[i])
                        {
                            throw new ArgumentException("Please enter a valid IP range");
                        }
                    }

                    if (Convert.ToInt32(ipStringRight.Split('.')[3]) >= Convert.ToInt32(ipStringLeft.Split('.')[3]))
                    {
                        for (int i = Convert.ToInt32(ipStringLeft.Split('.')[3]); i <= Convert.ToInt32(ipStringRight.Split('.')[3]); i++)
                        {
                            List<string> deltaIP = ipStringLeft.Split('.').ToList().GetRange(0, 3);
                            deltaIP.Add(i.ToString());
                            ipStringList.Add(string.Join(".", deltaIP));
                        }
                    }

                    return ipStringList.ToArray();
                }
            }

            throw new ArgumentException("Please Input a valid IPv4 Address");
        }

        //Verify that it is a valid port range and return a list of ports
        private int[] Port_Update(string portString)
        {
            List<int> portList = new List<int>();
            portString = portString.Replace(" ", "");

            if (!portString.Contains(",") && !portString.Contains("-") && ValidPort(Convert.ToInt32(portString)))
            {
                return new int[] { Convert.ToInt32(portString) };
            }
            else if (portString.Contains(","))
            {
                string[] portStringList = portString.Split(',');

                foreach (string port in portStringList)
                {
                    foreach (int sub in Port_Update(port))
                    {
                        portList.Add(sub);
                    }
                }
                return portList.ToArray();
            }
            else if (portString.Contains("-"))
            {
                int portNumLeft = Convert.ToInt32(portString.Split('-')[0]);
                int portNumRight = Convert.ToInt32(portString.Split('-')[1]);

                if (ValidPort(portNumLeft) && ValidPort(portNumRight))
                {
                    for (int i = portNumLeft; i <= portNumRight; i++)
                    {
                        portList.Add(i);
                    }

                    return portList.ToArray();
                }
            }

            throw new ArgumentException("Please input a valid Port");
        }

        private void PopulateList(IPInfo[] info)
        {
            listView1.Items.Clear();
            for (int i = 0; i < info.Length; i++)
            {
                listView1.Items.Add(new ListViewItem(new string[] { info[i].Address, info[i].Port, info[i].Status }));
            }
        }

        private bool ValidIP(string ip)
        {
            List<string> byteNumList = ip.Split('.').ToList();

            if (byteNumList.Count != 4)
            {
                return false;
            }

            foreach (string str in byteNumList)
            {
                try
                {
                    if (Convert.ToInt32(str) < 1 || Convert.ToInt32(str) > 255)
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidPort(int port)
        {
            if (port > 65535 || port < 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void IPAddressStart_Leave()
        {
            if (IPAddressStart.Text == "")
            {
                IPAddressStart.ForeColor = Color.Gray;
                IPAddressStart.Text = addressExampleText;
            }
        }

        private void IPAddressStart_Enter()
        {
            if (IPAddressStart.Text == addressExampleText)
            {
                IPAddressStart.ForeColor = Color.Black;
                IPAddressStart.Text = "";
            }
        }

        private void PortStart_Leave()
        {
            if (PortStart.Text == "")
            {
                PortStart.ForeColor = Color.Gray;
                PortStart.Text = portExampleText;
            }
        }

        private void PortStart_Enter() 
        {
            if (PortStart.Text == portExampleText)
            {
                PortStart.ForeColor = Color.Black;
                PortStart.Text = "";
            }
        }

        private void IPAddressStart_Enter(object sender, EventArgs e) { IPAddressStart_Enter(); }
        private void IPAddressStart_Leave(object sender, EventArgs e) { IPAddressStart_Leave(); }
        private void PortStart_Enter(object sender, EventArgs e) { PortStart_Enter(); }
        private void PortStart_Leave(object sender, EventArgs e) { PortStart_Leave(); }

        //timeout dropdown
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            timeoutMS = Convert.ToInt32(comboBox1.SelectedItem);
        }

        // scan LAN checkbox
        string textBefore = "";
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                if (IPAddressStart.Text != addressExampleText)
                {
                    textBefore = IPAddressStart.Text;
                }
                
                IPAddressStart.Focus();

                var start = localIP.Split('.').Select(str => Convert.ToInt32(str)).ToArray();
                var end = localIP.Split('.').Select(str => Convert.ToInt32(str)).ToArray();
                start[3] = 1;
                end[3] = 255;
                string lanRange = string.Join(".", start.Select(num => num.ToString())) + " - " + string.Join(".", end.Select(num => num.ToString()));

                IPAddressStart.Text = lanRange;
                ScanStart.Focus();
            }
            else
            {
                IPAddressStart.Focus();
                IPAddressStart.Text = textBefore;
                ScanStart.Focus();
            }
        }

        //progress bar update
        public void UpdateProgress(int progress, int goal)
        {
            progressBar.Value = (int)((float)progress / goal * 100);
        }
    }
}
