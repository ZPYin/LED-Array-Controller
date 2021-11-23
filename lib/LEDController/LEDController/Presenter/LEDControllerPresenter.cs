using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using LEDController.View;
using LEDController.Model;
using System.Threading;
using System.Net.Sockets;
using System.Drawing;
using System.Windows.Threading;

namespace LEDController.Presenter
{
    class LEDControllerPresenter
    {
        private ILEDControllerForm _view;
        private LEDBoardCom connector = new LEDBoardCom();
        private FileSysIOClass fileIOer;
        private LEDStatus currentLEDStatus;
        private Thread threadListen = null;
        private byte[] receiveBytes = null;
        private const int SendBufferSize = 2 * 1024;
        private const int RecBufferSize = 8 * 1024;
        private const double MaxGreenLEDPower = 10.0;
        private const double MinGreenLEDPower = 0.0;
        private const double MaxRedLEDPower = 20.0;
        private const double MinRedLEDPower = 0.0;
        private const double MaxDarkRedLEDPower = 30.0;
        private const double MinDarkRedLEDPower = 0.0;
        private const int NumScrollBarLevel = 50;

        public LEDControllerPresenter(ILEDControllerForm newView)
        {
            _view = newView;
            _view.Connect += new EventHandler<EventArgs>(OnConnect);
            _view.CloseConnect += new EventHandler<EventArgs>(OnCloseConnect);
            _view.SendTestData += new EventHandler<EventArgs>(OnSendTestData);
            _view.ShowSingleLEDStatus += new EventHandler<EventEDArgs>(OnShowSingleLEDStatus);
            _view.ClearSingleLEDStatus += new EventHandler<EventEDArgs>(OnClearSingleLEDStatus);
            _view.OpenFixLED += new EventHandler<EventFixLEDArgs>(OnOpenFixLED);
            _view.CloseFixLED += new EventHandler<EventFixLEDArgs>(OnCloseFixLED);
            _view.HandleFixLED += new EventHandler<EventFixLEDArgs>(OnHandleFixLED);
            _view.OpenDimLED += new EventHandler<EventDimLEDArgs>(OnOpenDimLED);
            _view.CloseDimLED += new EventHandler<EventDimLEDArgs>(OnCloseDimLED);
            _view.HandleDimLED += new EventHandler<EventDimLEDArgs>(OnHandleDimLED);
            _view.UpdateScrollBar += new EventHandler<EventArgs>(OnUpdateScrollBar);
            _view.UpdateLEDTbx += new EventHandler<EventArgs>(OnUpdateLEDTbx);
            _view.ShowLEDStatus += new EventHandler<EventArgs>(OnShowLEDStatus);
            _view.lblGreenLEDMaxLeftText = Convert.ToString(MaxGreenLEDPower);
            _view.lblGreenLEDMaxRightText = Convert.ToString(MaxGreenLEDPower);
            _view.lblGreenLEDMinLeftText = Convert.ToString(MinGreenLEDPower);
            _view.lblGreenLEDMinRightText = Convert.ToString(MinGreenLEDPower);
            _view.lblRedLEDMaxLeftText = Convert.ToString(MaxRedLEDPower);
            _view.lblRedLEDMaxRightText = Convert.ToString(MaxRedLEDPower);
            _view.lblRedLEDMinLeftText = Convert.ToString(MinRedLEDPower);
            _view.lblRedLEDMinRightText = Convert.ToString(MinRedLEDPower);
            _view.lblDarkRedLEDMaxLeftText = Convert.ToString(MaxDarkRedLEDPower);
            _view.lblDarkRedLEDMaxRightText = Convert.ToString(MaxDarkRedLEDPower);
            _view.lblDarkRedLEDMinLeftText = Convert.ToString(MinDarkRedLEDPower);
            _view.lblDarkRedLEDMinRightText = Convert.ToString(MinDarkRedLEDPower);
        }

        private void OnShowLEDStatus(object sender, EventArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Show LED status
            
            // Get Status values

            // Convert values to color
            Color[] LEDControlColors = new Color[132];
            for (int i = 0; i < 132; i++)
            {
                LEDControlColors[i] = Color.Red;
            }

            // Set Colors
            _view.LEDStatusColors = LEDControlColors;
        }

