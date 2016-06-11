namespace P2PDotNet.Network.UnitTests.Nodes
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;
    using Moq.Linq;
    
    using P2PDotNet.Network;
    using P2PDotNet.Network.Nodes;
    using P2PDotNet.Network.Helpers;

    [TestClass]
    public class P2PClientNodeTests
    {
        [TestMethod]
        public void Connect_DummyIpAndPort_SocketBeginConnectIsCalled()
        {
            // Setup the mock helper
            var mockHelper = new Mock<IP2PNodeHelper>();

            // Setup the expected method returns
            mockHelper.Setup(d => d.BeginConnect(It.IsAny<IPEndPoint>(), It.IsAny<AsyncCallback>(), It.IsAny<Socket>()));

            var classInTest = new P2PClientNode(mockHelper.Object);
            classInTest.Connect(IPAddress.Parse("127.0.0.1"), 1337);

            mockHelper.VerifyAll();
        }

        [TestMethod]
        public void Send_ByteArray_DataAddedToQueue()
        {
            // Setup the mock helper
            var mockHelper = new Mock<IP2PNodeHelper>();

            // Setup the expected method returns
            //mockHelper.Setup(d => d.BeginConnect(It.IsAny<IPEndPoint>(), It.IsAny<AsyncCallback>(), It.IsAny<Socket>()));

            Byte[] someData = new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            var classInTest = new P2PClientNodeHarness(mockHelper.Object);
            classInTest.HasNetworkCycleStarted = true;
            classInTest.Send(someData);

            mockHelper.VerifyAll();

            // size = 4 bytes plus 10 bytes of data
            Assert.AreEqual(14, classInTest.SendQueue.Count);
        }

        [TestMethod]
        public void Send_ByteArrayCycleNotStarted_DataSent()
        {
            // Setup the mock helper
            var mockHelper = new Mock<IP2PNodeHelper>();

            // Setup the expected method returns
            mockHelper.Setup(d => d.BeginSend(It.IsAny<Byte[]>(), It.IsAny<AsyncCallback>(), It.IsAny<Socket>()));

            mockHelper.Setup(d => d.IsConnected(It.IsAny<Socket>()))
                      .Returns(true);

            Byte[] someData = new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            var classInTest = new P2PClientNodeHarness(mockHelper.Object);
            classInTest.HasNetworkCycleStarted = false;
            classInTest.Send(someData);

            mockHelper.VerifyAll();

            // queue should be clear after the data is sent
            Assert.AreEqual(0, classInTest.SendQueue.Count);
        }

        [TestMethod]
        public void Shutdown_SocketIsConnected_ExecuteSocketShutdown()
        {
            // Setup the mock helper
            var mockHelper = new Mock<IP2PNodeHelper>();

            // Setup the expected method returns
            mockHelper.Setup(d => d.CloseSocket(It.IsAny<Socket>()));

            mockHelper.Setup(d => d.ShutdownSocket(It.IsAny<Socket>()));

            mockHelper.Setup(d => d.IsConnected(It.IsAny<Socket>()))
                      .Returns(true);

            Byte[] someData = new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            var classInTest = new P2PClientNodeHarness(mockHelper.Object);
            classInTest.Shutdown();

            mockHelper.VerifyAll();
        }

        [TestMethod]
        public void Shutdown_SocketNotConnected_SkipSocketShutdown()
        {
            // Setup the mock helper
            var mockHelper = new Mock<IP2PNodeHelper>();

            // Setup the expected method returns
            mockHelper.Setup(d => d.CloseSocket(It.IsAny<Socket>())).Verifiable();

            mockHelper.Setup(d => d.ShutdownSocket(It.IsAny<Socket>()))
                      .Throws(new AssertFailedException("Shutdown socket was executed, it should of been skipped."));

            mockHelper.Setup(d => d.IsConnected(It.IsAny<Socket>()))
                      .Returns(false)
                      .Verifiable();

            Byte[] someData = new Byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

            var classInTest = new P2PClientNodeHarness(mockHelper.Object);
            classInTest.Shutdown();

            mockHelper.Verify();
        }

    }
}
