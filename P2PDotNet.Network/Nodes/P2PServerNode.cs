namespace P2PDotNet.Network.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;

    using Helpers;

    public class P2PServerNode : IP2PNode
    {
        #region Private members

        // the socket to be used for an individual incoming connection
        private Socket serverSocket = null;

        // the semaphore object for locking the send queue
        private Object sendQueueLock = new Object();

        // a queue of packets waiting to be sent at the next opportunity
        // format is size:data:size:data... (size is a DWORD)
        private List<Byte> sendQueue = new List<Byte>();

        // buffer for receiving incoming data
        private Byte[] buffer = null;

        private IP2PNodeHelper helper = null;

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

        public event DisconnectedHandler Disconnected;
        public event LogHandler Log;
        public event ReceivedDataHandler ReceivedData;

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

        public P2PServerNode(Socket s)
        {
            serverSocket = s;
            helper = new P2PNodeHelper();
        }

        public P2PServerNode(Socket s, IP2PNodeHelper h)
        {
            serverSocket = s;
            helper = h;
        }

        #endregion

        #region Public methods

        public void Send(Byte[] data)
        {
            lock (sendQueueLock)
            {
                sendQueue.AddRange(BitConverter.GetBytes(data.Length));
                sendQueue.AddRange(data);
            }
        }

        public void Shutdown()
        {
            if (helper.IsConnected(serverSocket))
                helper.ShutdownSocket(serverSocket);
            helper.CloseSocket(serverSocket);
        }

        public void Run()
        {
            // start the server node, by beginning an async receive.
            buffer = new Byte[2048];
            helper.BeginReceive(buffer, new AsyncCallback(receiveCallback), serverSocket);
        }

        #endregion

        #region Protected socket callbacks

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
                // socket has become diconnected.
                OnDisconnected(instanceNodeId);
            }
            catch (ObjectDisposedException)
            {
                // socket/thread is being shut down.
                return;
            }

            // if nothing was received, the socket either closed, or some other problem, so return.
            if (receivedBytes == 0)
                return;

            OnReceivedData(this, buffer);

            // lock the send queue, build a byte array of the data and clear the queue.
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

        protected void sendCallback(IAsyncResult ar)
        {
            var socket = (Socket)ar.AsyncState;
            Int32 sentBytes = 0;
            try
            {
                sentBytes = helper.EndSend(ar, socket);
            }
            catch (SocketException)
            {
                // socket has become diconnected.
                OnDisconnected(instanceNodeId);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            // start receiving again
            buffer = new Byte[2048];
            try
            {
                helper.BeginReceive(buffer, new AsyncCallback(receiveCallback), socket);
            }
            catch (SocketException)
            {
                // socket has become diconnected.
                OnDisconnected(NodeId);
            }
            catch (ObjectDisposedException)
            {
                // socket/thread is being shut down. (no need to log this)
            }
        }

        #endregion
    }
}
