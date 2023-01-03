using System;
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
        public Boolean[] isFixGreenLEDOn = new Boolean[LEDBoardCom.NumFixGreenLED];
        public Boolean[] isFixRedLEDOn = new Boolean[LEDBoardCom.NumFixRedLED];
        public Boolean[] isFixDarkRedLEDOn = new Boolean[LEDBoardCom.NumFixDarkRedLED];
        public double[] dimGreenLEDPower = new double[LEDBoardCom.NumDimGreenLED];
        public double[] dimRedLEDPower = new double[LEDBoardCom.NumDimRedLED];
        public double[] dimDarkRedLEDPower = new double[LEDBoardCom.NumDimDarkRedLED];

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

        private const double LEDVoltageConvertFactor = 1;
        private const double LEDCurrentConvertFactor = 1;
        private const double LEDPowerConvertFactor = 1;
        public const int NumFixGreenLED = 26;
        public const int NumFixRedLED = 26;
        public const int NumFixDarkRedLED = 26;
        public const int NumDimGreenLED = 4;
        public const int NumDimRedLED = 4;
        public const int NumDimDarkRedLED = 4;

        // Define Modbus address according to Protocol
        public int[] addrFixRedLEDPower = new int[NumFixRedLED] {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
            0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
            0x17, 0x18, 0x19, 0x1A };
        public int[] addrFixRedLEDVoltage = new int[NumFixRedLED] {
            0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33,
            0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E,
            0x3F, 0x40, 0x41, 0x42 };
        public int[] addrFixRedLEDCurrent = new int[NumFixRedLED] {
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B,
            0x5C, 0x5D, 0x5E, 0x5F, 0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66,
            0x67, 0x68, 0x69, 0x6A };
        public int[] addrFixRedLEDSwitch = new int[NumFixRedLED] {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
            0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
            0x17, 0x18, 0x19, 0x1A };
        public int[] addrDimRedLEDPower = new int[NumDimRedLED] {
            0x1B, 0x1C, 0x1D, 0x1E };
        public int[] addrDimRedLEDVoltage = new int[NumDimRedLED] {
            0x43, 0x44, 0x45, 0x46 };
        public int[] addrDimRedLEDCurrent = new int[NumDimRedLED] {
            0x6B, 0x6C, 0x6D, 0x6E };
        public int[] addrDimRedLEDSwitch = new int[NumDimRedLED] {
            0x1B, 0x1C, 0x1D, 0x1E };
        public int[] addrDimRedLEDPowerControl = new int[NumDimRedLED] {
            0x78, 0x79, 0x7A, 0x7B };
        public int[] addrPlcAChillerWarn = new int[2] { 0x3C, 0x3D };
        public int[] addrPlcAPumpWarn = new int[3] { 0x3E, 0x3F, 0x40 };
        public int[] addrPlcAMainSwitch = new int[1] { 0x1F };
        public int[] addrPlcASpareSwitch = new int[9] {
            0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18};
        public int[] addrFixDarkRedLEDPower = new int[NumFixDarkRedLED] {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
            0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
            0x17, 0x18, 0x19, 0x1A };
        public int[] addrFixDarkRedLEDVoltage = new int[NumFixDarkRedLED] {
            0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33,
            0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E,
            0x3F, 0x40, 0x41, 0x42 };
        public int[] addrFixDarkRedLEDCurrent = new int[NumFixDarkRedLED] {
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B,
            0x5C, 0x5D, 0x5E, 0x5F, 0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66,
            0x67, 0x68, 0x69, 0x6A };
        public int[] addrFixDarkRedLEDSwitch = new int[NumFixDarkRedLED] {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
            0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
            0x17, 0x18, 0x19, 0x1A };
        public int[] addrDimDarkRedLEDPower = new int[NumDimDarkRedLED] {
            0x1B, 0x1C, 0x1D, 0x1E };
        public int[] addrDimDarkRedLEDVoltage = new int[NumDimDarkRedLED] {
            0x43, 0x44, 0x45, 0x46 };
        public int[] addrDimDarkRedLEDCurrent = new int[NumDimDarkRedLED] {
            0x6B, 0x6C, 0x6D, 0x6E };
        public int[] addrDimDarkRedLEDSwitch = new int[NumDimDarkRedLED] {
            0x1B, 0x1C, 0x1D, 0x1E };
        public int[] addrDimDarkRedLEDPowerControl = new int[NumDimDarkRedLED] {
            0x78, 0x79, 0x7A, 0x7B };
        public int[] addrPlcBMainSwitch = new int[1] { 0x1F };
        public int[] addrPlcBSpareSwitch = new int[1] { 0x10 };
        public int[] addrFixGreenLEDPower = new int[NumFixGreenLED] {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
            0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
            0x17, 0x18, 0x19, 0x1A };
        public int[] addrFixGreenLEDVoltage = new int[NumFixGreenLED] {
            0x29, 0x2A, 0x2B, 0x2C, 0x2D, 0x2E, 0x2F, 0x30, 0x31, 0x32, 0x33,
            0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E,
            0x3F, 0x40, 0x41, 0x42 };
        public int[] addrFixGreenLEDCurrent = new int[NumFixGreenLED] {
            0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5A, 0x5B,
            0x5C, 0x5D, 0x5E, 0x5F, 0x60, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66,
            0x67, 0x68, 0x69, 0x6A };
        public int[] addrFixGreenLEDSwitch = new int[NumFixGreenLED] {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
            0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
            0x17, 0x18, 0x19, 0x1A };
        public int[] addrDimGreenLEDPower = new int[NumDimGreenLED] {
            0x1B, 0x1C, 0x1D, 0x1E };
        public int[] addrDimGreenLEDVoltage = new int[NumDimGreenLED] {
            0x43, 0x44, 0x45, 0x46 };
        public int[] addrDimGreenLEDCurrent = new int[NumDimGreenLED] {
            0x6B, 0x6C, 0x6D, 0x6E };
        public int[] addrDimGreenLEDSwitch = new int[NumDimGreenLED] {
            0x1B, 0x1C, 0x1D, 0x1E };
        public int[] addrDimGreenLEDPowerControl = new int[NumDimGreenLED] {
            0x78, 0x79, 0x7A, 0x7B };
        public int[] addrPlcCMainSwitch = new int[1] { 0x1F };
        public int[] addrPlcCSpareSwitch = new int[1] { 0x10 };

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
            int addrLED = 0;
            switch (addrPLC)
            {
                case 1:
                    addrLED = this.addrFixRedLEDSwitch[LEDInd - 1];
                    break;

                case 2:
                    addrLED = this.addrFixDarkRedLEDSwitch[LEDInd - 1];
                    break;

                case 3:
                    addrLED = this.addrFixGreenLEDSwitch[LEDInd - 1];
                    break;
            }

            SendCmd((byte)addrPLC, 5, (ushort)addrLED, new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffFixLED(int addrPLC, int LEDInd)
        {
            int addrLED = 0;
            switch (addrPLC)
            {
                case 1:
                    addrLED = this.addrFixRedLEDSwitch[LEDInd - 1];
                    break;

                case 2:
                    addrLED = this.addrFixDarkRedLEDSwitch[LEDInd - 1];
                    break;

                case 3:
                    addrLED = this.addrFixGreenLEDSwitch[LEDInd - 1];
                    break;
            }

            SendCmd((byte)addrPLC, 5, (ushort)addrLED, new byte[2] { 0x00, 0x00 });
        }

        public void SetDimLED(int addrPLC, int LEDInd, int LEDBrightness)
        {
            int addrLEDSwitch = 0;
            int addrLEDPowerControl = 0;
            switch (addrPLC)
            {
                case 1:
                    addrLEDSwitch = this.addrDimRedLEDSwitch[LEDInd - 1];
                    addrLEDPowerControl = this.addrDimRedLEDPowerControl[LEDInd - 1];
                    break;

                case 2:
                    addrLEDSwitch = this.addrDimDarkRedLEDSwitch[LEDInd - 1];
                    addrLEDPowerControl = this.addrDimDarkRedLEDPowerControl[LEDInd - 1];
                    break;

                case 3:
                    addrLEDSwitch = this.addrDimGreenLEDSwitch[LEDInd - 1];
                    addrLEDPowerControl = this.addrDimGreenLEDPowerControl[LEDInd - 1];
                    break;
            }

            SendCmd((byte)addrPLC, 5, (ushort)addrLEDSwitch, new byte[2] { 0xFF, 0x00 });
            byte[] dataSend = BitConverter.GetBytes(Convert.ToInt16(LEDBrightness));
            Array.Reverse(dataSend);
            SendCmd((byte)addrPLC, 6, (ushort)addrLEDPowerControl, dataSend);
        }

        public void TurnOffDimLED(int addrPLC, int LEDInd)
        {
            int addrLEDSwitch = 0;
            switch (addrPLC)
            {
                case 1:
                    addrLEDSwitch = this.addrDimRedLEDSwitch[LEDInd - 1];
                    break;

                case 2:
                    addrLEDSwitch = this.addrDimDarkRedLEDSwitch[LEDInd - 1];
                    break;

                case 3:
                    addrLEDSwitch = this.addrDimGreenLEDSwitch[LEDInd - 1];
                    break;
            }

            SendCmd((byte)addrPLC, 5, (ushort)addrLEDSwitch, new byte[2] { 0x00, 0x00 });
        }

        public double[] ParseLEDPower(byte[] recData)
        {
            int nLED = recData.Length / 2;
            double[] LEDPowers = new double[nLED];

            for (int i = 0; i < nLED; i++)
            {
                byte[] thisLEDData = new byte[2];
                Array.Copy(recData, i * 2, thisLEDData, 0, 2);
                LEDPowers[i] = Convert.ToDouble(thisLEDData);
            }

            return LEDPowers;
        }

        public double[] ParseLEDVoltage(byte[] recData)
        {
            int nLED = recData.Length / 2;
            double[] LEDVoltages = new double[nLED];

            for (int i = 0; i < nLED; i++)
            {
                byte[] thisLEDData = new byte[2];
                Array.Copy(recData, i * 2, thisLEDData, 0, 2);
                LEDVoltages[i] = Convert.ToDouble(thisLEDData);
            }

            return LEDVoltages;
        }

        public AllLEDStatus QueryAllLEDStatus()
        {
            AllLEDStatus status = new AllLEDStatus(NumFixRedLED, NumFixDarkRedLED, NumFixGreenLED, NumDimRedLED, NumDimDarkRedLED, NumDimGreenLED);

            if (!this.isAlive)
            {
                return status;
            }

            byte[] recData = this.device.WriteReceive((byte)1, 3, (ushort)addrFixRedLEDPower[0], BitConverter.GetBytes((ushort)NumFixRedLED));
            double[] values = ParseLEDPower(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.fixRedLEDPower[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)1, 3, (ushort)addrFixRedLEDVoltage[0], BitConverter.GetBytes((ushort)NumFixRedLED));
            values = ParseLEDVoltage(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.fixRedLEDVoltage[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)1, 3, (ushort)addrFixRedLEDCurrent[0], BitConverter.GetBytes((ushort)NumFixRedLED));
            values = ParseLEDCurrent(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.fixRedLEDCurrent[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)2, 3, (ushort)addrFixDarkRedLEDPower[0], BitConverter.GetBytes((ushort)NumFixDarkRedLED));
            values = ParseLEDPower(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.fixDarkRedLEDPower[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)2, 3, (ushort)addrFixDarkRedLEDVoltage[0], BitConverter.GetBytes((ushort)NumFixDarkRedLED));
            values = ParseLEDVoltage(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.fixDarkRedLEDVoltage[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)2, 3, (ushort)addrFixDarkRedLEDCurrent[0], BitConverter.GetBytes((ushort)NumFixDarkRedLED));
            values = ParseLEDCurrent(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.fixDarkRedLEDCurrent[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)3, 3, (ushort)addrFixGreenLEDPower[0], BitConverter.GetBytes((ushort)NumFixGreenLED));
            values = ParseLEDPower(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.fixGreenLEDPower[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)3, 3, (ushort)addrFixGreenLEDVoltage[0], BitConverter.GetBytes((ushort)NumFixGreenLED));
            values = ParseLEDVoltage(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.fixGreenLEDVoltage[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)3, 3, (ushort)addrFixGreenLEDCurrent[0], BitConverter.GetBytes((ushort)NumFixGreenLED));
            values = ParseLEDCurrent(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.fixGreenLEDCurrent[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)1, 3, (ushort)addrDimRedLEDPower[0], BitConverter.GetBytes((ushort)NumDimRedLED));
            values = ParseLEDPower(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.dimRedLEDPower[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)1, 3, (ushort)addrDimRedLEDVoltage[0], BitConverter.GetBytes((ushort)NumDimRedLED));
            values = ParseLEDVoltage(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.dimRedLEDVoltage[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)1, 3, (ushort)addrDimRedLEDCurrent[0], BitConverter.GetBytes((ushort)NumDimRedLED));
            values = ParseLEDCurrent(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.dimRedLEDCurrent[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)2, 3, (ushort)addrDimDarkRedLEDPower[0], BitConverter.GetBytes((ushort)NumDimDarkRedLED));
            values = ParseLEDPower(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.dimDarkRedLEDPower[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)2, 3, (ushort)addrDimDarkRedLEDVoltage[0], BitConverter.GetBytes((ushort)NumDimDarkRedLED));
            values = ParseLEDVoltage(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.dimDarkRedLEDVoltage[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)2, 3, (ushort)addrDimDarkRedLEDCurrent[0], BitConverter.GetBytes((ushort)NumDimDarkRedLED));
            values = ParseLEDCurrent(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.dimDarkRedLEDCurrent[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)3, 3, (ushort)addrDimGreenLEDPower[0], BitConverter.GetBytes((ushort)NumDimGreenLED));
            values = ParseLEDPower(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.dimGreenLEDPower[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)3, 3, (ushort)addrDimGreenLEDVoltage[0], BitConverter.GetBytes((ushort)NumDimGreenLED));
            values = ParseLEDVoltage(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.dimGreenLEDVoltage[i] = values[i];
            }

            recData = this.device.WriteReceive((byte)3, 3, (ushort)addrDimGreenLEDCurrent[0], BitConverter.GetBytes((ushort)NumDimGreenLED));
            values = ParseLEDCurrent(recData);
            for (int i = 0; i < values.Length; i++)
            {
                status.dimGreenLEDCurrent[i] = values[i];
            }

            status.isValidStatus = true;
            status.updatedTime = DateTime.Now;

            return status;
        }

        public double[] ParseLEDCurrent(byte[] recData)
        {
            int nLED = recData.Length / 2;
            double[] LEDCurrents = new double[nLED];

            for (int i = 0; i < nLED; i++)
            {
                byte[] thisLEDData = new byte[2];
                Array.Copy(recData, i * 2, thisLEDData, 0, 2);
                LEDCurrents[i] = Convert.ToDouble(thisLEDData);
            }

            return LEDCurrents;
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
                    addrLEDPower = addrFixGreenLEDPower[idx - 1];
                    addrLEDCurrent = addrFixGreenLEDCurrent[idx - 1];
                    addrLEDVoltage = addrFixGreenLEDVoltage[idx - 1];
                    addrPLC = 3;
                    break;

                case QueryType.FixRedLED:
                    addrLEDPower = addrFixRedLEDPower[idx - 1];
                    addrLEDCurrent = addrFixRedLEDCurrent[idx - 1];
                    addrLEDVoltage = addrFixRedLEDVoltage[idx - 1];
                    addrPLC = 1;
                    break;

                case QueryType.FixDarkRedLED:
                    addrLEDPower = addrFixDarkRedLEDPower[idx - 1];
                    addrLEDCurrent = addrFixDarkRedLEDCurrent[idx - 1];
                    addrLEDVoltage = addrFixDarkRedLEDVoltage[idx - 1];
                    addrPLC = 2;
                    break;

                case QueryType.DimDarkRedLED:
                    addrLEDPower = addrDimDarkRedLEDPower[idx - 1];
                    addrLEDCurrent = addrDimDarkRedLEDCurrent[idx - 1];
                    addrLEDVoltage = addrDimDarkRedLEDVoltage[idx - 1];
                    addrPLC = 2;
                    break;

                case QueryType.DimRedLED:
                    addrLEDPower = addrDimRedLEDPower[idx - 1];
                    addrLEDCurrent = addrDimRedLEDCurrent[idx - 1];
                    addrLEDVoltage = addrDimRedLEDVoltage[idx - 1];
                    addrPLC = 1;
                    break;

                case QueryType.DimGreenLED:
                    addrLEDPower = addrDimGreenLEDPower[idx - 1];
                    addrLEDCurrent = addrDimGreenLEDCurrent[idx - 1];
                    addrLEDVoltage = addrDimGreenLEDVoltage[idx - 1];
                    addrPLC = 3;
                    break;
            }

            byte[] recData = this.device.WriteReceive((byte)addrPLC, 3, (ushort)addrLEDPower, new byte[2] { 0x00, 0x01 });
            double[] values = ParseLEDPower(recData);
            recStatus.LEDPower = values[0];
            recData = this.device.WriteReceive((byte)addrPLC, 3, (ushort)addrLEDCurrent, new byte[2] { 0x00, 0x01 });
            values = ParseLEDCurrent(recData);
            recStatus.LEDCurrent = values[0];
            recData = this.device.WriteReceive((byte)addrPLC, 3, (ushort)addrLEDVoltage, new byte[2] { 0x00, 0x01 });
            values = ParseLEDVoltage(recData);
            recStatus.LEDCurrent = values[0];

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

            byte[] recData = this.device.WriteReceive((byte)addrPLC, 1, (ushort)addrPlcAChillerWarn[chillerIndex], new byte[2] { 0x00, 0x01 });
            chillerWarning = ParseChillerStatus(recData, 1);

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

            byte[] recData = this.device.WriteReceive((byte)addrPLC, 1, (ushort)addrPlcAPumpWarn[pumpIndex], new byte[2] { 0x00, 0x01 });
            pumpWarning = ParsePumpStatus(recData, 1);

            return pumpWarning[0];
        }

        public void TurnOnSkyLight(int addrPLC, int skyLightIndex)
        {
            SendCmd((byte)addrPLC, 5, (ushort)addrPlcASpareSwitch[skyLightIndex], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffSkyLight(int addrPLC, int skyLightIndex)
        {
            SendCmd((byte)addrPLC, 5, (ushort)addrPlcASpareSwitch[skyLightIndex], new byte[2] { 0x00, 0x00 });
        }

        public void TurnOnLight(int addrPLC, int LightIndex)
        {
            SendCmd((byte)addrPLC, 5, (ushort)addrPlcASpareSwitch[LightIndex + 3], new byte[2] { 0xFF, 0x00 });
        }

        public void TurnOffLight(int addrPLC, int LightIndex)
        {
            SendCmd((byte)addrPLC, 5, (ushort)addrPlcASpareSwitch[LightIndex + 3], new byte[2] { 0x00, 0x00 });
        }
    }

}
