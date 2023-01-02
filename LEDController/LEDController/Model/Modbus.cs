using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace LEDController.Model
{
    public abstract class Modbus
    {

        public abstract bool Connect(int timeout);

        public abstract bool Disconnect();

        public abstract byte[] Read(byte addrPLC, byte function, ushort register, ushort count);

        public abstract void Write(byte addrPLC, byte function, ushort register, byte[] data);

        public abstract byte[] WriteReceive(byte addrPLC, byte function, ushort register, byte[] data);

        public abstract void WriteMsg(string msg, bool isSendHEX);

        public abstract byte[] ReadMsg(bool isReceiveHEX);

        public byte[] MakePacket(byte addrPLC, byte function, ushort register, ushort count)
        {
            return new byte[] {
                addrPLC,
                function,
                (byte)(register >> 8),
                (byte)register,
                (byte)(count >> 8),
                (byte)count
            };
        }

        public byte[] MakePacket(byte addrPLC, byte function, ushort register, byte[] data)
        {
            ushort count = (ushort)(data.Count() / 2);
            byte[] header = new byte[] {
                addrPLC,
                function,
                (byte)(register >> 8),
                (byte)register
            };

            return header.Concat(data).ToArray();
        }

        public ushort[] ReadInputReg(byte addrPLC, ushort register, ushort count)
        {
            ushort[] data;
            int dataI = 0;
            byte[] rPacket = Read(addrPLC, 4, register, count);
            data = new ushort[(rPacket.Length) / 2];
            for (int i = 0; i < rPacket.Length; i += 2)
            {
                data[dataI] = rPacket[i];
                data[dataI] <<= 8;
                data[dataI] += rPacket[i + 1];
                dataI++;
            }

            return data;
        }

        public ushort[] ReadHolding(byte addrPLC, ushort register, ushort count)
        {
            ushort[] data;
            int dataI = 0;
            byte[] rPacket = Read(addrPLC, 3, register, count);
            data = new ushort[(rPacket.Length) / 2];
            for (int i = 0; i < rPacket.Length; i += 2)
            {
                data[dataI] = rPacket[i];
                data[dataI] <<= 8;
                data[dataI] += rPacket[i + 1];
                dataI++;
            }

            return data;
        }

        public float[] ReadHoldingFloat(byte addrPLC, UInt16 register, UInt16 count)
        {
            byte[] rVal = Read(addrPLC, 3, register, (ushort)(count * 2));
            float[] values = new float[rVal.Length / 4];
            for (int i = 0; i < rVal.Length; i += 4)
            {
                values[i / 4] = BitConverter.ToSingle(new byte[] { rVal[i + 1], rVal[i], rVal[i + 3], rVal[i + 2] }, 0);
            }

            return values;
        }

        public void WriteHolding(byte addrPLC, ushort register, ushort[] data)
        {
            byte[] bData = new byte[data.Count() * 2];
            int i = 0;
            foreach (ushort item in data)
            {
                bData[i] = (byte)(item >> 8);
                bData[i + 1] = (byte)item;
                i += 2;
            }

            Write(addrPLC, 16, register, bData);
        }

        public void WriteHolding(byte addrPLC, ushort register, ushort data)
        {
            WriteHolding(addrPLC, register, new ushort[] { data });
        }

        public void WriteHoldingFloat(byte addrPLC, ushort register, float[] data)
        {
            byte[] bData = new byte[data.Count() * 4];
            byte[] myData;
            int i = 0;
            foreach (float item in data)
            {
                myData = BitConverter.GetBytes(item);
                bData[i + 1] = myData[0];
                bData[i] = myData[1];
                bData[i + 3] = myData[2];
                bData[i + 2] = myData[3];
                i += 4;
            }

            Write(addrPLC, 16, register, bData);
        }

        public void WriteHoldingFloat(byte addrPLC, ushort register, float data)
        {
            WriteHoldingFloat(addrPLC, register, new float[] { data });
        }

        public void WriteHoldingFloat(byte addrPLC, ushort register, double data)
        {
            WriteHoldingFloat(addrPLC, register, (float)data);
        }
    }

    public class ModbusTCP : Modbus
    {
        private Socket socket;
        private string _ipAddress;
        private string _port;
        public const int SendBufferSize = 2 * 1024;
        public const int ReceiveBufferSize = 2 * 1024;

        public ModbusTCP(string ipAddress, int port = 502)
        {
            this._ipAddress = ipAddress;
            this._port = port.ToString();
        }

        public override bool Connect(int timeout)
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddress = IPAddress.Parse(this._ipAddress);
            IPEndPoint endPoint = new IPEndPoint(ipAddress, int.Parse(this._port));

            IAsyncResult result = this.socket.BeginConnect(endPoint, null, null);
            bool isSuccess = result.AsyncWaitHandle.WaitOne(timeout, true);
            if (!isSuccess)
            {
                this.socket.Close();
                throw new IOException("Failed in connecting TCP/IP.");
            }
            else
            {
                this.socket.ReceiveTimeout = timeout;
                this.socket.SendTimeout = timeout;
            }

            return true;
        }

        public override bool Disconnect()
        {
            if (this.socket != null)
            {
                this.socket.Close(1);
                return true;
            }
            else
            {
                return true;
            }
        }

        private byte[] MakeMBAP(byte addrPLC, ushort count)
        {
            byte[] idBytes = BitConverter.GetBytes((short)addrPLC);
            return new byte[] {
                idBytes[0],
                idBytes[1],
                0,
                0,
                (byte)(count >> 8),
                (byte)(count)
            };
        }

        private byte[] SendReceive(byte[] packet)
        {
            byte[] mbap = new byte[7];
            byte[] response;
            ushort count;

            try
            {
                socket.Send(packet);
                socket.Receive(mbap, 0, mbap.Length, SocketFlags.None);
                count = mbap[4];
                count <<= 8;
                count += mbap[5];
                response = new byte[count - 1];
                socket.Receive(response, 0, response.Count(), SocketFlags.None);
            }
            catch
            {
                throw new SocketException(10060);
            }

            if (response[0] > 128)
            {
                throw new SocketException();
            }

            return response;
        }

        public override byte[] Read(byte addrPLC, byte function, ushort register, ushort count)
        {
            byte[] rtn;
            byte[] packet = MakePacket(addrPLC, function, register, count);
            byte[] mbap = MakeMBAP(addrPLC, (ushort)packet.Count());
            byte[] response = SendReceive(mbap.Concat(packet).ToArray());

            rtn = new byte[response[1]];
            Array.Copy(response, 2, rtn, 0, rtn.Length);

            return rtn;
        }

        public override void Write(byte addrPLC, byte function, ushort register, byte[] data)
        {
            byte[] packet;
            packet = MakePacket(addrPLC, function, register, data);

            socket.Send(MakeMBAP(addrPLC, (ushort)packet.Count()).Concat(packet).ToArray());
        }

        public override byte[] WriteReceive(byte addrPLC, byte function, ushort register, byte[] data)
        {
            byte[] packet;
            packet = MakePacket(addrPLC, function, register, data);

            return SendReceive(MakeMBAP(addrPLC, (ushort)packet.Count()).Concat(packet).ToArray());
        }

        public override void WriteMsg(string msg, bool isSendHEX)
        {
            if (isSendHEX)
            {
                char[] msgChars = msg.ToCharArray();
                byte[] sendBytes = new byte[msgChars.Length];
                for (int i = 0; i < msgChars.Length; i++)
                {
                    sendBytes[i] = Convert.ToByte(msgChars[i]);
                }

                this.socket.Send(sendBytes);
            }
            else
            {
                this.socket.Send(Encoding.UTF8.GetBytes(msg));
            }
        }

        public override byte[] ReadMsg(bool isReceiveHEX)
        {
            byte[] buffer = new byte[SendBufferSize];
            int msgLen = 0;

            try
            {
                while (socket.Available > 0)
                {
                    msgLen += socket.Receive(buffer, msgLen, buffer.Length - msgLen, SocketFlags.None);
                    Thread.Sleep(50);
                }
            }
            catch (SocketException ex)
            {
                throw ex;
            }

            byte[] recData = new byte[msgLen];
            if (isReceiveHEX)
            {
                int value;
                for (int i = 0; i < msgLen; i++)
                {
                    value = Convert.ToInt32(buffer[i]);
                    recData[i] = Convert.ToByte(String.Format("{0:X}", value));
                }
            }
            else
            {
                for (int i = 0; i < msgLen; i++)
                {
                    recData[i] = buffer[i];
                }
            }

            return recData;
        }
    }

    public class ModbusRTU : Modbus
    {
        private SerialPort serialPort;
        private int modbusTimeout = 30;
        private string _portName;
        private string _baudRate;
        private string _dataBit;
        private string _checkBit;
        private string _stopBit;
        public const int SendBufferSize = 2 * 1024;
        public const int ReceiveBufferSize = 2 * 1024;

        public ModbusRTU(string portName, string baudRate, string dataBit, string checkBit, string stopBit)
        {
            this._portName = portName;
            this._baudRate = baudRate;
            this._dataBit = dataBit;
            this._checkBit = checkBit;
            this._stopBit = stopBit;
        }

        public override bool Connect(int timeout)
        {
            this.serialPort = new SerialPort();
            if (!this.serialPort.IsOpen)
            {
                try
                {
                    this.serialPort.PortName = this._portName;
                    this.serialPort.BaudRate = Convert.ToInt32(this._baudRate);
                    this.serialPort.DataBits = Convert.ToInt32(this._dataBit);
                    this.serialPort.ReadTimeout = timeout;
                    this.serialPort.DtrEnable = true;
                    this.serialPort.RtsEnable = true;

                    switch (this._stopBit)
                    {
                        case "1":
                            this.serialPort.StopBits = StopBits.One;
                            break;

                        case "1.5":
                            this.serialPort.StopBits = StopBits.OnePointFive;
                            break;

                        case "2":
                            this.serialPort.StopBits = StopBits.Two;
                            break;
                    }

                    switch (this._checkBit)
                    {
                        case "None":
                            this.serialPort.Parity = Parity.None;
                            break;

                        case "Odd":
                            this.serialPort.Parity = Parity.Odd;
                            break;

                        case "Even":
                            this.serialPort.Parity = Parity.Even;
                            break;
                    }

                    this.serialPort.Open();

                    return true;
                }
                catch
                {
                    throw new IOException("Failed in connection serial port.");
                }
            }
            else
            {
                return true;
            }
        }

        public override bool Disconnect()
        {
            if (this.serialPort != null)
            {
                this.serialPort.Close();
                return true;
            }
            else
            {
                return true;
            }
        }

        private ushort MakeCRC(byte[] data, int count)
        {
            ushort crc = 0xFFFF;
            byte lsb;

            for (int i = 0; i < count; i++)
            {
                crc = (ushort)(crc ^ (ushort)data[i]);

                for (int j = 0; j < 8; j++)
                {
                    lsb = (byte)(crc & 0x0001);
                    crc = (ushort)((crc >> 1) & 0x7FFF);
                    if (lsb == 1)
                    {
                        crc = (ushort)(crc ^ 0xA001);
                    }
                }
            }

            return crc;
        }

        private byte[] SendReceive(byte[] packet)
        {
            byte[] buffer = new byte[ReceiveBufferSize];
            byte[] rtn;
            int msgLen;
            int timeoutTmp;
            try
            {
                serialPort.Write(packet, 0, packet.Count());
                Thread.Sleep(modbusTimeout);
                msgLen = serialPort.Read(buffer, 0, 1);
                timeoutTmp = serialPort.ReadTimeout;
                serialPort.ReadTimeout = modbusTimeout;
                msgLen += serialPort.Read(buffer, 1, ReceiveBufferSize - 1);
            }
            catch
            {
                return new byte[0];
                // throw new IOException("ModBus RTU error: No data received.");
            }

            byte[] subArray = new byte[msgLen - 2];
            if (msgLen > 3)
            {
                Array.Copy(buffer, 0, subArray, 0, msgLen - 2);
            }
            else
            {
                throw new IOException("ModBus RTU error: wrong data package.");
            }

            if ((buffer[1] > 128) || (MakeCRC(subArray, subArray.Length) == BitConverter.ToInt16(new byte[4] {buffer[msgLen - 4], buffer[msgLen - 3], buffer[msgLen - 2], buffer[msgLen - 1]}, 0)))
            {
                throw new IOException("ModBus RTU error(" + (buffer[2]) + ")");
            }
            else if (buffer[1] < 5)
            {
                rtn = new byte[buffer[2]];
                Array.Copy(buffer, 3, rtn, 0, buffer[2]);
            }
            else
            {
                rtn = new byte[4];
                Array.Copy(buffer, 2, rtn, 0, 4);
            }

            serialPort.ReadTimeout = timeoutTmp;

            return rtn;
        }

        public override byte[] Read(byte addrPLC, byte function, ushort register, ushort count)
        {
            byte[] packet = MakePacket(addrPLC, function, register, count);
            ushort crc = MakeCRC(packet, packet.Count());
            byte[] crcBytes = new byte[] { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF) };

            return SendReceive(packet.Concat(crcBytes).ToArray());
        }

        public override void Write(byte addrPLC, byte function, ushort register, byte[] data)
        {
            byte[] packet;
            packet = MakePacket(addrPLC, function, register, data);
            ushort crc = MakeCRC(packet, packet.Count());
            byte[] crcBytes = new byte[] { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF) };
            
            byte[] sendData = packet.Concat(crcBytes).ToArray();
            serialPort.Write(sendData, 0, sendData.Count());
        }

        public override byte[] WriteReceive(byte addrPLC, byte function, ushort register, byte[] data)
        {
            byte[] packet;
            packet = MakePacket(addrPLC, function, register, data);
            ushort crc = MakeCRC(packet, packet.Count());
            byte[] crcBytes = new byte[] { (byte)(crc & 0xFF), (byte)((crc >> 8) & 0xFF) };
            
            return SendReceive(packet.Concat(crcBytes).ToArray());
        }

        public override void WriteMsg(string msg, bool isSendHEX)
        {
            if (isSendHEX)
            {
                char[] msgChars = msg.ToCharArray();
                int value;
                string sendStr = "";
                for (int i = 0; i < msgChars.Length; i++)
                {
                    value = Convert.ToInt32(msgChars[i]);
                    sendStr.Concat(String.Format("{0:X}", value));
                }

                this.serialPort.Write(sendStr);
            }
            else
            {
                this.serialPort.Write(msg);
            }
        }
        

        public override byte[] ReadMsg(bool isReceiveHEX)
        {
            byte[] buffer = new byte[SendBufferSize];
            int msgLen = 0;

            try
            {
                while (serialPort.BytesToRead > 0)
                {
                    msgLen += serialPort.Read(buffer, msgLen, buffer.Length - msgLen);
                    Thread.Sleep(50);
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            byte[] recData = new byte[msgLen];
            if (isReceiveHEX)
            {
                int value;
                for (int i = 0; i < msgLen; i++)
                {
                    value = Convert.ToInt32(buffer[i]);
                    recData[i] = Convert.ToByte(String.Format("{0:X}", value));
                }
            }
            else
            {
                for (int i = 0; i < msgLen; i++)
                {
                    recData[i] = buffer[i];
                }
            }

            return recData;
        }
    }
}
