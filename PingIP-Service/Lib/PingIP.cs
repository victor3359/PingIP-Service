using System;
using System.Xml;
using System.Linq;
using System.Timers;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace PingIP_Service
{
    class PingIP
    {
        private readonly Timer _timer;
        private List<IpDevice> IpTable = new List<IpDevice>();
        private static int updRate;
        private static string xmlConfigFileName = @"PingIpConfig.xml";
        private static string xmlOutputFileName = @"PingIpOutput.xml";
        s61850 s61850;
        m61850 m61850;

        public PingIP()
        {
            ReadConfiguration();
            _timer = new Timer(updRate) { AutoReset = true };
            _timer.Elapsed += timerElapsed;

        }

        private void timerElapsed(object sender, ElapsedEventArgs e)
        {
            Ping pingIp;
            foreach(var ip in IpTable)
            {
                pingIp = new Ping();
                pingIp.PingCompleted += PingIp_PingCompleted;
                pingIp.SendAsync(ip.IpAddress, updRate, ip.IpAddress);
            }
            IpTableSavingXml();
            m61850.UpdateComtradeFiles();
        }

        private void PingIp_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine("Ping canceled.");
                return;
            }

            if (e.Error != null)
            {
                Console.WriteLine("Ping failed:");
                Console.WriteLine(e.Error.ToString());
                return;

            }
            if (e.Reply == null)
                return;

            (from ip in IpTable
             where ip.IpAddress == e.UserState.ToString()
             select ip).ToList().ForEach(x => x.IpStatus = e.Reply.Status);
        }

        private void IpTableSavingXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement(@"IpTable");
            int Ind = 1;
            string ValueStr;
            doc.AppendChild(root);
            IpTable.ForEach(x =>
            {
                XmlElement newNode = doc.CreateElement(@"Row");
                newNode.SetAttribute(@"MachineName", x.MachineName);
                newNode.SetAttribute(@"IpAddress", x.IpAddress);
                ValueStr = x.IpStatus == IPStatus.Success ? @"true" : @"false";
                newNode.InnerText = ValueStr;
                root.AppendChild(newNode);
                s61850.ModifySpsValue($"CSharp/GGIO1.Ind{Ind}", ValueStr);
                Ind++;
            });
            doc.Save(xmlOutputFileName);
        }

        public void Start()
        {
            _timer.Start();
        }
        public void Stop()
        {
            _timer.Stop();
        }
        private void ReadConfiguration()
        {
            XmlDocument xmlconf = new XmlDocument();
            XmlNode xmlNode;
            try
            {
                s61850 = new s61850();
                m61850 = new m61850("192.168.100.195");
                xmlconf.Load(xmlConfigFileName);

                xmlNode = xmlconf.SelectSingleNode(@"/Service/Info/UpdateRate");
                updRate = Convert.ToInt32(xmlNode.InnerText);

                xmlNode = xmlconf.SelectSingleNode(@"/Service/IpTable");
                foreach (XmlNode node in xmlNode)
                {
                    IpTable.Add(new IpDevice(node.Attributes[0].Value, node.Attributes[1].Value));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
