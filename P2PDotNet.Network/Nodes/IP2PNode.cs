namespace P2PDotNet.Network.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public delegate void ReceivedDataHandler(IP2PNode node, Byte[] data);
    public delegate void DisconnectedHandler(Guid nodeId);
    public delegate void LogHandler(IP2PNode node, String text);

    public interface IP2PNode
    {
        event ReceivedDataHandler ReceivedData;
        event DisconnectedHandler Disconnected;
        event LogHandler Log;

        Guid NodeId { get; }

        void Send(Byte[] packet);

        void Shutdown();
    }
}
