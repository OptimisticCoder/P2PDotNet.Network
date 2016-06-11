#pragma warning disable 0618 // stop Dns.Resolve obsolete warning

namespace P2PDotNet.Network
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Net;
    using System.Net.Sockets;

    using Nodes;

    public class P2PListener
    {
        private List<Thread> listenerThreads = new List<Thread>();

        public delegate void LogHandler(String text);
        public event LogHandler Log;

        public delegate void NewConnectionHandler(P2PServerNode node);
        public event NewConnectionHandler NewConnection;

        private void OnLog(String text)
        {
            if (Log != null)
                Log(text);
        }

        private void OnNewConnection(P2PServerNode node)
        {
            if (NewConnection != null)
                NewConnection(node);
        }

        public void Listen(Int32 incomingPort)
        {
            // start a listener thread on each local ip address
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            foreach (var ip in ipHostInfo.AddressList)
            {
                var listenerThread = new Thread(() => listenForIncoming(ip, incomingPort));
                listenerThread.Start();
                listenerThreads.Add(listenerThread);
            }
        }

        public void Stop()
        {
            // abort all of the listeners
            foreach (var t in listenerThreads)
                t.Abort();
        }

        private void listenForIncoming(IPAddress ip, Int32 port)
        {
            IPEndPoint localEndPoint = new IPEndPoint(ip, port);
            Socket listener = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Stream,
                                         ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
            }
            catch (Exception e)
            {
                OnLog(e.ToString());
                return;
            }

            OnLog("Waiting for connections on " + localEndPoint.ToString() + " ...");

            // Start an asynchronous socket to listen for connections.
            listener.BeginAccept(new AsyncCallback(acceptCallback), listener);
        }

        private void acceptCallback(IAsyncResult ar)
        {
            var listener = (Socket)ar.AsyncState;
            var socket = listener.EndAccept(ar);

            // create server node
            var server = new P2PServerNode(socket);
            OnNewConnection(server);

            listener.BeginAccept(new AsyncCallback(acceptCallback), listener);
        }
    }
}
