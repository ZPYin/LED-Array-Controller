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

        [TestMethod]
        public void TestTurnOnFixLED()
        {
            TcpListener server = null;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            Int32 port = (Int32)IpUtilities.GetAvailablePort();

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            var cmdBytes = new byte[11];
            cmdBytes[0] = 0xFF;
            cmdBytes[1] = 0x7E;
            cmdBytes[2] = 0x05;
            cmdBytes[3] = 0x30;
            cmdBytes[4] = 0x01;
            cmdBytes[5] = 0x01;
            cmdBytes[6] = 0x00;
            cmdBytes[7] = 0x00;
            cmdBytes[8] = 0xCC;
            cmdBytes[9] = 0xFF;
            cmdBytes[10] = 0xEF;
            LEDBoardCom client = new LEDBoardCom("127.0.0.1", Convert.ToString(port));

            client.Connect();

            Assert.AreEqual(Convert.ToBase64String(client.MakeCmd(1, true)), Convert.ToBase64String(cmdBytes));

            client.Disconnect();
            server.Stop();
        }

        [TestMethod]
        public void TestTurnOffFixLED()
        {
            TcpListener server = null;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            Int32 port = (Int32)IpUtilities.GetAvailablePort();

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            var cmdBytes = new byte[11];
            cmdBytes[0] = 0xFF;
            cmdBytes[1] = 0x7E;
            cmdBytes[2] = 0x05;
            cmdBytes[3] = 0x30;
            cmdBytes[4] = 0x01;
            cmdBytes[5] = 0x00;
            cmdBytes[6] = 0x00;
            cmdBytes[7] = 0x00;
            cmdBytes[8] = 0xCD;
            cmdBytes[9] = 0xFF;
            cmdBytes[10] = 0xEF;
            LEDBoardCom client = new LEDBoardCom("127.0.0.1", Convert.ToString(port));

            client.Connect();

            Assert.AreEqual(Convert.ToBase64String(client.MakeCmd(1, false)), Convert.ToBase64String(cmdBytes));

            client.Disconnect();
            server.Stop();
        }


        [TestMethod]
        public void TestSetDimLED()
        {
            TcpListener server = null;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            Int32 port = (Int32)IpUtilities.GetAvailablePort();

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();
            var cmdBytes = new byte[11];
            cmdBytes[0] = 0xFF;
            cmdBytes[1] = 0x7E;
            cmdBytes[2] = 0x05;
            cmdBytes[3] = 0x31;
            cmdBytes[4] = 0x81;
            cmdBytes[5] = 0x01;
            cmdBytes[6] = 0x00;
            cmdBytes[7] = 0x00;
            cmdBytes[8] = 0x4B;
            cmdBytes[9] = 0xFF;
            cmdBytes[10] = 0xEF;
            LEDBoardCom client = new LEDBoardCom("127.0.0.1", Convert.ToString(port));

            client.Connect();

            Assert.AreEqual(Convert.ToBase64String(client.MakeCmd(1, 1, true)), Convert.ToBase64String(cmdBytes));

            client.Disconnect();
            server.Stop();
        }

        [TestMethod]
        public void TestTurnOffDimLED()
        {
            TcpListener server = null;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            Int32 port = (Int32)IpUtilities.GetAvailablePort();

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();
            var cmdBytes = new byte[11];
            cmdBytes[0] = 0xFF;
            cmdBytes[1] = 0x7E;
            cmdBytes[2] = 0x05;
            cmdBytes[3] = 0x31;
            cmdBytes[4] = 0x81;
            cmdBytes[5] = 0x00;
            cmdBytes[6] = 0x00;
            cmdBytes[7] = 0x00;
            cmdBytes[8] = 0x4C;
            cmdBytes[9] = 0xFF;
            cmdBytes[10] = 0xEF;
            LEDBoardCom client = new LEDBoardCom("127.0.0.1", Convert.ToString(port));

            client.Connect();

            Assert.AreEqual(Convert.ToBase64String(client.MakeCmd(1, 0, false)), Convert.ToBase64String(cmdBytes));

            client.Disconnect();
            server.Stop();
        }
    }
}
