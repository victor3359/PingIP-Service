using System.Net.NetworkInformation;

namespace PingIP_Service
{
    class IpDevice
    {
        public string MachineName;
        public string IpAddress;
        public IPStatus IpStatus;
        public IpDevice(string m, string i)
        {
            MachineName = m;
            IpAddress = i;            
        }
    }
}
