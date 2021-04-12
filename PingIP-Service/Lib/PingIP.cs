using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Timers;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using IEC61850.Client;
using IEC61850.Common;

namespace PingIP_Service
{
    class PingIP
    {
        private readonly Timer _timer;
        private DateTime TimeStamp;
        private List<IpDevice> IpTable = new List<IpDevice>();
        private static int updRate;
        private static string xmlConfigFileName = @"PingIpConfig.xml";
        private static string xmlOutputFileName = @"PingIpOutput.xml";
        //private static string EventLogFileName = @"Events.log";
        s61850 s61850;
        //m61850 m61850;

        public PingIP()
        {
            ReadConfiguration();
            _timer = new Timer(updRate) { AutoReset = true };
            _timer.Elapsed += timerElapsed;

            /*if (!File.Exists(EventLogFileName))
            {
                using (StreamWriter sw = File.CreateText(EventLogFileName))
                {
                    string RawText = $"MachineName, IpAddress, PingStatus, TimeStamp";
                    sw.WriteLine(RawText);
                }
            }*/
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
            /*if(m61850.GetState() != IedConnectionState.IED_STATE_CONNECTING)
            {
                m61850.UpdateComtradeFiles();
            }*/
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
             select ip).ToList().ForEach(x => {
                 if (!x.IpAddress.Equals(e.Reply.Status))
                 {
                     x.IpStatus = e.Reply.Status;
                     TimeStamp = DateTime.Now;
                     string RawText = $"{x.MachineName}, {x.IpAddress}, {x.IpStatus}, {TimeStamp}";
                     /*using (StreamWriter sw = File.AppendText(EventLogFileName))
                     {
                         sw.WriteLine(RawText);
                     }*/
                 }
                 });
        }

        private void IpTableSavingXml()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement(@"IpTable");
            int Ind = 1;
            string NumState, StrState;
            doc.AppendChild(root);
            IpTable.ForEach(x =>
            {
                XmlElement newNode = doc.CreateElement(@"Row");
                newNode.SetAttribute(@"MachineName", x.MachineName);
                newNode.SetAttribute(@"IpAddress", x.IpAddress);
                NumState = x.IpStatus == IPStatus.Success ? @"1" : @"0";
                StrState = x.IpStatus == IPStatus.Success ? @"true" : @"false";
                newNode.InnerText = NumState;
                root.AppendChild(newNode);
                s61850.ModifySpsValue($"CSharp/STGGIO1.Ind{Ind}", StrState);
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
                //m61850 = new m61850("172.21.37.1");
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
