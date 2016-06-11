namespace P2PDotNet.Network
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Helpers;

    public class P2PHost
    {
        public String Ip { get; set; }
        public Int32 Port { get; set; }

        private IP2PHostHelper helper = null;

        public P2PHost()
        {
            helper = new P2PHostHelper();
        }

        public P2PHost(IP2PHostHelper h)
        {
            helper = h;
        }

        public IEnumerable<P2PHost> LoadAll(String filePath, IEnumerable<String> dnsSeeds, Int32 defaultPort)
        {
            var hosts = new List<P2PHost>();

            // try to load the application hosts data file.
            String raw = String.Empty;
            try
            {
                raw = helper.ReadFile(filePath);
            }
            catch(Exception)
            {
                // hosts file probably doesn't exist
            }

            // if we got some data, deserialize it into P2PHost objects.
            if (!String.IsNullOrEmpty(raw))
            {
                P2PHost[] fromFile = null;
                fromFile = JsonConvert.DeserializeObject<P2PHost[]>(raw);

                if (fromFile != null)
                {
                    foreach (var h in fromFile)
                    {
                        if (!String.IsNullOrEmpty(h.Ip) && h.Port > 0)
                            hosts.Add(h);
                    }
                }
            }

            // iterate through the dns seed domains/subdomains
            if (dnsSeeds != null)
            {
                foreach (var s in dnsSeeds)
                {
                    // lookup the domain A records, and get back an array of ip addresses.
                    var addresses = helper.GetHostAddresses(s);
                    foreach (var a in addresses)
                    {
                        // the dns seeds must be on the default port, cus we can't store 
                        // ports in a dns record
                        hosts.Add(new P2PHost { Ip = a.ToString(), Port = defaultPort });
                    }
                }
            }

            return hosts;
        }
    }
}
