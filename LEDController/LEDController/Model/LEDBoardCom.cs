using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace LEDController.Model
{

    public struct LEDStatus
    {
        public double[] fixLEDPower;
        public double[] dimLEDPower;
        public double[] fixLEDVoltage;
        public double[] dimLEDVoltage;
        public double[] fixLEDCurrent;
        public double[] dimLEDCurrent;
        public double[] temperature;
        public double totalGreenLEDPower;
        public double totalRedLEDPower;
        public double totalDarkredLEDPower;
        public Boolean isValidPackage;
    }

    public class LEDControllerCfg : LEDBoardCom
    {
        public Boolean[] isGreenFixLEDOn = new Boolean[LEDBoardCom.NumGreenFixLED];
        public Boolean[] isRedFixLEDOn = new Boolean[LEDBoardCom.NumRedFixLED];
        public Boolean[] isDarkRedFixLEDOn = new Boolean[LEDBoardCom.NumDarkRedFixLED];
        public double[] greenDimLEDPower = new double[LEDBoardCom.NumGreenDimLED];
        public double[] redDimLEDPower = new double[LEDBoardCom.NumRedDimLED];
        public double[] darkRedDimLEDPower = new double[LEDBoardCom.NumDarkRedDimLED];

        public LEDControllerCfg(string fileName)
        {
            Config cfgReader = new Config(fileName);

            foreach (var item in cfgReader.configData)
            {
                try
                {
                    if (item.Key.Equals("SlaveIP"))
                    {
                        slaveIP = item.Value;
                    }
                    else if (item.Key.Equals("SlavePort"))
                    {
                        slavePort = item.Value;
                    }
                    else if (item.Key.Contains("GreenFixLED"))
                    {
                        int LEDIndex = GetLEDIndex(item.Key, "GreenFixLED");

                        isGreenFixLEDOn[LEDIndex] = Convert.ToBoolean(item.Value);
                    }
                    else if ((item.Key.Contains("RedFixLED")) && (!item.Key.Contains("Dark")))
                    {
                        int LEDIndex = GetLEDIndex(item.Key, "RedFixLED");

                        isRedFixLEDOn[LEDIndex] = Convert.ToBoolean(item.Value);
                    }
                    else if (item.Key.Contains("DarkRedFixLED"))
                    {
                        int LEDIndex = GetLEDIndex(item.Key, "DarkRedFixLED");

                        isDarkRedFixLEDOn[LEDIndex] = Convert.ToBoolean(item.Value);
                    }
                    else if (item.Key.Contains("GreenDimLED"))
                    {
                        int LEDIndex = GetLEDIndex(item.Key, "GreenDimLED");

                        greenDimLEDPower[LEDIndex] = Convert.ToDouble(item.Value);
                    }
                    else if ((item.Key.Contains("RedDimLED")) && (!item.Key.Contains("Dark")))
                    {
                        int LEDIndex = GetLEDIndex(item.Key, "RedDimLED");

                        redDimLEDPower[LEDIndex] = Convert.ToDouble(item.Value);
                    }
                    else if (item.Key.Contains("DarkRedDimLED"))
                    {
                        int LEDIndex = GetLEDIndex(item.Key, "DarkRedDimLED");

                        darkRedDimLEDPower[LEDIndex] = Convert.ToDouble(item.Value);
                    }
                    else
                    {
                        Exception myEx = new Exception();
                        throw myEx;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public LEDControllerCfg()
        {}

        public int GetLEDIndex(string LEDStr, string searchPattern)
        {
            int LEDIndex;
            string LEDIndStr = LEDStr.Substring(LEDStr.IndexOf(searchPattern) + searchPattern.Count());
            LEDIndex = Convert.ToInt32(LEDIndStr) - 1;

            return LEDIndex;
        }

        public void LEDControllerCfgSave(string fileName)
        {
            Dictionary<string, string> LEDCfg = new Dictionary<string, string>();

            for (int i = 0; i < isGreenFixLEDOn.Count(); i++)
            {
                LEDCfg.Add($"GreenFixLED{i + 1}", Convert.ToString(isGreenFixLEDOn[i]));
            }

            for (int i = 0; i < isRedFixLEDOn.Count(); i++)
            {
                LEDCfg.Add($"RedFixLED{i + 1}", Convert.ToString(isRedFixLEDOn[i]));
            }

            for (int i = 0; i < isDarkRedFixLEDOn.Count(); i++)
            {
                LEDCfg.Add($"DarkRedFixLED{i + 1}", Convert.ToString(isDarkRedFixLEDOn[i]));
            }

            for (int i = 0; i < greenDimLEDPower.Count(); i++)
            {
                LEDCfg.Add($"GreenDimLED{i + 1}", Convert.ToString(greenDimLEDPower[i]));
            }

            for (int i = 0; i < redDimLEDPower.Count(); i++)
            {
                LEDCfg.Add($"RedDimLED{i + 1}", Convert.ToString(redDimLEDPower[i]));
            }

            for (int i = 0; i < darkRedDimLEDPower.Count(); i++)
            {
                LEDCfg.Add($"DarkRedDimLED{i + 1}", Convert.ToString(darkRedDimLEDPower[i]));
            }

            LEDCfg.Add("SlaveIP", slaveIP);
            LEDCfg.Add("SlavePort", slavePort);

            Config cfgWriter = new Config();

            cfgWriter.configData = LEDCfg;
            cfgWriter.fullFileName = fileName;
            cfgWriter.save();
        }
    }

    public static class IpUtilities
    {
        private const ushort MIN_PORT = 1;
        private const ushort MAX_PORT = UInt16.MaxValue;
        public static int? GetAvailablePort(ushort lowerPort = MIN_PORT, ushort upperPort = MAX_PORT)
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            var usedPorts = Enumerable.Empty<int>()
                .Concat(ipProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint.Port))
                .Concat(ipProperties.GetActiveTcpListeners().Select(l => l.Port))
                .Concat(ipProperties.GetActiveUdpListeners().Select(l => l.Port))
                .ToHashSet();
            for (int port = lowerPort; port <= upperPort; port++)
            {
                if (!usedPorts.Contains(port)) return port;
            }
            return null;
        }
    }

    public class LEDBoardCom
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

        public void SetDimLED(int LEDInd, int LEDBrightness)
        {
            var cmdBytes = MakeCmd(LEDInd, LEDBrightness, true);
            SendCmd(cmdBytes);
        }

        public void TurnOffDimLED(int LEDInd)
        {
            var cmdBytes = MakeCmd(LEDInd, 0, false);
            SendCmd(cmdBytes);
        }

        public byte[] MakeCmd(int LEDInd, Boolean isTurnOn)
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

        public byte[] MakeCmd(int LEDInd, int LEDBrightness, Boolean isTurnOn)
        {
            var cmdBytes = new byte[11];

            cmdBytes[0] = 0xFF;
            cmdBytes[1] = 0x7E;
            cmdBytes[2] = 0x05;
            cmdBytes[3] = 0x31;   // Command code
            cmdBytes[4] = Convert.ToByte(LEDInd + 8 + NumGreenFixLED + NumRedFixLED + NumDarkRedFixLED);   // LED address
            cmdBytes[5] = Convert.ToByte(LEDBrightness);
            cmdBytes[6] = 0x00;
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
            recStatus.totalRedLEDPower = 0;
            recStatus.totalGreenLEDPower = 0;
            recStatus.totalDarkredLEDPower = 0;
            recStatus.isValidPackage = false;
            int pkgLen = Convert.ToInt16((recBytes[2] + (recBytes[3] << 8)));//???

            //???????????????
            UInt16 sumData = 0;
            for (int i = 4; i < (4 + pkgLen ); i++)
            {
                sumData += (UInt16)(recBytes[i]);
            }

            UInt16 CHECKSUM = (UInt16)((recBytes[4 + pkgLen] + (recBytes[4 + pkgLen + 1] << 8)));

            UInt16 recCHK = (UInt16)((UInt16)0xFFFF - (CHECKSUM + 1));//????????????sumdata
            if (sumData == recCHK)
            {
                recStatus.isValidPackage = true;

                //?????????????????????????????????????????????
                // fixed LED????????????LED
                for (int i = 0; i < NumFixLED; i++)
                {
                    recStatus.fixLEDPower[i] = (double)(recBytes[4 + i * 2] + (recBytes[4 + i * 2 + 1]<<8)) * LEDPowerConvertFactor;
                    recStatus.fixLEDVoltage[i] = (double)(recBytes[4 + i * 2 + NumFixLED * 2 + NumDimLED * 2] + (recBytes[4 + i * 2 + NumFixLED * 2 + NumDimLED * 2 + 1] << 8)) * LEDVoltageConvertFactor;
                    recStatus.fixLEDCurrent[i] = (double)(recBytes[4 + i * 2 + NumFixLED * 2 * 2 + NumDimLED * 2 * 2] + (recBytes[4 + i * 2 + NumFixLED * 2*2 + NumDimLED * 2 * 2 + 1] << 8)) * LEDCurrentConvertFactor;
                }

                // Dimmable LED
                for (int i = 0; i < NumDimLED; i++)
                {
                    recStatus.dimLEDPower[i] = (double)(recBytes[4 + i * 2 + NumFixLED * 2]+(recBytes[4 + i * 2 + NumFixLED * 2 + 1] << 8)) * LEDPowerConvertFactor;
                    recStatus.dimLEDVoltage[i] = (double)(recBytes[4 + i * 2 + NumFixLED * 4+ NumDimLED*2 ] + (recBytes[4 + i * 2 + NumFixLED * 4 + NumDimLED * 2 + 1] << 8)) * LEDVoltageConvertFactor;
                    recStatus.dimLEDCurrent[i] = (double)(recBytes[4 + i * 2 + NumFixLED * 6 + NumDimLED * 4 ] + (recBytes[4 + i * 2 + NumFixLED * 6 + NumDimLED * 4 + 1] << 8)) * LEDCurrentConvertFactor;
                }

                recStatus.temperature[0] = (double)(recBytes[4 + NumFixLED * 3 * 2 + NumDimLED * 3 * 2 ] + (recBytes[4 + NumFixLED * 3 * 2 + NumDimLED * 3 * 2 + 1] << 8)) * TempConvertFactor;
                //recStatus.temperature[0] = (recStatus.temperature[0] - 0.76) / 0.0025 + 25;//??????????????????
                recStatus.temperature[1] = (double)(recBytes[4 + NumFixLED * 3 * 2 + NumDimLED * 3 * 2 + 2] + (recBytes[4 + NumFixLED * 3 * 2 + NumDimLED * 3 * 2 + 3] << 8)) * TempConvertFactor;
                recStatus.temperature[2] = (double)(recBytes[4 + NumFixLED * 3 * 2 + NumDimLED * 3 * 2 + 4] + (recBytes[4 + NumFixLED * 3 * 2 + NumDimLED * 3 * 2 + 5] << 8)) * TempConvertFactor;
                recStatus.temperature[3] = (double)(recBytes[4 + NumFixLED * 3 * 2 + NumDimLED * 3 * 2 + 6] + (recBytes[4 + NumFixLED * 3 * 2 + NumDimLED * 3 * 2 + 7] << 8)) * TempConvertFactor;
            }

            return recStatus;
        }

    }

}
