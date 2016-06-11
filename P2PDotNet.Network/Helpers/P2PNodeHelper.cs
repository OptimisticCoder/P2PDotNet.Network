namespace P2PDotNet.Network.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Net;
    using System.Net.Sockets;

    public class P2PNodeHelper : IP2PNodeHelper
    {
        public Boolean IsConnected(Socket s)
        {
            return s.Connected;
        }

        public void ShutdownSocket(Socket s)
        {
            s.Shutdown(SocketShutdown.Both);
        }

        public void CloseSocket(Socket s)
        {
            s.Close();
        }

        public void BeginConnect(IPEndPoint endpoint, AsyncCallback callback, Socket socket)
        {
            socket.BeginConnect(endpoint, callback, socket);
        }

        public void EndConnect(IAsyncResult ar, Socket socket)
        {
            socket.EndConnect(ar);
        }

        public void BeginSend(Byte[] packet, AsyncCallback callback, Socket socket)
        {
            socket.BeginSend(packet,
                             0,
                             packet.Length,
                             SocketFlags.None,
                             new AsyncCallback(callback),
                             socket);
        }

        public Int32 EndSend(IAsyncResult ar, Socket socket)
        {
            return socket.EndSend(ar);
        }

        public void BeginReceive(Byte[] buffer, AsyncCallback callback, Socket socket)
        {
            socket.BeginReceive(buffer,
                                0,
                                buffer.Length,
                                SocketFlags.None,
                                new AsyncCallback(callback),
                                socket);
        }

        public Int32 EndReceive(IAsyncResult ar, Socket socket)
        {
            return socket.EndReceive(ar);
        }
    }
}