        private void OnHandleDimLED(object sender, EventDimLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn on/off Dimmable LED Button
            try
            {
                Button btn = sender as Button;
                if (btn.BackColor.ToArgb().Equals(Color.Gray.ToArgb()))
                {
                    // Send control cmd
                    OnOpenDimLED(sender, e);
                }
                else
                {
                    // Send control cmd
                    OnCloseDimLED(sender, e);
                }
            }
            catch (Exception ex)
            {
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n";
            }
        }

        private void OnHandleFixLED(object sender, EventFixLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn on/off Fix LED Button
            try
            {
                Button btn = sender as Button;
                if (btn.BackColor.ToArgb().Equals(Color.Gray.ToArgb()))
                {
                    OnOpenFixLED(sender, e);
                }
                else
                {
                    OnCloseFixLED(sender, e);
                }
            }
            catch (Exception ex)
            {
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n";
            }
        }

        private void OnUpdateLEDTbx(object sender, EventArgs e)
        {
            // Update LED Power setting text box
            TrackBar tbar = sender as TrackBar;
            string LEDTbxText = null;
            double dimLEDPower = 0.0;

            int tagLED = Int32.Parse(tbar.Tag as string);
            dimLEDPower = CalcDimLEDPower(tbar.Value, tagLED);
            LEDTbxText = Convert.ToString(dimLEDPower);

            switch (tagLED)
            {
                case 1:
                    _view.tbxDimLED1Text = LEDTbxText;
                    break;
                case 2:
                    _view.tbxDimLED2Text = LEDTbxText;
                    break;
                case 3:
                    _view.tbxDimLED3Text = LEDTbxText;
                    break;
                case 4:
                    _view.tbxDimLED4Text = LEDTbxText;
                    break;
                case 5:
                    _view.tbxDimLED5Text = LEDTbxText;
                    break;
                case 6:
                    _view.tbxDimLED6Text = LEDTbxText;
                    break;
                case 7:
                    _view.tbxDimLED7Text = LEDTbxText;
                    break;
                case 8:
                    _view.tbxDimLED8Text = LEDTbxText;
                    break;
                case 9:
                    _view.tbxDimLED9Text = LEDTbxText;
                    break;
                case 10:
                    _view.tbxDimLED10Text = LEDTbxText;
                    break;
                case 11:
                    _view.tbxDimLED11Text = LEDTbxText;
                    break;
                case 12:
                    _view.tbxDimLED12Text = LEDTbxText;
                    break;
            }
        }

        private double CalcDimLEDPower(double sbarValue, int LEDIndex)
        {
            // Calculate Dimmable LED power
            double LEDPower = 0.0;

            if ((LEDIndex >= 1) && (LEDIndex <= LEDBoardCom.NumGreenFixLED))
            {
                // Green LED
                LEDPower = (sbarValue - 0) / NumScrollBarLevel * (MaxGreenLEDPower - MinGreenLEDPower) + MinGreenLEDPower;
                return LEDPower;
            }
            else if ((LEDIndex >= (LEDBoardCom.NumGreenDimLED + 1)) && (LEDIndex <= (LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumRedDimLED)))
            {
                // Red LED
                LEDPower = (sbarValue - 0) / NumScrollBarLevel * (MaxRedLEDPower - MinRedLEDPower) + MinRedLEDPower;
                return LEDPower;
            }
            else if ((LEDIndex >= (LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumRedDimLED + 1)) && (LEDIndex <= (LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumRedDimLED + LEDBoardCom.NumDarkRedDimLED)))
            {
                // DarkRed LED
                LEDPower = (sbarValue - 0) / NumScrollBarLevel * (MaxDarkRedLEDPower - MinDarkRedLEDPower) + MinDarkRedLEDPower;
                return LEDPower;
            }
            else
            {
                return 0.0;
            }
        }

