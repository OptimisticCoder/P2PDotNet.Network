namespace P2PDotNet.Network.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.IO;
    using System.Net;

    public class P2PHostHelper : IP2PHostHelper
    {
        public String ReadFile(String filePath)
        {
            return File.ReadAllText(filePath);
        }

        public IPAddress[] GetHostAddresses(String hostName)
        {
            return Dns.GetHostAddresses(hostName);
        }
    }
}
