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
        private LEDBoardCom connector;
        private FileSysIOClass fileIOer;
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

        public LEDControllerPresenter(ILEDControllerForm newView)
        {
            _view = newView;
            _view.Connect += new EventHandler<EventArgs>(Connect);
            _view.CloseConnect += new EventHandler<EventArgs>(CloseConnect);
            _view.SendTestData += new EventHandler<EventArgs>(SendTestData);
            _view.ShowSingleLEDStatus += new EventHandler<EventArgs>(ShowSingleLEDStatus);
            _view.ClearSingleLEDStatus += new EventHandler<EventArgs>(ClearSingleLEDStatus);
            _view.OpenFixLED += new EventHandler<EventArgs>(OpenFixLED);
            _view.CloseFixLED += new EventHandler<EventArgs>(CloseFixLED);
            _view.OpenDimLED += new EventHandler<EventArgs>(OpenDimLED);
            _view.CloseDimLED += new EventHandler<EventArgs>(CloseDimLED);
            _view.UpdateScrollBar += new EventHandler<EventArgs>(UpdateScrollBar);
            _view.UpdateLEDTbx += new EventHandler<EventArgs>(UpdateLEDTbx);
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

        private void UpdateLEDTbx(object sender, EventArgs e)
        {
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
            double LEDPower = 0.0;

            if ((LEDIndex >= 1) && (LEDIndex <= 4))
            {
                // Green LED
                LEDPower = (sbarValue - 0) / 50 * (MaxGreenLEDPower - MinGreenLEDPower) + MinGreenLEDPower;
                return LEDPower;
            }
            else if ((LEDIndex >= 5) && (LEDIndex <= 8))
            {
                // Red LED
                LEDPower = (sbarValue - 0) / 50 * (MaxRedLEDPower - MinRedLEDPower) + MinRedLEDPower;
                return LEDPower;
            }
            else if ((LEDIndex >= 9) && (LEDIndex <= 12))
            {
                // DarkRed LED
                LEDPower = (sbarValue - 0) / 50 * (MaxDarkRedLEDPower - MinDarkRedLEDPower) + MinDarkRedLEDPower;
                return LEDPower;
            }
            else
            {
                return 0.0;
            }
        }

        private void UpdateScrollBar(object sender, EventArgs e)
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
            int sbarValue = 0;

            if ((LEDIndex >= 1) && (LEDIndex <= 4))
            {
                // Green LED
                if ((LEDPower < MinGreenLEDPower) || (LEDPower > MaxGreenLEDPower))
                {
                    MessageBox.Show("设定功率超出范围", "警告");
                    return sbarValue;
                }
                else
                {
                    sbarValue = Convert.ToInt32((LEDPower - MinGreenLEDPower) / (MaxGreenLEDPower - MinGreenLEDPower) * 50);
                }
                return sbarValue;
            }
            else if ((LEDIndex >= 5) && (LEDIndex <= 8))
            {
                // Red LED
                if ((LEDPower < MinRedLEDPower) || (LEDPower > MaxRedLEDPower))
                {
                    MessageBox.Show("设定功率超出范围", "警告");
                    return sbarValue;
                }
                else
                {
                    sbarValue = Convert.ToInt32((LEDPower - MinRedLEDPower) / (MaxRedLEDPower - MinRedLEDPower) * 50);
                }
                return sbarValue;
            }
            else if ((LEDIndex >= 9) && (LEDIndex <= 12))
            {
                // DarkRed LED
                if ((LEDPower < MinDarkRedLEDPower) || (LEDPower > MaxDarkRedLEDPower))
                {
                    MessageBox.Show("设定功率超出范围", "警告");
                    return sbarValue;
                }
                else
                {
                    sbarValue = Convert.ToInt32((LEDPower - MinDarkRedLEDPower) / (MaxDarkRedLEDPower - MinDarkRedLEDPower) * 50);
                }
                return sbarValue;
            }
            else
            {
                return 0;
            }
        }

        private void OpenDimLED(object sender, EventArgs e)
        {
            // send command to slave

            ShowSendStatus();

            // set button color
            Button btn = sender as Button;
            int tagLED = Int32.Parse(btn.Tag as string);
            if ((tagLED >= 121) && (tagLED <= 124))
            {
                btn.BackColor = Color.Green;
            }
            else if ((tagLED >= 125) && (tagLED <= 128))
            {
                btn.BackColor = Color.Red;
            }
            else if ((tagLED >= 129) && (tagLED <= 132))
            {
                btn.BackColor = Color.DarkRed;
            }

            btn.Refresh();
        }

        private void CloseDimLED(object sender, EventArgs e)
        {
            // send command to slave

            ShowSendStatus();

            // Change color
            Button btn = sender as Button;
            btn.BackColor = Color.Gray;
            btn.Refresh();
        }

        private void OpenFixLED(object sender, EventArgs e)
        {

            // send command to slave 

            ShowSendStatus();

            // set button color
            Button btn = sender as Button;
            int tagLED = Int32.Parse(btn.Tag as string);
            if ((tagLED >= 1) && (tagLED <= 40))
            {
                btn.BackColor = Color.Green;
            }
            else if ((tagLED >= 41) && (tagLED <= 80))
            {
                btn.BackColor = Color.Red;
            }
            else if ((tagLED >= 81) && (tagLED <= 120))
            {
                btn.BackColor = Color.DarkRed;
            }

            btn.Refresh();

        }

        private void CloseFixLED(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            ShowSendStatus();

            // Change color
            btn.BackColor = Color.Gray;
            btn.Refresh();
        }

        private void ShowSingleLEDStatus(object sender, EventArgs e)
        {
            if (connector != null)
            {
                // parsing status
                LEDStatus thisLEDStatus;
                thisLEDStatus = connector.ParseStatus(this.receiveBytes);

                // show status
                _view.toolStripLEDStatusText = "test";
            }
        }

        private void ClearSingleLEDStatus(object sender, EventArgs e)
        {
            _view.toolStripLEDStatusText = "LED状态";
        }
        private void SendTestData(object sender, EventArgs e)
        {
            try
            {
                connector.SendCmd(_view.testCmdStr);
                ShowSendStatus();
            }
            catch (Exception ex)
            {
                throw ex;
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
                        receiveBytes = buffer;
                        string recMsg = Encoding.UTF8.GetString(buffer);
                        recMsg = recMsg.Replace("\0", "");
                        // TODO add timestamp

                        // format recMsg
                        string formatRecMsg = "[" + GetCurrentTime() + "] " + "\r\n" + recMsg + "\r\n";
                        _view.Invoke(new Action(() => { _view.testMsgRecStr = formatRecMsg; }));

                        // showing receiving status with colorful light
                        _view.btnRecStatus1Color = Color.Green;
                        Thread.Sleep(100);
                        _view.btnRecStatus2Color = Color.Green;
                        Thread.Sleep(100);
                        _view.btnRecStatus3Color = Color.Green;
                        Thread.Sleep(300);
                        _view.btnRecStatus1Color = Color.DarkRed;
                        _view.btnRecStatus2Color = Color.DarkRed;
                        _view.btnRecStatus3Color = Color.DarkRed;

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
                    throw ex;
                }
            }
        }

        private void ShowSendStatus()
        {
            DelayAction(0, new Action(() => { _view.btnSendStatus1Color = Color.Green; }));
            DelayAction(100, new Action(() => { _view.btnSendStatus2Color = Color.Green; }));
            DelayAction(100, new Action(() => { _view.btnSendStatus3Color = Color.Green; }));
            DelayAction(300, new Action(() => { 
                _view.btnSendStatus1Color = Color.DarkRed;
                _view.btnSendStatus2Color = Color.DarkRed;
                _view.btnSendStatus3Color = Color.DarkRed; 
            }));
        }

        public void DelayAction(int millisecond, Action action)
        {
            var timer = new DispatcherTimer();
            timer.Tick += delegate

            {
                _view.Invoke(action);
                timer.Stop();
            };

            timer.Interval = TimeSpan.FromMilliseconds(millisecond);
            timer.Start();
        }

        private void Connect(object sender, EventArgs e)
        {
            connector = new LEDBoardCom(_view.slaveIP, _view.slavePort);

            try
            {
                connector.Connect();
                ShowSendStatus();
                _view.toolStripConnectionStatusText = "连接成功";
                // show connection message
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + " 连接成功\r\n";

                threadListen = new Thread(ReceiveMsg);
                threadListen.IsBackground = true;
                threadListen.Start();
            }
            catch (Exception ex)
            {
                _view.toolStripConnectionStatusText = "连接失败";
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + " 连接失败\r\n";
                _view.tbxConnectMsgText = ex.ToString() + "\r\n";
                throw ex;
            }
        }

        private void CloseConnect(object sender, EventArgs e)
        {
            try
            {
                if (threadListen != null) threadListen.Abort();
                connector.Close();
                ShowSendStatus();
                _view.toolStripConnectionStatusText = "断开成功";
                // show disconnect message
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + " 断开成功\r\n";
            }
            catch (Exception ex)
            {
                _view.toolStripConnectionStatusText = "断开失败";
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + " 断开失败\r\n";
                _view.tbxConnectMsgText = ex.ToString() + "\r\n";
                throw ex;
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
