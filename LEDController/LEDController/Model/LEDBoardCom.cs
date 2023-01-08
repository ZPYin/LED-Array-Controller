using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace LEDController.Model
{
    public enum QueryType : uint
    {
        FixGreenLED,
        FixRedLED,
        FixDarkRedLED,
        DimGreenLED,
        DimRedLED,
        DimDarkRedLED,
        PlcAChillerWarn,
        PlcAMainSwitch,
        PlcASpareSwitch,
        PlcAPumpWarn,
        PlcBMainSwitch,
        PlcBSpareSwitch,
        PlcCMainSwitch,
        PlcCSpareSwitch
    }

    public struct LEDStatus
    {
        public double LEDPower;
        public double LEDVoltage;
        public double LEDCurrent;
        public bool isValidStatus;
    }

    public class AllLEDStatus
    {
        public double[] fixGreenLEDPower;
        public double[] fixGreenLEDVoltage;
        public double[] fixGreenLEDCurrent;
        public double[] fixRedLEDPower;
        public double[] fixRedLEDVoltage;
        public double[] fixRedLEDCurrent;
        public double[] fixDarkRedLEDPower;
        public double[] fixDarkRedLEDVoltage;
        public double[] fixDarkRedLEDCurrent;
        public double[] dimGreenLEDPower;
        public double[] dimGreenLEDVoltage;
        public double[] dimGreenLEDCurrent;
        public double[] dimRedLEDPower;
        public double[] dimRedLEDVoltage;
        public double[] dimRedLEDCurrent;
        public double[] dimDarkRedLEDPower;
        public double[] dimDarkRedLEDVoltage;
        public double[] dimDarkRedLEDCurrent;
        public double[] tempGreenLED;   // 0: left-up; 1: left-down; 2: right-down; 3: right-up
        public double[] tempRedLED;
        public double[] tempDarkRedLED;
        public bool isValidStatus;
        public DateTime updatedTime;

        public AllLEDStatus(int numFixRedLED, int numFixDarkRedLED, int numFixGreenLED, int numDimRedLED, int numDimDarkRedLED, int numDimGreenLED)
        {
            this.fixGreenLEDPower = new double[numFixGreenLED];
            this.fixGreenLEDCurrent = new double[numFixGreenLED];
            this.fixGreenLEDVoltage = new double[numFixGreenLED];
            this.fixRedLEDPower = new double[numFixRedLED];
            this.fixRedLEDCurrent = new double[numFixRedLED];
            this.fixRedLEDVoltage = new double[numFixRedLED];
            this.fixDarkRedLEDPower = new double[numFixDarkRedLED];
            this.fixDarkRedLEDCurrent = new double[numFixDarkRedLED];
            this.fixDarkRedLEDVoltage = new double[numFixDarkRedLED];
            this.dimGreenLEDPower = new double[numDimGreenLED];
            this.dimGreenLEDCurrent = new double[numDimGreenLED];
            this.dimGreenLEDVoltage = new double[numDimGreenLED];
            this.dimRedLEDPower = new double[numDimRedLED];
            this.dimRedLEDCurrent = new double[numDimRedLED];
            this.dimRedLEDVoltage = new double[numDimRedLED];
            this.dimDarkRedLEDPower = new double[numDimDarkRedLED];
            this.dimDarkRedLEDCurrent = new double[numDimDarkRedLED];
            this.dimDarkRedLEDVoltage = new double[numDimDarkRedLED];
            this.tempGreenLED = new double[4] { 0.0, 0.0, 0.0, 0.0 };
            this.tempRedLED = new double[4] { 0.0, 0.0, 0.0, 0.0 };
            this.tempDarkRedLED = new double[4] { 0.0, 0.0, 0.0, 0.0 };
            this.isValidStatus = false;
            this.updatedTime = DateTime.Now;
        }

        public AllLEDStatus()
        {
            this.isValidStatus = false;
            this.updatedTime = DateTime.Now;
        }

        public double CalcTotalGreenLEDPower()
        {
            double power = 0.0;
            for (int i = 0; i < this.fixGreenLEDPower.Length; i++)
            {
                power += this.fixGreenLEDPower[i];
            }

            for (int j = 0; j < this.dimGreenLEDPower.Length; j++)
            {
                power += this.dimGreenLEDPower[j];
            }

            return power;
        }

        public double CalcTotalRedLEDPower()
        {
            double power = 0.0;
            for (int i = 0; i < this.fixRedLEDPower.Length; i++)
            {
                power += this.fixRedLEDPower[i];
            }

            for (int j = 0; j < this.dimRedLEDPower.Length; j++)
            {
                power += this.dimRedLEDPower[j];
            }

            return power;
        }

        public double CalcTotalDarkRedLEDPower()
        {
            double power = 0.0;
            for (int i = 0; i < this.fixDarkRedLEDPower.Length; i++)
            {
                power += this.fixDarkRedLEDPower[i];
            }

            for (int j = 0; j < this.dimDarkRedLEDPower.Length; j++)
            {
                power += this.dimDarkRedLEDPower[j];
            }

            return power;
        }

        public double CalcTotalGreenLEDCurrent()
        {
            double power = 0.0;
            for (int i = 0; i < this.fixGreenLEDCurrent.Length; i++)
            {
                power += this.fixGreenLEDCurrent[i];
            }

            for (int j = 0; j < this.dimGreenLEDCurrent.Length; j++)
            {
                power += this.dimGreenLEDCurrent[j];
            }

            return power;
        }

        public double CalcTotalRedLEDCurrent()
        {
            double power = 0.0;
            for (int i = 0; i < this.fixRedLEDCurrent.Length; i++)
            {
                power += this.fixRedLEDCurrent[i];
            }

            for (int j = 0; j < this.dimRedLEDCurrent.Length; j++)
            {
                power += this.dimRedLEDCurrent[j];
            }

            return power;
        }

        public double CalcTotalDarkRedLEDCurrent()
        {
            double power = 0.0;
            for (int i = 0; i < this.fixDarkRedLEDCurrent.Length; i++)
            {
                power += this.fixDarkRedLEDCurrent[i];
            }

            for (int j = 0; j < this.dimDarkRedLEDCurrent.Length; j++)
            {
                power += this.dimDarkRedLEDCurrent[j];
            }

            return power;
        }

        public double CalcTotalGreenLEDVoltage()
        {
            double power = 0.0;
            for (int i = 0; i < this.fixGreenLEDVoltage.Length; i++)
            {
                power += this.fixGreenLEDVoltage[i];
            }

            for (int j = 0; j < this.dimGreenLEDVoltage.Length; j++)
            {
                power += this.dimGreenLEDVoltage[j];
            }

            return power;
        }

        public double CalcTotalRedLEDVoltage()
        {
            double power = 0.0;
            for (int i = 0; i < this.fixRedLEDVoltage.Length; i++)
            {
                power += this.fixRedLEDVoltage[i];
            }

            for (int j = 0; j < this.dimRedLEDVoltage.Length; j++)
            {
                power += this.dimRedLEDVoltage[j];
            }

            return power;
        }

        public double CalcTotalDarkRedLEDVoltage()
        {
            double power = 0.0;
            for (int i = 0; i < this.fixDarkRedLEDVoltage.Length; i++)
            {
                power += this.fixDarkRedLEDVoltage[i];
            }

            for (int j = 0; j < this.dimDarkRedLEDVoltage.Length; j++)
            {
                power += this.dimDarkRedLEDVoltage[j];
            }

            return power;
        }

        public double[] GetLEDPowerArray()
        {
            List<double> thisList = this.fixGreenLEDPower.ToList().Concat(this.dimGreenLEDPower.ToList()).Concat(this.fixRedLEDPower.ToList()).Concat(this.dimRedLEDPower.ToList()).Concat(this.fixDarkRedLEDPower.ToList()).Concat(this.dimDarkRedLEDPower.ToList()).ToList();
            return thisList.ToArray();
        }

        public double[] GetLEDCurrentArray()
        {
            List<double> thisList = this.fixGreenLEDCurrent.ToList().Concat(this.dimGreenLEDCurrent.ToList()).Concat(this.fixRedLEDCurrent.ToList()).Concat(this.dimRedLEDCurrent.ToList()).Concat(this.fixDarkRedLEDCurrent.ToList()).Concat(this.dimDarkRedLEDCurrent.ToList()).ToList();
            return thisList.ToArray();
        }

        public double[] GetLEDVoltageArray()
        {
            List<double> thisList = this.fixGreenLEDVoltage.ToList().Concat(this.dimGreenLEDVoltage.ToList()).Concat(this.fixRedLEDVoltage.ToList()).Concat(this.dimRedLEDVoltage.ToList()).Concat(this.fixDarkRedLEDVoltage.ToList()).Concat(this.dimDarkRedLEDVoltage.ToList()).ToList();
            return thisList.ToArray();
        }

        public double CalcLEDTotalPower()
        {
            double totalVal;
            if (this.isValidStatus)
            {
                totalVal = CalcTotalGreenLEDPower() + CalcTotalRedLEDPower() + CalcTotalDarkRedLEDPower();
            }
            else
            {
                totalVal = 0.0;
            }

            return totalVal;
        }

        public double CalcLEDTotalVoltage()
        {
            double totalVal;
            if (this.isValidStatus)
            {
                totalVal = CalcTotalGreenLEDVoltage() + CalcTotalRedLEDVoltage() + CalcTotalDarkRedLEDVoltage();
            }
            else
            {
                totalVal = 0.0;
            }

            return totalVal;
        }

        public double CalcLEDTotalCurrent()
        {
            double totalVal;
            if (this.isValidStatus)
            {
                totalVal = CalcTotalGreenLEDCurrent() + CalcTotalRedLEDCurrent() + CalcTotalDarkRedLEDCurrent();
            }
            else
            {
                totalVal = 0.0;
            }

            return totalVal;
        }
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
        public string protocol;
        public bool isSendASCII;
        public bool isRecASCII;
        public Boolean[] isFixGreenLEDOn = new Boolean[LEDConfig.NumFixGreenLED];
        public Boolean[] isFixRedLEDOn = new Boolean[LEDConfig.NumFixRedLED];
        public Boolean[] isFixDarkRedLEDOn = new Boolean[LEDConfig.NumFixDarkRedLED];
        public double[] dimGreenLEDPower = new double[LEDConfig.NumDimGreenLED];
        public double[] dimRedLEDPower = new double[LEDConfig.NumDimRedLED];
        public double[] dimDarkRedLEDPower = new double[LEDConfig.NumDimDarkRedLED];

        public LEDControllerCfg(string fileName)
        {
            Config cfgReader = new Config(fileName);

            foreach (var item in cfgReader.configData)
            {
                if (item.Key.Equals("SlaveIP"))
                {
                    slaveIP = item.Value;
                }
                else if (item.Key.Equals("SlavePort"))
                {
                    slavePort = item.Value;
                }
                else if (item.Key.Equals("Protocol"))
                {
                    protocol = item.Value;
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
                else if (item.Key.Equals("IsRecASCII"))
                {
                    isRecASCII = Convert.ToBoolean(item.Value);
                }
                else if (item.Key.Contains("FixGreenLED"))
                {
                    int LEDIndex = GetLEDIndex(item.Key, "FixGreenLED");

                    isFixGreenLEDOn[LEDIndex] = Convert.ToBoolean(item.Value);
                }
                else if ((item.Key.Contains("FixRedLED")) && (!item.Key.Contains("Dark")))
                {
                    int LEDIndex = GetLEDIndex(item.Key, "FixRedLED");

                    isFixRedLEDOn[LEDIndex] = Convert.ToBoolean(item.Value);
                }
                else if (item.Key.Contains("FixDarkRedLED"))
                {
                    int LEDIndex = GetLEDIndex(item.Key, "FixDarkRedLED");

                    isFixDarkRedLEDOn[LEDIndex] = Convert.ToBoolean(item.Value);
                }
                else if (item.Key.Contains("DimGreenLED"))
                {
                    int LEDIndex = GetLEDIndex(item.Key, "DimGreenLED");

                    dimGreenLEDPower[LEDIndex] = Convert.ToDouble(item.Value);
                }
                else if ((item.Key.Contains("DimRedLED")) && (!item.Key.Contains("Dark")))
                {
                    int LEDIndex = GetLEDIndex(item.Key, "DimRedLED");

                    dimRedLEDPower[LEDIndex] = Convert.ToDouble(item.Value);
                }
                else if (item.Key.Contains("DimDarkRedLED"))
                {
                    int LEDIndex = GetLEDIndex(item.Key, "DimDarkRedLED");

                    dimDarkRedLEDPower[LEDIndex] = Convert.ToDouble(item.Value);
                }
                else
                {
                    Exception myEx = new Exception();
                    throw myEx;
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

            for (int i = 0; i < isFixGreenLEDOn.Count(); i++)
            {
                LEDCfg.Add($"FixGreenLED{i + 1}", Convert.ToString(isFixGreenLEDOn[i]));
            }

            for (int i = 0; i < isFixRedLEDOn.Count(); i++)
            {
                LEDCfg.Add($"FixRedLED{i + 1}", Convert.ToString(isFixRedLEDOn[i]));
            }

            for (int i = 0; i < isFixDarkRedLEDOn.Count(); i++)
            {
                LEDCfg.Add($"FixDarkRedLED{i + 1}", Convert.ToString(isFixDarkRedLEDOn[i]));
            }

            for (int i = 0; i < dimGreenLEDPower.Count(); i++)
            {
                LEDCfg.Add($"DimGreenLED{i + 1}", Convert.ToString(dimGreenLEDPower[i]));
            }

            for (int i = 0; i < dimRedLEDPower.Count(); i++)
            {
                LEDCfg.Add($"DimRedLED{i + 1}", Convert.ToString(dimRedLEDPower[i]));
            }

            for (int i = 0; i < dimDarkRedLEDPower.Count(); i++)
            {
                LEDCfg.Add($"DimDarkRedLED{i + 1}", Convert.ToString(dimDarkRedLEDPower[i]));
            }

            LEDCfg.Add("SlaveIP", slaveIP);
            LEDCfg.Add("SlavePort", slavePort);
            LEDCfg.Add("COMPort", serialName);
            LEDCfg.Add("BaudRate", baudRate);
            LEDCfg.Add("CheckBit", checkBit);
            LEDCfg.Add("DataBit", dataBit);
            LEDCfg.Add("StopBit", stopBit);
            LEDCfg.Add("Protocol", protocol);
            LEDCfg.Add("IsSendDataASCII", isSendASCII.ToString());
            LEDCfg.Add("IsRecASCII", isRecASCII.ToString());

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
            int addrLED;
            if (addrPLC == LEDConfig.addrPLCGreenLED)
            {
                addrLED = LEDConfig.addrFixGreenLEDSwitch[LEDInd - 1];
            }
            else if (addrPLC == LEDConfig.addrPLCRedLED)
            {
                addrLED = LEDConfig.addrFixRedLEDSwitch[LEDInd - 1];
            }
            else if (addrPLC == LEDConfig.addrPLCDarkRedLED)
            {
                addrLED = LEDConfig.addrFixDarkRedLEDSwitch[LEDInd - 1];
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            SendCmd((byte)addrPLC, 5, (ushort)addrLED, new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffFixLED(int addrPLC, int LEDInd)
        {
            int addrLED;
            if (addrPLC == LEDConfig.addrPLCGreenLED)
            {
                addrLED = LEDConfig.addrFixGreenLEDSwitch[LEDInd - 1];
            }
            else if (addrPLC == LEDConfig.addrPLCRedLED)
            {
                addrLED = LEDConfig.addrFixRedLEDSwitch[LEDInd - 1];
            }
            else if (addrPLC == LEDConfig.addrPLCDarkRedLED)
            {
                addrLED = LEDConfig.addrFixDarkRedLEDSwitch[LEDInd - 1];
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            SendCmd((byte)addrPLC, 5, (ushort)addrLED, new byte[2] { 0x00, 0x00 });
        }

        public void SetDimLED(int addrPLC, int LEDInd, int LEDBrightness)
        {
            int addrLEDSwitch;
            int addrLEDPowerControl;
            if (addrPLC == LEDConfig.addrPLCGreenLED)
            {
                addrLEDSwitch = LEDConfig.addrDimGreenLEDSwitch[LEDInd - 1];
                addrLEDPowerControl = LEDConfig.addrDimGreenLEDPowerControl[LEDInd - 1];
            }
            else if (addrPLC == LEDConfig.addrPLCRedLED)
            {
                addrLEDSwitch = LEDConfig.addrDimRedLEDSwitch[LEDInd - 1];
                addrLEDPowerControl = LEDConfig.addrDimRedLEDPowerControl[LEDInd - 1];
            }
            else if (addrPLC == LEDConfig.addrPLCDarkRedLED)
            {
                addrLEDSwitch = LEDConfig.addrDimDarkRedLEDSwitch[LEDInd - 1];
                addrLEDPowerControl = LEDConfig.addrDimDarkRedLEDPowerControl[LEDInd - 1];
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }

            SendCmd((byte)addrPLC, 5, (ushort)addrLEDSwitch, new byte[2] { 0xFF, 0x00 });
            Thread.Sleep(100);
            byte[] dataSend = BitConverter.GetBytes(Convert.ToInt16(LEDBrightness));
            Array.Reverse(dataSend);
            SendCmd((byte)addrPLC, 6, (ushort)addrLEDPowerControl, dataSend);
        }

        public void TurnOffDimLED(int addrPLC, int LEDInd)
        {
            int addrLEDSwitch;
            if (addrPLC == LEDConfig.addrPLCGreenLED)
            {
                addrLEDSwitch = LEDConfig.addrDimGreenLEDSwitch[LEDInd - 1];
            }
            else if (addrPLC == LEDConfig.addrPLCRedLED)
            {
                addrLEDSwitch = LEDConfig.addrDimRedLEDSwitch[LEDInd - 1];
            }
            else if (addrPLC == LEDConfig.addrPLCDarkRedLED)
            {
                addrLEDSwitch = LEDConfig.addrDimDarkRedLEDSwitch[LEDInd - 1];
            }
            else
            {
                throw new ArgumentException();
            }

            SendCmd((byte)addrPLC, 5, (ushort)addrLEDSwitch, new byte[2] { 0x00, 0x00 });
        }

        public double[] ParseLEDPower(byte[] recData)
        {
            int nLED = recData.Length / 2;
            double[] LEDPowers = new double[nLED];

            int value;
            for (int i = 0; i < nLED; i++)
            {
                value = (recData[i * 2] << 8) | (recData[i * 2 + 1]);
                LEDPowers[i] = Convert.ToDouble(value);
            }

            return LEDPowers;
        }

        public double[] ParseLEDVoltage(byte[] recData)
        {
            int nLED = recData.Length / 2;
            double[] LEDVoltages = new double[nLED];

            int value;
            for (int i = 0; i < nLED; i++)
            {
                value = (recData[i * 2] << 8) | (recData[i * 2 + 1]);
                LEDVoltages[i] = Convert.ToDouble(value) / 100.0;
            }

            return LEDVoltages;
        }

        public double[] ParseLEDCurrent(byte[] recData)
        {
            int nLED = recData.Length / 2;
            double[] LEDCurrents = new double[nLED];

            int value;
            for (int i = 0; i < nLED; i++)
            {
                value = (recData[i * 2] << 8) | (recData[i * 2 + 1]);
                LEDCurrents[i] = Convert.ToDouble(value) / 1000.0;
            }

            return LEDCurrents;
        }

        public double[] ParseTemperature(byte[] recData)
        {
            int nTemp = recData.Length / 2;
            double[] temperatures = new double[nTemp];

            int value;
            for (int i = 0; i < nTemp; i++)
            {
                value = (recData[i * 2] << 8) | (recData[i * 2 + 1]);
                temperatures[i] = Convert.ToDouble(value) / 10;
            }

            return temperatures;
        }

        public AllLEDStatus QueryAllLEDStatus()
        {
            AllLEDStatus status = new AllLEDStatus(LEDConfig.NumFixRedLED, LEDConfig.NumFixDarkRedLED, LEDConfig.NumFixGreenLED, LEDConfig.NumDimRedLED, LEDConfig.NumDimDarkRedLED, LEDConfig.NumDimGreenLED);

            if (!this.isAlive)
            {
                return status;
            }

            byte[] recData;
            double[] values;
            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCRedLED, 3, (ushort)LEDConfig.addrFixRedLEDPower[0], BitConverter.GetBytes((ushort)LEDConfig.NumFixRedLED));
                values = ParseLEDPower(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.fixRedLEDPower[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCRedLED, 3, (ushort)LEDConfig.addrFixRedLEDVoltage[0], BitConverter.GetBytes((ushort)LEDConfig.NumFixRedLED));
                values = ParseLEDVoltage(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.fixRedLEDVoltage[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCRedLED, 3, (ushort)LEDConfig.addrFixRedLEDCurrent[0], BitConverter.GetBytes((ushort)LEDConfig.NumFixRedLED));
                values = ParseLEDCurrent(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.fixRedLEDCurrent[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCDarkRedLED, 3, (ushort)LEDConfig.addrFixDarkRedLEDPower[0], BitConverter.GetBytes((ushort)LEDConfig.NumFixDarkRedLED));
                values = ParseLEDPower(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.fixDarkRedLEDPower[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCDarkRedLED, 3, (ushort)LEDConfig.addrFixDarkRedLEDVoltage[0], BitConverter.GetBytes((ushort)LEDConfig.NumFixDarkRedLED));
                values = ParseLEDVoltage(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.fixDarkRedLEDVoltage[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCDarkRedLED, 3, (ushort)LEDConfig.addrFixDarkRedLEDCurrent[0], BitConverter.GetBytes((ushort)LEDConfig.NumFixDarkRedLED));
                values = ParseLEDCurrent(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.fixDarkRedLEDCurrent[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCGreenLED, 3, (ushort)LEDConfig.addrFixGreenLEDPower[0], BitConverter.GetBytes((ushort)LEDConfig.NumFixGreenLED));
                values = ParseLEDPower(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.fixGreenLEDPower[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCGreenLED, 3, (ushort)LEDConfig.addrFixGreenLEDVoltage[0], BitConverter.GetBytes((ushort)LEDConfig.NumFixGreenLED));
                values = ParseLEDVoltage(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.fixGreenLEDVoltage[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCGreenLED, 3, (ushort)LEDConfig.addrFixGreenLEDCurrent[0], BitConverter.GetBytes((ushort)LEDConfig.NumFixGreenLED));
                values = ParseLEDCurrent(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.fixGreenLEDCurrent[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCRedLED, 3, (ushort)LEDConfig.addrDimRedLEDPower[0], BitConverter.GetBytes((ushort)LEDConfig.NumDimRedLED));
                values = ParseLEDPower(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.dimRedLEDPower[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCRedLED, 3, (ushort)LEDConfig.addrDimRedLEDVoltage[0], BitConverter.GetBytes((ushort)LEDConfig.NumDimRedLED));
                values = ParseLEDVoltage(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.dimRedLEDVoltage[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCRedLED, 3, (ushort)LEDConfig.addrDimRedLEDCurrent[0], BitConverter.GetBytes((ushort)LEDConfig.NumDimRedLED));
                values = ParseLEDCurrent(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.dimRedLEDCurrent[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCDarkRedLED, 3, (ushort)LEDConfig.addrDimDarkRedLEDPower[0], BitConverter.GetBytes((ushort)LEDConfig.NumDimDarkRedLED));
                values = ParseLEDPower(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.dimDarkRedLEDPower[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCDarkRedLED, 3, (ushort)LEDConfig.addrDimDarkRedLEDVoltage[0], BitConverter.GetBytes((ushort)LEDConfig.NumDimDarkRedLED));
                values = ParseLEDVoltage(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.dimDarkRedLEDVoltage[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCDarkRedLED, 3, (ushort)LEDConfig.addrDimDarkRedLEDCurrent[0], BitConverter.GetBytes((ushort)LEDConfig.NumDimDarkRedLED));
                values = ParseLEDCurrent(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.dimDarkRedLEDCurrent[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCGreenLED, 3, (ushort)LEDConfig.addrDimGreenLEDPower[0], BitConverter.GetBytes((ushort)LEDConfig.NumDimGreenLED));
                values = ParseLEDPower(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.dimGreenLEDPower[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCGreenLED, 3, (ushort)LEDConfig.addrDimGreenLEDVoltage[0], BitConverter.GetBytes((ushort)LEDConfig.NumDimGreenLED));
                values = ParseLEDVoltage(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.dimGreenLEDVoltage[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCGreenLED, 3, (ushort)LEDConfig.addrDimGreenLEDCurrent[0], BitConverter.GetBytes((ushort)LEDConfig.NumDimGreenLED));
                values = ParseLEDCurrent(recData);
                for (int i = 0; i < values.Length; i++)
                {
                    status.dimGreenLEDCurrent[i] = values[i];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            Thread.Sleep(50);

            // receive temperature sensor data
            try
            {
                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCGreenLED, 3, (ushort)LEDConfig.addrTempSensor[0], BitConverter.GetBytes((ushort)4));
                status.tempGreenLED = ParseTemperature(recData);
                Thread.Sleep(100);

                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCRedLED, 3, (ushort)LEDConfig.addrTempSensor[0], BitConverter.GetBytes((ushort)4));
                status.tempRedLED = ParseTemperature(recData);
                Thread.Sleep(100);

                recData = this.device.WriteReceive((byte)LEDConfig.addrPLCDarkRedLED, 3, (ushort)LEDConfig.addrTempSensor[0], BitConverter.GetBytes((ushort)4));
                status.tempDarkRedLED = ParseTemperature(recData);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            status.isValidStatus = true;
            status.updatedTime = DateTime.Now;

            return status;
        }

        public LEDStatus QueryLEDStatus(int idx, QueryType qType)
        {

            LEDStatus recStatus = new LEDStatus();

            if (!this.isAlive)
            {
                recStatus.LEDPower = 0.0;
                recStatus.LEDCurrent = 0.0;
                recStatus.LEDVoltage = 0.0;
                recStatus.isValidStatus = false;
                return recStatus;
            }

            int addrLEDPower = 0;
            int addrLEDCurrent = 0;
            int addrLEDVoltage = 0;
            int addrPLC = 0;
            switch (qType)
            {
                case QueryType.FixGreenLED:
                    addrLEDPower = LEDConfig.addrFixGreenLEDPower[idx - 1];
                    addrLEDCurrent = LEDConfig.addrFixGreenLEDCurrent[idx - 1];
                    addrLEDVoltage = LEDConfig.addrFixGreenLEDVoltage[idx - 1];
                    addrPLC = LEDConfig.addrPLCGreenLED;
                    break;

                case QueryType.FixRedLED:
                    addrLEDPower = LEDConfig.addrFixRedLEDPower[idx - 1];
                    addrLEDCurrent = LEDConfig.addrFixRedLEDCurrent[idx - 1];
                    addrLEDVoltage = LEDConfig.addrFixRedLEDVoltage[idx - 1];
                    addrPLC = LEDConfig.addrPLCRedLED;
                    break;

                case QueryType.FixDarkRedLED:
                    addrLEDPower = LEDConfig.addrFixDarkRedLEDPower[idx - 1];
                    addrLEDCurrent = LEDConfig.addrFixDarkRedLEDCurrent[idx - 1];
                    addrLEDVoltage = LEDConfig.addrFixDarkRedLEDVoltage[idx - 1];
                    addrPLC = LEDConfig.addrPLCDarkRedLED;
                    break;

                case QueryType.DimDarkRedLED:
                    addrLEDPower = LEDConfig.addrDimDarkRedLEDPower[idx - 1];
                    addrLEDCurrent = LEDConfig.addrDimDarkRedLEDCurrent[idx - 1];
                    addrLEDVoltage = LEDConfig.addrDimDarkRedLEDVoltage[idx - 1];
                    addrPLC = LEDConfig.addrPLCDarkRedLED;
                    break;

                case QueryType.DimRedLED:
                    addrLEDPower = LEDConfig.addrDimRedLEDPower[idx - 1];
                    addrLEDCurrent = LEDConfig.addrDimRedLEDCurrent[idx - 1];
                    addrLEDVoltage = LEDConfig.addrDimRedLEDVoltage[idx - 1];
                    addrPLC = LEDConfig.addrPLCRedLED;
                    break;

                case QueryType.DimGreenLED:
                    addrLEDPower = LEDConfig.addrDimGreenLEDPower[idx - 1];
                    addrLEDCurrent = LEDConfig.addrDimGreenLEDCurrent[idx - 1];
                    addrLEDVoltage = LEDConfig.addrDimGreenLEDVoltage[idx - 1];
                    addrPLC = LEDConfig.addrPLCGreenLED;
                    break;
            }

            byte[] recData;
            double[] values;
            try
            {
                recData = this.device.WriteReceive((byte)addrPLC, 3, (ushort)addrLEDPower, new byte[2] { 0x00, 0x01 });
                values = ParseLEDPower(recData);
                recStatus.LEDPower = values[0];

                Thread.Sleep(100);
                recData = this.device.WriteReceive((byte)addrPLC, 3, (ushort)addrLEDCurrent, new byte[2] { 0x00, 0x01 });
                values = ParseLEDCurrent(recData);
                recStatus.LEDCurrent = values[0];

                Thread.Sleep(100);
                recData = this.device.WriteReceive((byte)addrPLC, 3, (ushort)addrLEDVoltage, new byte[2] { 0x00, 0x01 });
                values = ParseLEDVoltage(recData);
                recStatus.LEDVoltage = values[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }

            recStatus.isValidStatus = true;

            return recStatus;
        }

        public bool[] ParseChillerStatus(byte[] recData, int count)
        {
            bool[] chillerStatus = new bool[count];

            for (int i = 0; i <= count / 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i * 8 + j) < count)
                    {
                        chillerStatus[i * 8 + j] = Convert.ToBoolean(recData[i] & (1 << (7 - j)));
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return chillerStatus;
        }

        public bool IsChillerWarning(int addrPLC, int chillerIndex)
        {
            bool[] chillerWarning;

            try
            {
                byte[] recData = this.device.WriteReceive((byte)addrPLC, 1, (ushort)LEDConfig.addrPlcAChillerWarn[chillerIndex], new byte[2] { 0x00, 0x01 });
                chillerWarning = ParseChillerStatus(recData, 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return chillerWarning[0];
        }

        public bool[] ParsePumpStatus(byte[] recData, int count)
        {
            bool[] pumpStatus = new bool[count];

            for (int i = 0; i <= count / 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i * 8 + j) < count)
                    {
                        pumpStatus[i * 8 + j] = Convert.ToBoolean(recData[i] & (1 << (7 - j)));
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return pumpStatus;
        }

        public bool IsPumpWarning(int addrPLC, int pumpIndex)
        {
            bool[] pumpWarning;

            try
            {
                byte[] recData = this.device.WriteReceive((byte)addrPLC, 1, (ushort)LEDConfig.addrPlcAPumpWarn[pumpIndex], new byte[2] { 0x00, 0x01 });
                pumpWarning = ParsePumpStatus(recData, 1);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return pumpWarning[0];
        }

        public void TurnOnSkyLight(int addrPLC, int skyLightIndex)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[skyLightIndex], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffSkyLight(int addrPLC, int skyLightIndex)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[skyLightIndex], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnLight(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[5], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffLight(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[5], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnChiller(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[1], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffChiller(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[1], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnLightMainSwitch(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[0], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffLightMainSwitch(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[0], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnRTPower(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[4], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffRTPower(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[4], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnAirConditionerPower(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[3], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffAirConditionerPower(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[3], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnCamPower(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[6], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffCamPower(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[6], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnPCPower(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[2], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffPCPower(int addrPLC)
        {
            SendCmd((byte)addrPLC, 5, (ushort)LEDConfig.addrPlcASpareSwitch[2], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnGreenLEDMainSwitch()
        {
            SendCmd((byte)LEDConfig.addrPLCGreenLED, 5, (ushort)LEDConfig.addrGreenMainSwitch[0], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffGreenLEDMainSwitch()
        {
            SendCmd((byte)LEDConfig.addrPLCGreenLED, 5, (ushort)LEDConfig.addrGreenMainSwitch[0], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnRedLEDMainSwitch()
        {
            SendCmd((byte)LEDConfig.addrPLCRedLED, 5, (ushort)LEDConfig.addrRedMainSwitch[0], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffRedLEDMainSwitch()
        {
            SendCmd((byte)LEDConfig.addrPLCRedLED, 5, (ushort)LEDConfig.addrRedMainSwitch[0], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnDarkRedLEDMainSwitch()
        {
            SendCmd((byte)LEDConfig.addrPLCDarkRedLED, 5, (ushort)LEDConfig.addrDarkRedMainSwitch[0], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffDarkRedLEDMainSwitch()
        {
            SendCmd((byte)LEDConfig.addrPLCDarkRedLED, 5, (ushort)LEDConfig.addrDarkRedMainSwitch[0], new byte[2] { 0x00, 0x00 });
        }

    }

}
