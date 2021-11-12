using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;

namespace LEDController.Model
{

    struct LEDStats
    {
        public double[] fixLEDPower;
        public double[] dimLEDPower;
        public double[] fixLEDVoltage;
        public double[] dimLEDVoltage;
        public double[] fixLEDCurrent;
        public double[] dimLEDCurrent;
        public double[] temperature;
    }


    public class Config
    {
        // read INI file
        public Dictionary<string, string> configData;
        public string fullFileName;
        public Config(string _fileName)
        {
            configData = new Dictionary<string, string>();
            fullFileName = _fileName;
            bool hasCfgFile = File.Exists(fullFileName);
            if (hasCfgFile == false)
            {
                StreamWriter writer = new StreamWriter(File.Create(fullFileName), Encoding.Default);
                writer.Close();
            }
            StreamReader reader = new StreamReader(fullFileName, Encoding.Default);
            string line;

            int index = 0;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith(";") || string.IsNullOrEmpty(line))
                    configData.Add(";" + index++, line);
                else
                {
                    string[] key_value = line.Split('=');
                    if (key_value.Length >= 2)
                        configData.Add(key_value[0], key_value[1]);
                    else
                        configData.Add(";" + index++, line);
                }
            }
            reader.Close();
        }

        public string get(string key)
        {
            if (configData.Count <= 0)
                return null;
            else if (configData.ContainsKey(key))
                return configData[key].ToString();
            else
                return null;
        }

        public void set(string key, string value)
        {
            if (configData.ContainsKey(key))
                configData[key] = value;
            else
                configData.Add(key, value);
        }

        public void save()
        {
            StreamWriter writer = new StreamWriter(fullFileName, false, Encoding.Default);
            IDictionaryEnumerator enu = configData.GetEnumerator();
            while (enu.MoveNext())
            {
                if (enu.Key.ToString().StartsWith(";"))
                    writer.WriteLine(enu.Value);
                else
                    writer.WriteLine(enu.Key + "=" + enu.Value);
            }
            writer.Close();
        }
    }

    class LEDBoardCom
    {
        private string slaveIP;
        private string slavePort;
        private const int SendBufferSize = 2 * 1024;
        private const int RecBufferSize = 8 * 1024;
        Socket socketHost = null;
        Thread threadHost = null;
        public string strRecMsg = null;
        private const double LEDVoltageConvertFactor = 1;
        private const double LEDCurrentConvertFactor = 1;
        private const double LEDPowerConvertFactor = 1;
        private const double TempConvertFactor = 1;

        public LEDBoardCom(string SlaveIP, string SlavePort)
        {
            // Verify Program Version
            // Test port occupation
            slaveIP = SlaveIP;
            slavePort = SlavePort;
        }

        private void Connect()
        {
            // Start TCP connection

            // Initialize a socket for communication
            socketHost = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress slaveIPAddress = IPAddress.Parse(slaveIP);
            IPEndPoint endPoint = new IPEndPoint(slaveIPAddress, int.Parse(slavePort));

            // Send connection request
            socketHost.Connect(endPoint);

            // Create a new thread for watching coming messages
            threadHost = new Thread(ReceiveMsg);
            threadHost.IsBackground = true;

            // Start thread for listening
            threadHost.Start();
        }

        private void Close()
        {
            socketHost.Close(1);
        }

        private void ReceiveMsg(object socketHostObj)
        {
            // Socket socketSlave = socketHostObj as Socket;
            byte[] buffer = new byte[SendBufferSize];

            while (true)   // Receiving message from slave
            {
                int msgLen = 0;

                try
                {
                    if (socketHost != null) msgLen = socketHost.Receive(buffer);

                    if (msgLen > 0)
                    {
                        // TODO add timestamp
                        strRecMsg = Encoding.UTF8.GetString(buffer);
                    }
                }
                catch (SocketException ex)
                {
                    // TODO: Write Log
                    throw ex;
                }
            }
        }

        private void SendCmd(string sendMsg)
        {
            try
            {
                byte[] arrCmd = Encoding.UTF8.GetBytes(sendMsg);
                socketHost.Send(arrCmd);
            }
            catch (Exception ex)
            {
                // TODO Write Log
                throw ex;
            }
        }

        public void TurnOnFixLED(int[] LEDInd)
        {}

        public void TurnOnAllFixLED()
        {}

        public void TurnOffAllFixLED()
        {}

        public void TurnOnDimLED(int[] LEDInd, int[] LEDBrightness)
        {}

        public void TurnOnDimLED()
        {}

        public void TurnOnDimLED(int LEDBrightness = 125)
        {}

        private string MakeCmd()
        {
            return "00";
        }

        public LEDStats ParseStats()
        {
            LEDStats currentLEDStats;
            currentLEDStats.dimLEDCurrent = new double[] {1, 2, 3};
            currentLEDStats.fixLEDCurrent = new double[] {1, 2, 3};
            currentLEDStats.fixLEDVoltage = new double[] {1, 2, 3};
            currentLEDStats.dimLEDVoltage = new double[] {1, 2, 3};
            currentLEDStats.fixLEDPower = new double[] {1, 2, 3};
            currentLEDStats.dimLEDPower = new double[] {1, 2, 3};
            currentLEDStats.temperature = new double[] {1, 2, 3};

            return currentLEDStats;
        }

        private IPAddress GetLocalIPv4Address()
        {
            IPAddress localIPv4 = null;
            // Get all IP addresses
            IPAddress[] ipAddressList = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ipAddress in ipAddressList)
            {
                // Iterate all IP addresses
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIPv4 = ipAddress;
                }
                else
                {
                    continue;
                }
            }

            return localIPv4;
        }


        private DateTime GetCurrentTime()
        {
            // Get current timestamp
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;

            return currentTime;
        }
    }

}
