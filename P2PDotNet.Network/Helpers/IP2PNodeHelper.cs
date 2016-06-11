namespace P2PDotNet.Network.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Sockets;

    public interface IP2PNodeHelper
    {
        Boolean IsConnected(Socket s);

        void ShutdownSocket(Socket s);

        void CloseSocket(Socket s);

        void BeginConnect(IPEndPoint endpoint,
                          AsyncCallback callback,
                          Socket socket);

        void EndConnect(IAsyncResult ar, Socket socket);

        void BeginSend(Byte[] packet,
                       AsyncCallback callback,
                       Socket socket);

        Int32 EndSend(IAsyncResult ar, Socket socket);

        void BeginReceive(Byte[] packet,
                          AsyncCallback callback,
                          Socket socket);

        Int32 EndReceive(IAsyncResult ar, Socket socket);
    }
}
