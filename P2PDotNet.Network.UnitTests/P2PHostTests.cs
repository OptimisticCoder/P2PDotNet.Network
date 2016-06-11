namespace P2PDotNet.Network.UnitTests
{
    using System;
    using System.Linq;
    using System.Net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;
    using Moq.Linq;
    
    using P2PDotNet.Network;
    using P2PDotNet.Network.Helpers;

    [TestClass]
    public class P2PHostTests
    {
        [TestMethod]
        public void LoadAll_EmptyParams_ReturnEmptyArray()
        {
            // Setup the mock helper
            var mockHelper = new Mock<IP2PHostHelper>();

            // Setup the expected method returns
            mockHelper.Setup(d => d.ReadFile(It.IsAny<String>()))
                      .Throws(new System.IO.FileNotFoundException());

            var classInTest = new P2PHost(mockHelper.Object);
            var results = classInTest.LoadAll(String.Empty, null, 0);

            mockHelper.VerifyAll();

            Assert.IsTrue(results.Count() == 0);
        }

        [TestMethod]
        public void LoadAll_EntryInFile_ReturnSingleObject()
        {
            // Setup the mock helper
            var mockHelper = new Mock<IP2PHostHelper>();

            // create a response for ReadFile.
            var rawJson = "[ { \"Ip\": \"127.0.0.1\", \"Port\": 666 } ]";

            // Setup the expected method returns
            mockHelper.Setup(d => d.ReadFile(It.IsAny<String>()))
                      .Returns(rawJson);

            var classInTest = new P2PHost(mockHelper.Object);
            var results = classInTest.LoadAll("test-hosts", new String[] {}, 0);

            mockHelper.VerifyAll();

            Assert.IsTrue(results.Count() == 1);
            Assert.IsTrue(results.FirstOrDefault().Ip == "127.0.0.1");
            Assert.IsTrue(results.FirstOrDefault().Port == 666);
        }

        [TestMethod]
        public void LoadAll_BrokenEntryInFile_ReturnEmptyArray()
        {
            // Setup the mock helper
            var mockHelper = new Mock<IP2PHostHelper>();

            // create a broken record response for ReadFile (ip is missing).
            var rawJson = "[ { \"Ip\": \"\", \"Port\": 666 } ]";

            // Setup the expected method returns
            mockHelper.Setup(d => d.ReadFile(It.IsAny<String>()))
                      .Returns(rawJson);

            var classInTest = new P2PHost(mockHelper.Object);
            var results = classInTest.LoadAll("test-hosts", new String[] { }, 0);

            mockHelper.VerifyAll();

            Assert.IsTrue(results.Count() == 0);
        }

        [TestMethod]
        public void LoadAll_DnsSeedAndEntryInFile_ReturnTwo()
        {
            // Setup the mock helper
            var mockHelper = new Mock<IP2PHostHelper>();

            // create a response for ReadFile.
            var rawJson = "[ { \"Ip\": \"192.168.0.0\", \"Port\": 1337 } ]";

            // Setup the expected method returns
            mockHelper.Setup(d => d.ReadFile(It.IsAny<String>()))
                      .Returns(rawJson);

            mockHelper.Setup(d => d.GetHostAddresses(It.IsAny<String>()))
                      .Returns(new IPAddress[] {
                          IPAddress.Parse("127.0.0.1")
                      });

            var classInTest = new P2PHost(mockHelper.Object);
            var results = classInTest.LoadAll("test-hosts", new String[] { "dnsseed.test.com" }, 31337);

            mockHelper.VerifyAll();

            Assert.IsTrue(results.Count() == 2);
            Assert.IsTrue(results.Where(i => i.Ip == "192.168.0.0").FirstOrDefault().Port == 1337);
            Assert.IsTrue(results.Where(i => i.Ip == "127.0.0.1").FirstOrDefault().Port == 31337);
        }
    }
}