        private void OnUpdateScrollBar(object sender, EventArgs e)
        {
            // Convert string to scroll bar value
            TextBox tbx = sender as TextBox;

            int tagLED = Int32.Parse(tbx.Tag as string);
            double dimLEDPower = Convert.ToDouble(tbx.Text);

            int sbarValue = CalcSbarValue(dimLEDPower, tagLED);

            switch (tagLED)
            {
                case 1:
                    _view.sbarDimLED1Value = sbarValue;
                    break;
                case 2:
                    _view.sbarDimLED2Value = sbarValue;
                    break;
                case 3:
                    _view.sbarDimLED3Value = sbarValue;
                    break;
                case 4:
                    _view.sbarDimLED4Value = sbarValue;
                    break;
                case 5:
                    _view.sbarDimLED5Value = sbarValue;
                    break;
                case 6:
                    _view.sbarDimLED6Value = sbarValue;
                    break;
                case 7:
                    _view.sbarDimLED7Value = sbarValue;
                    break;
                case 8:
                    _view.sbarDimLED8Value = sbarValue;
                    break;
                case 9:
                    _view.sbarDimLED9Value = sbarValue;
                    break;
                case 10:
                    _view.sbarDimLED10Value = sbarValue;
                    break;
                case 11:
                    _view.sbarDimLED11Value = sbarValue;
                    break;
                case 12:
                    _view.sbarDimLED12Value = sbarValue;
                    break;
            }
        }

        private int CalcSbarValue(double LEDPower, int LEDIndex)
        {
            // Calculate scroll bar value for Dimmable LED
            int sbarValue = 0;

            if ((LEDIndex >= 1) && (LEDIndex <= LEDBoardCom.NumGreenDimLED))
            {
                // Green LED
                if ((LEDPower < MinGreenLEDPower) || (LEDPower > MaxGreenLEDPower))
                {
                    MessageBox.Show("设定功率超出范围", "警告");
                    return sbarValue;
                }
                else
                {
                    sbarValue = Convert.ToInt32((LEDPower - MinGreenLEDPower) / (MaxGreenLEDPower - MinGreenLEDPower) * NumScrollBarLevel);
                }
                return sbarValue;
            }
            else if ((LEDIndex >= (LEDBoardCom.NumRedDimLED + 1)) && (LEDIndex <= (LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumRedDimLED)))
            {
                // Red LED
                if ((LEDPower < MinRedLEDPower) || (LEDPower > MaxRedLEDPower))
                {
                    MessageBox.Show("设定功率超出范围", "警告");
                    return sbarValue;
                }
                else
                {
                    sbarValue = Convert.ToInt32((LEDPower - MinRedLEDPower) / (MaxRedLEDPower - MinRedLEDPower) * NumScrollBarLevel);
                }
                return sbarValue;
            }
            else if ((LEDIndex >= (LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumRedDimLED + 1)) && (LEDIndex <= (LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumRedDimLED + LEDBoardCom.NumDarkRedDimLED)))
            {
                // DarkRed LED
                if ((LEDPower < MinDarkRedLEDPower) || (LEDPower > MaxDarkRedLEDPower))
                {
                    MessageBox.Show("设定功率超出范围", "警告");
                    return sbarValue;
                }
                else
                {
                    sbarValue = Convert.ToInt32((LEDPower - MinDarkRedLEDPower) / (MaxDarkRedLEDPower - MinDarkRedLEDPower) * NumScrollBarLevel);
                }
                return sbarValue;
            }
            else
            {
                return 0;
            }
        }

        private void OnOpenDimLED(object sender, EventDimLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn on Dimmable LED
            int LEDPowerBit;
            LEDPowerBit = (Int16)(CalcSbarValue(e.LEDPower, e.LEDIndex) / NumScrollBarLevel * 255);   // Convert to 0-255
            try
            {
                connector.TurnOnDimLED(e.LEDIndex, LEDPowerBit);
            }
            catch (Exception ex)
            {
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n";
            }

            ShowSendStatus();

            int numTotalFixLED = LEDBoardCom.NumGreenFixLED + LEDBoardCom.NumRedFixLED + LEDBoardCom.NumDarkRedFixLED;

            // set button color
            Button btn = sender as Button;
            int tagLED = Int32.Parse(btn.Tag as string);
            if ((tagLED >= (numTotalFixLED + 1)) && (tagLED <= (numTotalFixLED + LEDBoardCom.NumGreenDimLED)))
            {
                btn.BackColor = Color.Green;
            }
            else if ((tagLED >= (numTotalFixLED + LEDBoardCom.NumGreenDimLED + 1)) && (tagLED <= (numTotalFixLED + LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumRedDimLED)))
            {
                btn.BackColor = Color.Red;
            }
            else if ((tagLED >= (numTotalFixLED + LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumRedDimLED + 1)) && (tagLED <= (numTotalFixLED + LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumRedDimLED + LEDBoardCom.NumDarkRedDimLED)))
            {
                btn.BackColor = Color.DarkRed;
            }

            btn.Refresh();
        }

