namespace P2PDotNet.Network.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using P2PDotNet.Network;
    using P2PDotNet.Network.Nodes;
    using P2PDotNet.Network.Helpers;

    public class P2PClientNodeHarness : P2PClientNode
    {
        public P2PClientNodeHarness(IP2PNodeHelper h) : base(h)
        { }

        public List<Byte> SendQueue
        {
            get
            {
                return sendQueue;
            }

            set
            {
                sendQueue = value;
            }
        }

        public Boolean HasNetworkCycleStarted
        {
            get
            {
                return hasNetworkCycleStarted;
            }

            set
            {
                hasNetworkCycleStarted = value;
            }
        }
    }
}
