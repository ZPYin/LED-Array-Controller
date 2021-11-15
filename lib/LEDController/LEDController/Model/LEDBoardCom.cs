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

    struct LEDStatus
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
        public Socket socketHost = null;
        public string strRecMsg = null;
        private const double LEDVoltageConvertFactor = 1;
        private const double LEDCurrentConvertFactor = 1;
        private const double LEDPowerConvertFactor = 1;
        private const double TempConvertFactor = 1;
        private const int NumFixLED = 120;
        private const int NumDimLED = 12;

        public LEDBoardCom(string SlaveIP, string SlavePort)
        {
            // Verify Program Version
            // Test port occupation
            slaveIP = SlaveIP;
            slavePort = SlavePort;
        }

        public void Connect(int timeout = 2)
        {
            // Start TCP connection

            // Initialize a socket for communication
            socketHost = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress slaveIPAddress = IPAddress.Parse(slaveIP);
            IPEndPoint endPoint = new IPEndPoint(slaveIPAddress, int.Parse(slavePort));

            // Send connection request
            var result = socketHost.BeginConnect(endPoint, null, null);
            bool isSuccess = result.AsyncWaitHandle.WaitOne(timeout, true);
            if (! isSuccess)
            {
                socketHost.Close();
                throw new SocketException(10060);
            }
        }

        public void Close()
        {
            socketHost.Close(1);
        }

        public void SendCmd(string sendMsg)
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

        public LEDStatus ParseStatus(byte[] receiveMsg)
        {

            LEDStatus currentLEDStats;
            currentLEDStats.dimLEDCurrent = new double[] {1, 2, 3};
            currentLEDStats.fixLEDCurrent = new double[] {1, 2, 3};
            currentLEDStats.fixLEDVoltage = new double[] {1, 2, 3};
            currentLEDStats.dimLEDVoltage = new double[] {1, 2, 3};
            currentLEDStats.fixLEDPower = new double[] {1, 2, 3};
            currentLEDStats.dimLEDPower = new double[] {1, 2, 3};
            currentLEDStats.temperature = new double[] {1, 2, 3};

            return currentLEDStats;
        }

    }

}