        private void OnCloseDimLED(object sender, EventDimLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn off Dimmable LED
            try
            {
                connector.TurnOffDimLED(e.LEDIndex);
            }
            catch (Exception ex)
            {
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n";
            }

            ShowSendStatus();

            // Change color
            Button btn = sender as Button;
            btn.BackColor = Color.Gray;
            btn.Refresh();
        }

        private void OnOpenFixLED(object sender, EventFixLEDArgs e)
        {
            if (connector.isAlive)
            {
                // Turn on Fix LED
                connector.TurnOnFixLED(e.LEDIndex);
                ShowSendStatus();

                // set button color
                Button btn = sender as Button;
                int tagLED = Int32.Parse(btn.Tag as string);
                if ((tagLED >= 1) && (tagLED <= LEDBoardCom.NumGreenFixLED))
                {
                    btn.BackColor = Color.Green;
                }
                else if ((tagLED >= (LEDBoardCom.NumGreenFixLED + 1)) && (tagLED <= (LEDBoardCom.NumGreenFixLED + LEDBoardCom.NumRedFixLED)))
                {
                    btn.BackColor = Color.Red;
                }
                else if ((tagLED >= (LEDBoardCom.NumGreenFixLED + LEDBoardCom.NumRedFixLED + 1)) && (tagLED <= (LEDBoardCom.NumGreenFixLED + LEDBoardCom.NumRedFixLED + LEDBoardCom.NumDarkRedFixLED)))
                {
                    btn.BackColor = Color.DarkRed;
                }

                btn.Refresh();
            }

        }

        private void OnCloseFixLED(object sender, EventFixLEDArgs e)
        {
            // Turn off Fix LED
            if (connector.isAlive)
            {
                connector.TurnOffFixLED(e.LEDIndex);

                ShowSendStatus();

                // Change color
                Button btn = sender as Button;
                btn.BackColor = Color.Gray;
                btn.Refresh();
            }
        }

        private void OnShowSingleLEDStatus(object sender, EventEDArgs e)
        {
            // Show LED status

            if ((connector != null) && (receiveBytes != null))
            {
                try
                {
                    // parsing status
                    LEDStatus thisLEDStatus;
                    thisLEDStatus = connector.ParsePackage(receiveBytes);

                    // show status
                    // LED power, current, voltage, temperature ...
                }
                catch (Exception ex)
                {
                    // show status
                    _view.toolStripLEDStatusText = "获取LED状态失败";
                }
            }
            else
            {
                // show status
                _view.toolStripLEDStatusText = "获取LED状态失败";
            }
        }

        private void OnClearSingleLEDStatus(object sender, EventEDArgs e)
        {
            // Clear LED status
            _view.toolStripLEDStatusText = "LED状态";
        }

        private void OnSendTestData(object sender, EventArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Send test data
            try
            {
                connector.SendCmd(_view.testCmdStr);
                ShowSendStatus();
            }
            catch (Exception ex)
            {
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n";
            }
        }

        public void ReceiveMsg()
        {
            // Socket socketSlave = socketHostObj as Socket;
            byte[] buffer = new byte[SendBufferSize];

            while (true)   // Receiving message from slave
            {
                int msgLen = 0;

                try
                {
                    if (connector.socketHost != null) msgLen = connector.socketHost.Receive(buffer);

                    if (msgLen > 0)
                    {
                        LEDStatus currentLEDStatus = connector.ParsePackage(buffer);

                        string recMsg = Encoding.UTF8.GetString(buffer);
                        recMsg = recMsg.Replace("\0", "");
                        // TODO add timestamp

                        // format recMsg
                        string formatRecMsg = "[" + GetCurrentTime() + "] " + "\r\n" + recMsg + "\r\n";
                        _view.Invoke(new Action(() => { _view.testMsgRecStr = formatRecMsg; }));

                        // showing receiving status with colorful light
                        ShowReceiveStatus();

                        // showing total power
                        _view.tsslGreenLEDTPText = "绿光实时总功率: 1W";
                        _view.tsslRedLEDTPText = "红光实时总功率: 2W";
                        _view.tsslDarkRedTPText = "红外实时总功率: 3W";

                        // showing temperature sensors
                        _view.tsslTemp1Text = "测温点1: 20°C";
                        _view.tsslTemp2Text = "测温点2: 20°C";
                        _view.tsslTemp3Text = "测温点3: 20°C";
                        _view.tsslTemp4Text = "测温点4: 20°C";
                    }
                }
                catch (SocketException ex)
                {
                    // TODO: Write Log
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n";
                }
            }
        }

