namespace P2PDotNet.Network.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net;

    public interface IP2PHostHelper
    {
        String ReadFile(String filePath);

        IPAddress[] GetHostAddresses(String hostName);
    }
}
