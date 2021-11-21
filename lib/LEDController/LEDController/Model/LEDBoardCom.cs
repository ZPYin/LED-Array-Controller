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
        public Boolean isValidPackage;
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
        public string slaveIP;
        public string slavePort;
        public Socket socketHost = null;
        public string strRecMsg = null;
        private const double LEDVoltageConvertFactor = 1;
        private const double LEDCurrentConvertFactor = 1;
        private const double LEDPowerConvertFactor = 1;
        private const double TempConvertFactor = 1;
        public const int NumGreenFixLED = 40;
        public const int NumRedFixLED = 40;
        public const int NumDarkRedFixLED = 40;
        public const int NumGreenDimLED = 4;
        public const int NumRedDimLED = 4;
        public const int NumDarkRedDimLED = 4;
        public Boolean isAlive = false;

        public LEDBoardCom(string SlaveIP, string SlavePort)
        {
            // Verify Program Version
            // Test port occupation
            slaveIP = SlaveIP;
            slavePort = SlavePort;
        }

        public LEDBoardCom()
        {}

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
            isAlive = true;
        }

        public void Close()
        {
            if (socketHost != null)
            {
                socketHost.Close(1);
                isAlive = false;
            }
        }

        public void SendCmd(string sendMsg)
        {
            try
            {
                byte[] arrCmd = Encoding.UTF8.GetBytes(sendMsg);
                if (socketHost != null)
                {
                    socketHost.Send(arrCmd);
                }
            }
            catch (Exception ex)
            {
                // TODO Write Log
                throw ex;
            }
        }

        public void SendCmd(byte[] cmdBytes)
        {
            try
            {
                if (socketHost != null)
                {
                    socketHost.Send(cmdBytes);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void TurnOnFixLED(int LEDInd)
        {
            var cmdBytes = MakeCmd(LEDInd, true);
            SendCmd(cmdBytes);
        }

        public void TurnOffFixLED(int LEDInd)
        {
            var cmdBytes = MakeCmd(LEDInd, false);
            SendCmd(cmdBytes);
        }

        public void TurnOnDimLED(int LEDInd, int LEDBrightness)
        {
            var cmdBytes = MakeCmd(LEDInd, LEDBrightness, true);
            SendCmd(cmdBytes);
        }

        public void TurnOffDimLED(int LEDInd)
        {
            var cmdBytes = MakeCmd(LEDInd, 0, false);
            SendCmd(cmdBytes);
        }

        private byte[] MakeCmd(int LEDInd, Boolean isTurnOn)
        {
            var cmdBytes = new byte[11];

            cmdBytes[0] = 0xFF;
            cmdBytes[1] = 0x7E;
            cmdBytes[2] = 0x05;
            cmdBytes[3] = 0x30;   // Command code
            cmdBytes[4] = Convert.ToByte(LEDInd);   // LED address
            cmdBytes[5] = Convert.ToByte(isTurnOn);
            cmdBytes[6] = 0x00;
            cmdBytes[7] = 0x00;

            UInt16 CHK = (UInt16)((UInt16)0xFFFF - ((UInt16)cmdBytes[3] + (UInt16)cmdBytes[4] + (UInt16)cmdBytes[5] + (UInt16)cmdBytes[6] + (UInt16)cmdBytes[7] + (UInt16)1));

            cmdBytes[8] = (byte)(CHK & 0x00FF);
            cmdBytes[9] = (byte)(CHK >> 8);
            cmdBytes[10] = 0xEF;

            return cmdBytes;
        }

        private byte[] MakeCmd(int LEDInd, int LEDBrightness, Boolean isTurnOn)
        {
            var cmdBytes = new byte[11];

            cmdBytes[0] = 0xFF;
            cmdBytes[1] = 0x7E;
            cmdBytes[2] = 0x05;
            cmdBytes[3] = 0x31;   // Command code
            cmdBytes[4] = Convert.ToByte(LEDInd + 8);   // LED address
            cmdBytes[5] = Convert.ToByte(isTurnOn);
            cmdBytes[6] = Convert.ToByte(LEDBrightness);
            cmdBytes[7] = 0x00;

            UInt16 CHK = (UInt16)((UInt16)0xFFFF - ((UInt16)cmdBytes[3] + (UInt16)cmdBytes[4] + (UInt16)cmdBytes[5] + (UInt16)cmdBytes[6] + (UInt16)cmdBytes[7] + (UInt16)1));

            cmdBytes[8] = (byte)(CHK & 0x00FF);
            cmdBytes[9] = (byte)(CHK >> 8);
            cmdBytes[10] = 0xEF;

            return cmdBytes;
        }

        public LEDStatus ParsePackage(byte[] recBytes)
        {
            int NumFixLED = NumGreenFixLED + NumRedFixLED + NumDarkRedFixLED;
            int NumDimLED = NumGreenDimLED + NumRedDimLED + NumDarkRedDimLED;

            LEDStatus recStatus;
            recStatus.fixLEDPower = new double[NumFixLED];
            recStatus.fixLEDCurrent = new double[NumFixLED];
            recStatus.fixLEDVoltage = new double[NumFixLED];
            recStatus.dimLEDPower = new double[NumDimLED];
            recStatus.dimLEDCurrent = new double[NumDimLED];
            recStatus.dimLEDVoltage = new double[NumDimLED];
            recStatus.temperature = new double[4];
            recStatus.isValidPackage = false;
            int pkgLen = Convert.ToInt16((recBytes[2] + recBytes[3] >> 8));

            Int32 sumData = 0;
            for (int i = 4; i < (4 + pkgLen + 1); i++)
            {
                sumData = Convert.ToInt32(recBytes[i]) + sumData;
            }

            UInt16 recCHK = (UInt16)((UInt16)0xFFFF - (UInt16)recBytes[4 + pkgLen + 1] - ((UInt16)recBytes[4 + pkgLen + 2]) - 1);

            if (sumData == (Int32)recCHK)
            {
                recStatus.isValidPackage = true;

                // fixed LED
                for (int i = 0; i < NumFixLED; i++)
                {
                    recStatus.fixLEDPower[i] = (double)recBytes[4 + i] * LEDPowerConvertFactor;
                    recStatus.fixLEDVoltage[i] = (double)recBytes[4 + i + NumFixLED + NumFixLED] * LEDVoltageConvertFactor;
                    recStatus.fixLEDCurrent[i] = (double)recBytes[4 + i + NumFixLED * 2 + NumFixLED * 2] * LEDCurrentConvertFactor;
                }

                // Dimmable LED
                for (int i = 0; i < NumDimLED; i++)
                {
                    recStatus.dimLEDPower[i] = (double)recBytes[4 + i + NumFixLED] * LEDPowerConvertFactor;
                    recStatus.dimLEDVoltage[i] = (double)recBytes[4 + i + NumFixLED * 2] * LEDVoltageConvertFactor;
                    recStatus.dimLEDCurrent[i] = (double)recBytes[4 + i + NumFixLED * 3] * LEDCurrentConvertFactor;
                }

                recStatus.temperature[0] = (double)recBytes[4 + NumFixLED * 3 + NumDimLED * 3 + 1] * TempConvertFactor;
                recStatus.temperature[1] = (double)recBytes[4 + NumFixLED * 3 + NumDimLED * 3 + 2] * TempConvertFactor;
                recStatus.temperature[2] = (double)recBytes[4 + NumFixLED * 3 + NumDimLED * 3 + 3] * TempConvertFactor;
                recStatus.temperature[3] = (double)recBytes[4 + NumFixLED * 3 + NumDimLED * 3 + 4] * TempConvertFactor;
            }

            return recStatus;
        }

    }

}