        private void ShowSendStatus()
        {
            // // Async
            // DelayAction(0, new Action(() => { _view.btnSendStatus1Color = Color.Green; }));
            // DelayAction(100, new Action(() => { _view.btnSendStatus2Color = Color.Green; }));
            // DelayAction(100, new Action(() => { _view.btnSendStatus3Color = Color.Green; }));
            // DelayAction(300, new Action(() => { 
            //     _view.btnSendStatus1Color = Color.DarkRed;
            //     _view.btnSendStatus2Color = Color.DarkRed;
            //     _view.btnSendStatus3Color = Color.DarkRed; 
            // }));

            // Sync
            _view.btnSendStatus1Color = Color.Green;
            Thread.Sleep(100);
            _view.btnSendStatus2Color = Color.Green;
            Thread.Sleep(100);
            _view.btnSendStatus3Color = Color.Green;
            Thread.Sleep(100);
            _view.btnSendStatus1Color = Color.DarkRed;
            _view.btnSendStatus2Color = Color.DarkRed;
            _view.btnSendStatus3Color = Color.DarkRed;

        }

        private void ShowReceiveStatus()
        {
            // Show status bar for sending data
            DelayAction(0, new Action(() => { _view.btnRecStatus1Color = Color.Green; }));
            DelayAction(100, new Action(() => { _view.btnRecStatus2Color = Color.Green; }));
            DelayAction(100, new Action(() => { _view.btnRecStatus3Color = Color.Green; }));
            DelayAction(100, new Action(() => { 
                _view.btnRecStatus1Color = Color.DarkRed;
                _view.btnRecStatus2Color = Color.DarkRed;
                _view.btnRecStatus3Color = Color.DarkRed; 
            }));
        }

        public void DelayAction(int millisecond, Action action)
        {
            // Timer delay for action
            var timer = new DispatcherTimer();
            timer.Tick += delegate

            {
                _view.Invoke(action);
                timer.Stop();
            };

            timer.Interval = TimeSpan.FromMilliseconds(millisecond);
            timer.Start();
        }

        private void OnConnect(object sender, EventArgs e)
        {
            // Start TCP connection
            connector.slaveIP = _view.slaveIP;
            connector.slavePort = _view.slavePort;

            try
            {
                connector.Connect();
                ShowSendStatus();
                _view.toolStripConnectionStatusText = "连接成功";
                // show connection message
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + " 连接成功\r\n";
                _view.btnConnectColor = Color.Green;
                _view.btnCloseColor = Color.Empty;

                threadListen = new Thread(ReceiveMsg);
                threadListen.IsBackground = true;
                threadListen.Start();
            }
            catch (Exception ex)
            {
                _view.toolStripConnectionStatusText = "连接失败";
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + " 连接失败\r\n";
                _view.tbxConnectMsgText = ex.ToString() + "\r\n";
                _view.btnConnectColor = Color.Empty;
            }
        }

        private void OnCloseConnect(object sender, EventArgs e)
        {
            // Close connection
            try
            {
                if (threadListen != null) threadListen.Abort();
                connector.Close();
                ShowSendStatus();
                _view.toolStripConnectionStatusText = "断开成功";
                // show disconnect message
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + " 断开成功\r\n";
                _view.btnCloseColor = Color.Empty;
                _view.btnConnectColor = Color.Empty;
            }
            catch (Exception ex)
            {
                _view.toolStripConnectionStatusText = "断开失败";
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + " 断开失败\r\n";
                _view.tbxConnectMsgText = ex.ToString() + "\r\n";
                _view.btnCloseColor = Color.Red;
            }
        }

        public DateTime GetCurrentTime()
        {
            // Get current timestamp
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;

            return currentTime;
        }
    }
}
