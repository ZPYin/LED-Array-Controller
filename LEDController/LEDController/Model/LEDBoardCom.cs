using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.IO.Ports;
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
        public double totalGreenLEDPower;
        public double totalRedLEDPower;
        public double totalDarkredLEDPower;
        public Boolean isValidPackage;
    }

    public class LEDControllerCfg : LEDBoardCom
    {
        public string slaveIP;
        public string slavePort;
        public string serialName;
        public string dataBit;
        public string stopBit;
        public string checkBit;
        public string baudRate;
        public string protocal;
        public bool isSendASCII;
        public bool isRecASCII;
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
                    else if (item.Key.Equals("Protocal"))
                    {
                        protocal = item.Value;
                    }
                    else if (item.Key.Equals("COMPort"))
                    {
                        serialName = item.Value;
                    }
                    else if (item.Key.Equals("BaudRate"))
                    {
                        baudRate = item.Value;
                    }
                    else if (item.Key.Equals("CheckBit"))
                    {
                        checkBit = item.Value;
                    }
                    else if (item.Key.Equals("DataBit"))
                    {
                        dataBit = item.Value;
                    }
                    else if (item.Key.Equals("StopBit"))
                    {
                        stopBit = item.Value;
                    }
                    else if (item.Key.Equals("IsSendDataASCII"))
                    {
                        isSendASCII = Convert.ToBoolean(item.Value);
                    }
                    else if (item.Key.Equals("isRecASCII"))
                    {
                        isRecASCII = Convert.ToBoolean(item.Value);
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
        public Modbus device;
        public Boolean isAlive = false;
        public Boolean isTCP = false;
        public Boolean isSerialPort = false;

        private const double LEDVoltageConvertFactor = 1;
        private const double LEDCurrentConvertFactor = 1;
        private const double LEDPowerConvertFactor = 1;
        public const int NumGreenFixLED = 26;
        public const int NumRedFixLED = 26;
        public const int NumDarkRedFixLED = 26;
        public const int NumGreenDimLED = 4;
        public const int NumRedDimLED = 4;
        public const int NumDarkRedDimLED = 4;

        public LEDBoardCom(string SlaveIP, string SlavePort)
        {
            device = new ModbusTCP(SlaveIP, Convert.ToInt32(SlavePort));
            this.isTCP = true;
            this.isSerialPort = false;
        }

        public LEDBoardCom(string serialName, string baudRate, string dataBit, string checkBit, string stopBit)
        {
            device = new ModbusRTU(serialName, baudRate, dataBit, checkBit, stopBit);
            this.isTCP = false;
            this.isSerialPort = true;
        }

        public LEDBoardCom()
        {}

        public void Connect(int timeout = 500)
        {
            this.isAlive = this.device.Connect(timeout);
        }

        public void Disconnect()
        {
            this.isAlive = !this.device.Disconnect();
        }

        public void SendCmd(string sendMsg, Boolean isSendHEX = false)
        {
            this.device.WriteMsg(sendMsg, isSendHEX);
        }

        public void SendCmd(byte addrPLC, byte function, ushort register, byte[] data)
        {
            if (this.isAlive)
            {
                this.device.Write(addrPLC, function, register, data);
            }
        }

        public void TurnOnFixLED(int addrPLC, int LEDInd)
        {
            var cmdBytes = MakeCmd(LEDInd, true);
            SendCmd((byte)addrPLC, 5, (byte)LEDInd, cmdBytes);
        }

        public void TurnOffFixLED(int addrPLC, int LEDInd)
        {
            var cmdBytes = MakeCmd(LEDInd, false);
            SendCmd((byte)addrPLC, 5, (byte)LEDInd, cmdBytes);
        }

        public void SetDimLED(int addrPLC, int LEDInd, int LEDBrightness)
        {
            var cmdBytes = MakeCmd(LEDInd, LEDBrightness, true);
            int idxReg = 01;
            SendCmd((byte)addrPLC, 6, (byte)idxReg, cmdBytes);
        }

        public void TurnOffDimLED(int addrPLC, int LEDInd)
        {
            var cmdBytes = MakeCmd(LEDInd, 0, false);
            SendCmd((byte)addrPLC, 5, (byte)LEDInd, cmdBytes);
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
            recStatus.totalRedLEDPower = 0;
            recStatus.totalGreenLEDPower = 0;
            recStatus.totalDarkredLEDPower = 0;
            recStatus.isValidPackage = false;
            int pkgLen = Convert.ToInt16((recBytes[2] + (recBytes[3] << 8)));//改

            //校验也改了
            UInt16 sumData = 0;
            for (int i = 4; i < (4 + pkgLen ); i++)
            {
                sumData += (UInt16)(recBytes[i]);
            }

            UInt16 CHECKSUM = (UInt16)((recBytes[4 + pkgLen] + (recBytes[4 + pkgLen + 1] << 8)));

            UInt16 recCHK = (UInt16)((UInt16)0xFFFF - (CHECKSUM + 1));//单片机的sumdata
            if (sumData == recCHK)
            {
                recStatus.isValidPackage = true;

                //修改协议，一个字节改为两个字节
                // fixed LED固定亮度LED
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
            }

            return recStatus;
        }

    }

}
