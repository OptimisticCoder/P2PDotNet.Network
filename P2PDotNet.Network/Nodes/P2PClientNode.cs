namespace P2PDotNet.Network.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;

    using Helpers;

    /// <summary>
    /// Client implementation of IP2PNode for managing sockets for outgoing connection instance
    /// </summary>
    public class P2PClientNode : IP2PNode
    {
        #region Private / Protected members

        // the socket to be used for an individual outgoing connection
        private Socket clientSocket = null;

        // the semaphore object for locking the send queue
        private Object sendQueueLock = new Object();

        // a queue of packets waiting to be sent at the next opportunity
        // format is size:data:size:data... (size is a DWORD)
        protected List<Byte> sendQueue = new List<Byte>();

        // buffer for receiving incoming data
        private Byte[] buffer = null;

        private IP2PNodeHelper helper = null;

        protected Boolean hasNetworkCycleStarted = false;

        private Guid instanceNodeId = Guid.NewGuid();

        #endregion

        #region Public properties

        public Guid NodeId
        {
            get
            {
                return instanceNodeId;
            }
        }

        #endregion

        #region Events

        public delegate void ConnectedHandler(P2PClientNode node);
        public event ConnectedHandler Connected;
        public event DisconnectedHandler Disconnected;
        public event LogHandler Log;
        public event ReceivedDataHandler ReceivedData;

        private void OnConnected(P2PClientNode node)
        {
            if (Connected != null)
                Connected(node);
        }

        private void OnDisconnected(Guid nodeId)
        {
            if (Disconnected != null)
                Disconnected(nodeId);
        }

        private void OnLog(IP2PNode node, String text)
        {
            if (Log != null)
                Log(node, text);
        }

        private void OnReceivedData(IP2PNode node, Byte[] data)
        {
            if (ReceivedData != null)
                ReceivedData(node, data);
        }

        #endregion

        #region Constructors

        public P2PClientNode()
        {
            helper = new P2PNodeHelper();
        }

        public P2PClientNode(IP2PNodeHelper h)
        {
            helper = h;
        }

        #endregion

        #region Public methods

        public void Connect(IPAddress ip, Int32 port)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork,
                                      SocketType.Stream,
                                      ProtocolType.Tcp);
            helper.BeginConnect(new IPEndPoint(ip, port),
                                new AsyncCallback(connectCallback),
                                clientSocket);
        }

        public void Send(Byte[] data)
        {
            lock (sendQueueLock)
            {
                sendQueue.AddRange(BitConverter.GetBytes(data.Length));
                sendQueue.AddRange(data);
            }

            if (!hasNetworkCycleStarted && helper.IsConnected(clientSocket))
            {
                hasNetworkCycleStarted = true;

                Byte[] outgoing = null;
                lock (sendQueueLock)
                {
                    outgoing = sendQueue.ToArray();
                    sendQueue.Clear();
                }

                helper.BeginSend(outgoing, new AsyncCallback(sendCallback), clientSocket);
            }
        }

        public void Shutdown()
        {
            if (helper.IsConnected(clientSocket))
                helper.ShutdownSocket(clientSocket);
            helper.CloseSocket(clientSocket);
        }

        #endregion

        #region Protected socket callbacks

        protected void connectCallback(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;

            try
            {
                helper.EndConnect(ar, socket);
            }
            catch (SocketException ex)
            {
                OnLog(this, ex.ToString());
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            OnConnected(this);
        }

        protected void sendCallback(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;
            try
            {
                helper.EndSend(ar, socket);
            }
            catch (SocketException)
            {
                OnDisconnected(instanceNodeId);
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            buffer = new Byte[2048];

            try
            {
                helper.BeginReceive(buffer,
                                    new AsyncCallback(receiveCallback),
                                    socket);
            }
            catch (SocketException)
            {
                OnDisconnected(instanceNodeId);
                return;
            }
        }

        protected void receiveCallback(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;
            Int32 receivedBytes = 0;
            try
            {
                receivedBytes = helper.EndReceive(ar, socket);
            }
            catch (SocketException)
            {
                OnDisconnected(instanceNodeId);
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            if (receivedBytes == 0)
                return;


            OnReceivedData(this, buffer);

            // throttle the connection cycle
            System.Threading.Thread.Sleep(500);

            Byte[] outgoing = null;
            lock (sendQueueLock)
            {
                outgoing = sendQueue.ToArray();
                sendQueue.Clear();
            }

            // send a response
            try
            {
                helper.BeginSend(outgoing, new AsyncCallback(sendCallback), socket);
            }
            catch (SocketException)
            {
                // socket has become diconnected.
                OnDisconnected(instanceNodeId);
            }
            catch (ObjectDisposedException)
            {
                // socket/thread is being shut down. (no need to log this)
            }
        }

        #endregion
    }
}
