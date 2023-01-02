using Microsoft.VisualStudio.TestTools.UnitTesting;
using LEDController.Model;
using System.IO;
using System;
using System.Net;
using System.Net.Sockets;

namespace LEDControllerTest
{
    [TestClass]
    public class LEDBoardComTests
    {
        [TestMethod]
        public void TestReadINICfg()
        {
            string workingDir = Environment.CurrentDirectory;
            string projectDir = Directory.GetParent(workingDir).Parent.Parent.FullName;

            string iniTestFile = Path.Combine(projectDir, "testCfg.ini");

            Config cfgReader = new Config(iniTestFile);

            Assert.AreEqual(cfgReader.configData["FixLED1"], "true");
        }

        [TestMethod]
        public void TestConnect()
        {
            TcpListener server = null;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            Int32 port = (Int32)IpUtilities.GetAvailablePort();

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            LEDBoardCom client = new LEDBoardCom("127.0.0.1", Convert.ToString(port));

            client.Connect();

            Assert.IsNotNull(client.device);

            client.Disconnect();
            server.Stop();
        }

    }
}
