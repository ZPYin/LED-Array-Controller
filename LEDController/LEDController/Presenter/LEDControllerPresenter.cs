using LEDController.View;
using LEDController.Model;
using System;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Drawing;
using System.Windows.Threading;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace LEDController.Presenter
{
    class LEDControllerPresenter
    {
        private const int SendBufferSize = 2 * 1024;
        private const int RecBufferSize = 8 * 1024;
        private const double MaxGreenLEDPower = 10.0;
        private const double MinGreenLEDPower = 0.0;
        private const double MaxRedLEDPower = 20.0;
        private const double MinRedLEDPower = 0.0;
        private const double MaxDarkRedLEDPower = 30.0;
        private const double MinDarkRedLEDPower = 0.0;
        private const int NumScrollBarLevel = 50;
        private const int NumLiveData = 3600;

        private LEDControllerViewer _view;
        private LEDBoardCom connector;
        private LEDStatus currentLEDStatus;
        private byte[] receiveBytes = null;
        private double[] LEDPowerLiveData = new double[NumLiveData];
        private double[] LEDCurrentLiveData = new double[NumLiveData];
        private double[] LEDVoltageLiveData = new double[NumLiveData];
        public Stopwatch sw;
        public System.Threading.Timer updateLEDStatusTimer;
        public System.Threading.Timer renderLEDStatusTimer;
        BackgroundWorker recWorker = new BackgroundWorker();
        public string cfgFileName;

        public LEDControllerPresenter(LEDControllerViewer newView)
        {
            this.sw = Stopwatch.StartNew();
            this.connector = new LEDBoardCom();
            recWorker.WorkerReportsProgress = true;
            recWorker.WorkerSupportsCancellation = true;
            recWorker.DoWork += ReceiveMsg;
            recWorker.ProgressChanged += ShowReceiveStatusAsync;
            recWorker.RunWorkerCompleted += OnConnectionBreak;

            _view = newView;
            _view.ConnectTCP += new EventHandler<EventArgs>(OnConnectTCP);
            _view.CloseTCP += new EventHandler<EventArgs>(OnCloseTCP);
            _view.ConnectSerialPort += new EventHandler<EventArgs>(OnConnectSerialPort);
            _view.CloseSerialPort += new EventHandler<EventArgs>(OnCloseSerialPort);
            _view.SendTestData += new EventHandler<EventArgs>(OnSendTestData);
            _view.ShowSingleLEDStatus += new EventHandler<EventLEDArgs>(OnShowSingleLEDStatus);
            _view.ClearSingleLEDStatus += new EventHandler<EventLEDArgs>(OnClearSingleLEDStatus);
            _view.OpenFixLED += new EventHandler<EventLEDArgs>(OnOpenFixLED);
            _view.CloseFixLED += new EventHandler<EventLEDArgs>(OnCloseFixLED);
            _view.HandleFixLED += new EventHandler<EventLEDArgs>(OnHandleFixLED);
            _view.SetDimLED += new EventHandler<EventDimLEDArgs>(OnSetDimLED);
            _view.CloseDimLED += new EventHandler<EventDimLEDArgs>(OnCloseDimLED);
            _view.HandleDimLED += new EventHandler<EventDimLEDArgs>(OnHandleDimLED);
            _view.UpdateScrollBar += new EventHandler<EventDimLEDArgs>(OnUpdateScrollBar);
            _view.UpdateLEDTbx += new EventHandler<EventDimLEDArgs>(OnUpdateLEDTbx);
            _view.ShowLEDStatus += new EventHandler<EventArgs>(OnShowLEDStatus);
            _view.OpenWithCfgFile += new EventHandler<EventArgs>(OnOpenWithCfgFile);
            _view.SaveCfgFile += new EventHandler<EventArgs>(OnSaveCfgFile);
            _view.SaveasCfgFile += new EventHandler<EventArgs>(OnSaveasCfgFile);
            _view.ShowVersion += new EventHandler<EventArgs>(OnShowVersion);

            // Initialize Form
            InitialForm();

            // Initialize LED status plot
            sw.Start();
            Random rand = new Random(0);
            var sig = _view.formsLEDStatusPlot.Plot.AddSignal(LEDPowerLiveData, sampleRate: 3600 * 24.0, label: "功率");
            _view.formsLEDStatusPlot.Plot.AddSignal(LEDCurrentLiveData, sampleRate: 3600 * 24.0, label: "电流");
            _view.formsLEDStatusPlot.Plot.AddSignal(LEDVoltageLiveData, sampleRate: 3600 * 24.0, label: "电压");
            _view.formsLEDStatusPlot.Plot.Title("LED状态");
            _view.formsLEDStatusPlot.Plot.XLabel("时间");
            _view.formsLEDStatusPlot.Plot.YLabel("强度");
            _view.formsLEDStatusPlot.Plot.Grid(true);
            _view.formsLEDStatusPlot.Plot.SetAxisLimitsY(0, 20);
            sig.OffsetX = GetCurrentTime().ToOADate();
            _view.formsLEDStatusPlot.Plot.SetAxisLimitsX(GetCurrentTime().ToOADate(), GetCurrentTime().AddHours(1.0).ToOADate());
            _view.formsLEDStatusPlot.Plot.XAxis.ManualTickSpacing(5, ScottPlot.Ticks.DateTimeUnit.Minute);
            _view.formsLEDStatusPlot.Plot.XAxis.TickLabelFormat("HH:mm", true);
            _view.formsLEDStatusPlot.Plot.XAxis.DateTimeFormat(true);
            var legend = _view.formsLEDStatusPlot.Plot.Legend(location: ScottPlot.Alignment.UpperRight);
            _view.formsLEDStatusPlot.Refresh();

            updateLEDStatusTimer = new System.Threading.Timer(this.UpdateLEDLiveData, 0, 0, 1000);
            renderLEDStatusTimer = new System.Threading.Timer(this.RenderLEDStatus, sig, 0, 3600 * 1000);
        }

        public void OnConnectSerialPort(object sender, EventArgs e)
        {
            // Start serial port connection
            ComboBox cbxCOMPort = (ComboBox)(this._view.Controls.Find("cbxCOMPort", true)[0]);
            ComboBox cbxBaudRate = (ComboBox)(this._view.Controls.Find("cbxBaudRate", true)[0]);
            ComboBox cbxCheckBit = (ComboBox)(this._view.Controls.Find("cbxCheckBit", true)[0]);
            ComboBox cbxDataBit = (ComboBox)(this._view.Controls.Find("cbxDataBit", true)[0]);
            ComboBox cbxStopBit = (ComboBox)(this._view.Controls.Find("cbxStopBit", true)[0]);
            connector = new LEDBoardCom(
                cbxCOMPort.SelectedItem.ToString(),
                cbxBaudRate.SelectedItem.ToString(),
                cbxCheckBit.SelectedItem.ToString(),
                cbxDataBit.SelectedItem.ToString(),
                cbxStopBit.SelectedItem.ToString()
            );

            RadioButton rbnSendHEX = (RadioButton)(this._view.Controls.Find("rbnSendHEX", true)[0]);
            RadioButton rbnSendASCII = (RadioButton)(this._view.Controls.Find("rbnSendASCII", true)[0]);
            RadioButton rbnRecHEX = (RadioButton)(this._view.Controls.Find("rbnRecHEX", true)[0]);
            RadioButton rbnRecASCII = (RadioButton)(this._view.Controls.Find("rbnRecASCII", true)[0]);
            TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
            Button btnOpenCOM = (Button)(this._view.Controls.Find("btnOpenCOM", true)[0]);
            Button btnCloseCOM = (Button)(this._view.Controls.Find("btnCloseCOM", true)[0]);
            Button btnSendTestMsg = (Button)(this._view.Controls.Find("btnSendTestMsg", true)[0]);

            try
            {
                connector.Connect(2000);
                ShowSendStatusAsync();
                _view.toolStripConnectionStatusText = "连接成功";

                // show connection message
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + " 串口连接成功\r\n");
                btnOpenCOM.BackColor = Color.Green;
                btnCloseCOM.BackColor = Color.Empty;

                // Disable serial port configuration
                cbxCOMPort.Enabled = false;
                cbxBaudRate.Enabled = false;
                cbxCheckBit.Enabled = false;
                cbxDataBit.Enabled = false;
                cbxStopBit.Enabled = false;
                rbnSendHEX.Enabled = false;
                rbnSendASCII.Enabled = false;
                rbnRecHEX.Enabled = false;
                rbnRecASCII.Enabled = false;
                btnSendTestMsg.Enabled = true;

                if (!recWorker.IsBusy)
                {
                    recWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                _view.toolStripConnectionStatusText = "连接失败";
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + " 串口连接失败\r\n");
                tbxConnectMsg.AppendText(ex.ToString() + "\r\n");
                btnOpenCOM.BackColor = Color.Empty;
            }
        }

        public void OnCloseSerialPort(object sender, EventArgs e)
        {
            ComboBox cbxCOMPort = (ComboBox)(this._view.Controls.Find("cbxCOMPort", true)[0]);
            ComboBox cbxBaudRate = (ComboBox)(this._view.Controls.Find("cbxBaudRate", true)[0]);
            ComboBox cbxCheckBit = (ComboBox)(this._view.Controls.Find("cbxCheckBit", true)[0]);
            ComboBox cbxDataBit = (ComboBox)(this._view.Controls.Find("cbxDataBit", true)[0]);
            ComboBox cbxStopBit = (ComboBox)(this._view.Controls.Find("cbxStopBit", true)[0]);
            RadioButton rbnSendHEX = (RadioButton)(this._view.Controls.Find("rbnSendHEX", true)[0]);
            RadioButton rbnSendASCII = (RadioButton)(this._view.Controls.Find("rbnSendASCII", true)[0]);
            RadioButton rbnRecHEX = (RadioButton)(this._view.Controls.Find("rbnRecHEX", true)[0]);
            RadioButton rbnRecASCII = (RadioButton)(this._view.Controls.Find("rbnRecASCII", true)[0]);
            TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
            Button btnOpenCOM = (Button)(this._view.Controls.Find("btnOpenCOM", true)[0]);
            Button btnCloseCOM = (Button)(this._view.Controls.Find("btnCloseCOM", true)[0]);
            Button btnSendTestMsg = (Button)(this._view.Controls.Find("btnSendTestMsg", true)[0]);

            // Close connection
            try
            {
                connector.Disconnect();
                recWorker.CancelAsync();
                ShowSendStatusAsync();
                _view.toolStripConnectionStatusText = "断开成功";
                // show disconnect message
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + " 串口断开成功\r\n");
                btnCloseCOM.BackColor = Color.Empty;
                btnOpenCOM.BackColor = Color.Empty;

                // Enable serial port configuration
                cbxCOMPort.Enabled = true;
                cbxBaudRate.Enabled = true;
                cbxCheckBit.Enabled = true;
                cbxDataBit.Enabled = true;
                cbxStopBit.Enabled = true;
                rbnSendHEX.Enabled = true;
                rbnSendASCII.Enabled = true;
                rbnRecHEX.Enabled = true;
                rbnRecASCII.Enabled = true;
                btnSendTestMsg.Enabled = false;
            }
            catch (Exception ex)
            {
                _view.toolStripConnectionStatusText = "断开失败";
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + " 串口断开失败\r\n");
                tbxConnectMsg.AppendText(ex.ToString() + "\r\n");
                btnCloseCOM.BackColor = Color.Red;
            }
        }

        public void InitialForm()
        {
            // Serial COM Ports Initialization
            ComboBox cbxCOMPort = (ComboBox)(this._view.Controls.Find("cbxCOMPort", true)[0]);
            string[] strCOMPorts = SerialPort.GetPortNames();
            if (strCOMPorts == null)
            {
                MessageBox.Show("本机没有串口!", "Error");
                return;
            }
            foreach (string strCOMPort in strCOMPorts)
            {
                cbxCOMPort.Items.Add(strCOMPort);
            }
            cbxCOMPort.SelectedIndex = 0;

            // Baud Rate Initialization
            ComboBox cbxBaudRate = (ComboBox)(this._view.Controls.Find("cbxBaudRate", true)[0]);
            string[] baudRates = {"9600", "19200", "38400", "57600", "115200"};
            foreach (string baudRate in baudRates)
            {
                cbxBaudRate.Items.Add(baudRate);
            }
            cbxBaudRate.SelectedIndex = 4;

            // Check Bit Initialization
            ComboBox cbxCheckBit = (ComboBox)(this._view.Controls.Find("cbxCheckBit", true)[0]);
            string[] checkBits = {"None", "Even", "Odd", "Mask", "Space"};
            foreach (string checkBit in checkBits)
            {
                cbxCheckBit.Items.Add(checkBit);
            }
            cbxCheckBit.SelectedIndex = 0;

            // Data Bit Initialization
            ComboBox cbxDataBit = (ComboBox)(this._view.Controls.Find("cbxDataBit", true)[0]);
            string[] dataBits = {"5", "6", "7", "8"};
            foreach (string dataBit in dataBits)
            {
                cbxDataBit.Items.Add(dataBit);
            }
            cbxDataBit.SelectedIndex = 3;

            // Stop Bit Initialization
            ComboBox cbxStopBit = (ComboBox)(this._view.Controls.Find("cbxStopBit", true)[0]);
            string[] stopBits = {"1", "1.5", "2"};
            foreach (string stopBit in stopBits)
            {
                cbxStopBit.Items.Add(stopBit);
            }
            cbxStopBit.SelectedIndex = 0;

            RadioButton rbnSendASCII = (RadioButton)(this._view.Controls.Find("rbnSendASCII", true)[0]);
            RadioButton rbnRecASCII = (RadioButton)(this._view.Controls.Find("rbnRecASCII", true)[0]);
            rbnSendASCII.Checked = true;
            rbnRecASCII.Checked = true;

            // Initialize LED min/max values
            Label lblGreenLEDMaxLeft = (Label)(this._view.Controls.Find("lblGreenLEDMaxLeft", true)[0]);
            Label lblGreenLEDMinLeft = (Label)(this._view.Controls.Find("lblGreenLEDMinLeft", true)[0]);
            Label lblGreenLEDMinRight = (Label)(this._view.Controls.Find("lblGreenLEDMinRight", true)[0]);
            Label lblGreenLEDMaxRight = (Label)(this._view.Controls.Find("lblGreenLEDMaxRight", true)[0]);
            Label lblRedLEDMaxLeft = (Label)(this._view.Controls.Find("lblRedLEDMaxLeft", true)[0]);
            Label lblRedLEDMinLeft = (Label)(this._view.Controls.Find("lblRedLEDMinLeft", true)[0]);
            Label lblRedLEDMinRight = (Label)(this._view.Controls.Find("lblRedLEDMinRight", true)[0]);
            Label lblRedLEDMaxRight = (Label)(this._view.Controls.Find("lblRedLEDMaxRight", true)[0]);
            Label lblDarkRedLEDMaxLeft = (Label)(this._view.Controls.Find("lblDarkRedLEDMaxLeft", true)[0]);
            Label lblDarkRedLEDMinLeft = (Label)(this._view.Controls.Find("lblDarkRedLEDMinLeft", true)[0]);
            Label lblDarkRedLEDMinRight = (Label)(this._view.Controls.Find("lblDarkRedLEDMinRight", true)[0]);
            Label lblDarkRedLEDMaxRight = (Label)(this._view.Controls.Find("lblDarkRedLEDMaxRight", true)[0]);
            lblGreenLEDMaxLeft.Text = Convert.ToString(MaxGreenLEDPower);
            lblGreenLEDMinLeft.Text = Convert.ToString(MinGreenLEDPower);
            lblGreenLEDMaxRight.Text = Convert.ToString(MaxGreenLEDPower);
            lblGreenLEDMinRight.Text = Convert.ToString(MinGreenLEDPower);
            lblRedLEDMaxLeft.Text = Convert.ToString(MaxRedLEDPower);
            lblRedLEDMinLeft.Text = Convert.ToString(MinRedLEDPower);
            lblRedLEDMaxRight.Text = Convert.ToString(MaxRedLEDPower);
            lblRedLEDMinRight.Text = Convert.ToString(MinRedLEDPower);
            lblDarkRedLEDMaxLeft.Text = Convert.ToString(MaxDarkRedLEDPower);
            lblDarkRedLEDMinLeft.Text = Convert.ToString(MinDarkRedLEDPower);
            lblDarkRedLEDMaxRight.Text = Convert.ToString(MaxDarkRedLEDPower);
            lblDarkRedLEDMinRight.Text = Convert.ToString(MinDarkRedLEDPower);

            Button btnSendTestMsg = (Button)(this._view.Controls.Find("btnSendTestMsg", true)[0]);
            btnSendTestMsg.Enabled = false;
        }

        public LEDControllerCfg GetUISettings(LEDControllerViewer thisView)
        {
            LEDControllerCfg LEDCfgWriter = new LEDControllerCfg();

            TextBox tbxIP = (TextBox)(_view.Controls.Find("tbxIP", true)[0]);
            LEDCfgWriter.slaveIP = tbxIP.Text;
            TextBox tbxPort = (TextBox)(_view.Controls.Find("tbxPort", true)[0]);
            LEDCfgWriter.slavePort = tbxPort.Text;

            for (int i = 0; i < LEDBoardCom.NumGreenFixLED; i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(_view.Controls.Find($"btnLED{LEDIndex}", true)[0]);

                LEDCfgWriter.isGreenFixLEDOn[i] = (btn.BackColor == Color.Green);
            }

            for (int i = 0; i < LEDBoardCom.NumRedFixLED; i++)
            {
                int LEDIndex = i + 1 + LEDBoardCom.NumGreenFixLED;
                Button btn = (Button)(_view.Controls.Find($"btnLED{LEDIndex}", true)[0]);

                LEDCfgWriter.isRedFixLEDOn[i] = (btn.BackColor == Color.Red);
            }

            for (int i = 0; i < LEDBoardCom.NumDarkRedFixLED; i++)
            {
                int LEDIndex = i + 1 + LEDBoardCom.NumGreenFixLED + LEDBoardCom.NumRedFixLED;
                Button btn = (Button)(_view.Controls.Find($"btnLED{LEDIndex}", true)[0]);

                LEDCfgWriter.isDarkRedFixLEDOn[i] = (btn.BackColor == Color.DarkRed);
            }

            for (int i = 0; i < LEDBoardCom.NumGreenDimLED; i++)
            {
                int LEDIndex = i + 1;
                TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimLED{LEDIndex}", true)[0]);

                LEDCfgWriter.greenDimLEDPower[i] = Convert.ToDouble(tbx.Text);
            }

            for (int i = 0; i < LEDBoardCom.NumRedDimLED; i++)
            {
                int LEDIndex = i + 1 + LEDBoardCom.NumGreenDimLED;
                TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimLED{LEDIndex}", true)[0]);

                LEDCfgWriter.redDimLEDPower[i] = Convert.ToDouble(tbx.Text);
            }

            for (int i = 0; i < LEDBoardCom.NumDarkRedDimLED; i++)
            {
                int LEDIndex = i + 1 + LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumDarkRedDimLED;
                TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimLED{LEDIndex}", true)[0]);

                LEDCfgWriter.darkRedDimLEDPower[i] = Convert.ToDouble(tbx.Text);
            }

            return LEDCfgWriter;
        }

        public void OnShowVersion(object sender, EventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var companyName = fvi.CompanyName;
            var productName = fvi.ProductName;
            var productVersion = fvi.ProductVersion;

            MessageBox.Show(String.Format("Product Name: {0}\nVersion: {1}\nCompany: {2}", productName, productVersion, companyName));
        }

        public void OnSaveCfgFile(object sender, EventArgs e)
        {
            if (!File.Exists(cfgFileName))
            {
                SaveFileDialog saveFileDlg = new SaveFileDialog();
                saveFileDlg.Filter = "LEDController Configuration file|*.ini";
                saveFileDlg.Title = "Save LEDController Configuration file";
                saveFileDlg.CheckFileExists = true;
                saveFileDlg.RestoreDirectory = true;

                if (saveFileDlg.FileName != "")
                {
                    cfgFileName = saveFileDlg.FileName;
                }
            }

            LEDControllerCfg LEDCfgWriter = GetUISettings(_view);
            LEDCfgWriter.LEDControllerCfgSave(cfgFileName);

            MessageBox.Show("保存配置文件成功!");
        }

        public void OnSaveasCfgFile(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDlg = new SaveFileDialog();
            saveFileDlg.Filter = "LEDController Configuration file|*.ini";
            saveFileDlg.Title = "Saveas LEDController Configuration file...";
            saveFileDlg.CheckFileExists = true;
            saveFileDlg.RestoreDirectory = true;

            if (saveFileDlg.FileName != "")
            {
                cfgFileName = saveFileDlg.FileName;
            }

            LEDControllerCfg LEDCfgWriter = GetUISettings(_view);
            LEDCfgWriter.LEDControllerCfgSave(cfgFileName);

            MessageBox.Show("保存配置文件成功!");
        }

        private void OnOpenWithCfgFile(object sender, EventArgs e)
        {
            // Choose a configration file
            OpenFileDialog openFileDlg = new OpenFileDialog();

            openFileDlg.InitialDirectory = "c:\\";
            openFileDlg.Filter = "LEDController Configuration files (*.ini)|*.ini";
            openFileDlg.FilterIndex = 0;
            openFileDlg.RestoreDirectory = true;

            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                LEDControllerCfg LEDCfgReader = new LEDControllerCfg(openFileDlg.FileName);
                cfgFileName = openFileDlg.FileName;

                if (connector != null) connector.Disconnect();

                // Show IP and Port
                TextBox slaveIPTbx = (TextBox)(_view.Controls.Find("tbxIP", true)[0]);
                slaveIPTbx.Text = LEDCfgReader.slaveIP;
                TextBox slavePortTbx = (TextBox)(_view.Controls.Find("tbxPort", true)[0]);

                // Show Serial Port Configurations
                ComboBox cbxCOMPort = (ComboBox)(this._view.Controls.Find("cbxCOMPort", true)[0]);
                ComboBox cbxBaudRate = (ComboBox)(this._view.Controls.Find("cbxBaudRate", true)[0]);
                ComboBox cbxCheckBit = (ComboBox)(this._view.Controls.Find("cbxCheckBit", true)[0]);
                ComboBox cbxDataBit = (ComboBox)(this._view.Controls.Find("cbxDataBit", true)[0]);
                ComboBox cbxStopBit = (ComboBox)(this._view.Controls.Find("cbxStopBit", true)[0]);
                RadioButton rbnSendASCII = (RadioButton)(this._view.Controls.Find("rbnSendASCII", true)[0]);
                RadioButton rbnRecASCII = (RadioButton)(this._view.Controls.Find("rbnRecASCII", true)[0]);

                cbxCOMPort.SelectedItem = LEDCfgReader.serialName;
                cbxBaudRate.SelectedItem = LEDCfgReader.baudRate;
                cbxCheckBit.SelectedItem = LEDCfgReader.checkBit;
                cbxDataBit.SelectedItem = LEDCfgReader.dataBit;
                cbxStopBit.SelectedItem = LEDCfgReader.stopBit;
                rbnSendASCII.Checked = LEDCfgReader.isSendASCII;
                rbnRecASCII.Checked = LEDCfgReader.isRecASCII;

                // Start connection
                TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
                try
                {
                    if (LEDCfgReader.isTCP)
                    {
                        OnConnectTCP(sender, e);
                    }
                    else if (LEDCfgReader.isSerialPort)
                    {
                        OnConnectSerialPort(sender, e);
                    }
                }
                catch (Exception ex)
                {
                    tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
                    return;
                }

                // Turn on Fix LEDs
                for (int i = 0; i < LEDCfgReader.isGreenFixLEDOn.Length; i++)
                {
                    int LEDIndex = i + 1;
                    EventLEDArgs myEvent = new EventLEDArgs(LEDIndex, 3);
                    Button btn = (Button)(_view.Controls.Find($"btnLED{LEDIndex}", true)[0]);

                    if (LEDCfgReader.isGreenFixLEDOn[i])
                    {
                        OnOpenFixLED(btn, myEvent);
                    }
                    else
                    {
                        OnCloseFixLED(btn, myEvent);
                    }
                }

                for (int i = 0; i < LEDCfgReader.isRedFixLEDOn.Length; i++)
                {
                    int LEDIndex = i + LEDBoardCom.NumGreenFixLED + 1;
                    EventLEDArgs myEvent = new EventLEDArgs(LEDIndex, 1);
                    Button btn = (Button)(_view.Controls.Find($"btnLED{LEDIndex}", true)[0]);

                    if (LEDCfgReader.isRedFixLEDOn[i])
                    {
                        OnOpenFixLED(btn, myEvent);
                    }
                    else
                    {
                        OnCloseFixLED(btn, myEvent);
                    }
                }

                for (int i = 0; i < LEDCfgReader.isDarkRedFixLEDOn.Length; i++)
                {
                    int LEDIndex = i + LEDBoardCom.NumGreenFixLED + LEDBoardCom.NumRedFixLED + 1;
                    EventLEDArgs myEvent = new EventLEDArgs(LEDIndex, 2);
                    Button btn = (Button)(_view.Controls.Find($"btnLED{LEDIndex}", true)[0]);

                    if (LEDCfgReader.isDarkRedFixLEDOn[i])
                    {
                        OnOpenFixLED(btn, myEvent);
                    }
                    else
                    {
                        OnCloseFixLED(btn, myEvent);
                    }
                }

                for (int i = 0; i < LEDCfgReader.greenDimLEDPower.Length; i++)
                {
                    int dimLEDIndex = i + 1;
                    int LEDIndex = dimLEDIndex;
                    int LEDPowerBit = (Int16)((double)CalcSbarValue(LEDCfgReader.greenDimLEDPower[i], LEDIndex, 3) / (double)NumScrollBarLevel * 255);

                    // configure dimmable LED textbox and scrollbar
                    TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimLED{dimLEDIndex}", true)[0]);
                    tbx.Text = Convert.ToString(LEDCfgReader.greenDimLEDPower[i]);

                    TrackBar tbar = (TrackBar)(_view.Controls.Find($"sbarDimLED{dimLEDIndex}", true)[0]);
                    int sbarVal = CalcSbarValue(LEDCfgReader.greenDimLEDPower[i], dimLEDIndex, 3);
                    tbar.Value = sbarVal;

                    // turn on or turn off the LED button
                    EventDimLEDArgs myEvent = new EventDimLEDArgs(LEDIndex, 3, LEDCfgReader.greenDimLEDPower[i]);
                    Button btn = (Button)(_view.Controls.Find($"btnDimLED{dimLEDIndex}", true)[0]);

                    OnSetDimLED(btn, myEvent);
                }

                for (int i = 0; i < LEDCfgReader.redDimLEDPower.Length; i++)
                {
                    int dimLEDIndex = i + 1;
                    int LEDIndex = dimLEDIndex;
                    int LEDPowerBit = (Int16)((double)CalcSbarValue(LEDCfgReader.redDimLEDPower[i], LEDIndex, 1) / (double)NumScrollBarLevel * 255);

                    // configure dimmable LED textbox and scrollbar
                    TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimLED{dimLEDIndex}", true)[0]);
                    tbx.Text = Convert.ToString(LEDCfgReader.redDimLEDPower[i]);

                    TrackBar tbar = (TrackBar)(_view.Controls.Find($"sbarDimLED{dimLEDIndex}", true)[0]);
                    int sbarVal = CalcSbarValue(LEDCfgReader.redDimLEDPower[i], dimLEDIndex, 1);
                    tbar.Value = sbarVal;

                    // turn on or turn off the LED button
                    EventDimLEDArgs myEvent = new EventDimLEDArgs(LEDIndex, 1, LEDCfgReader.redDimLEDPower[i]);
                    Button btn = (Button)(_view.Controls.Find($"btnDimLED{dimLEDIndex}", true)[0]);

                    OnSetDimLED(btn, myEvent);
                }

                for (int i = 0; i < LEDCfgReader.darkRedDimLEDPower.Length; i++)
                {
                    int dimLEDIndex = i + 1;
                    int LEDIndex = dimLEDIndex;
                    int LEDPowerBit = (Int16)((double)CalcSbarValue(LEDCfgReader.darkRedDimLEDPower[i], LEDIndex, 2) / (double)NumScrollBarLevel * 255);

                    // configure dimmable LED textbox and scrollbar
                    TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimLED{dimLEDIndex}", true)[0]);
                    tbx.Text = Convert.ToString(LEDCfgReader.darkRedDimLEDPower[i]);

                    TrackBar tbar = (TrackBar)(_view.Controls.Find($"sbarDimLED{dimLEDIndex}", true)[0]);
                    int sbarVal = CalcSbarValue(LEDCfgReader.darkRedDimLEDPower[i], dimLEDIndex, 2);
                    tbar.Value = sbarVal;

                    // turn on or turn off the LED button
                    EventDimLEDArgs myEvent = new EventDimLEDArgs(LEDIndex, 2, LEDCfgReader.darkRedDimLEDPower[i]);
                    Button btn = (Button)(_view.Controls.Find($"btnDimLED{dimLEDIndex}", true)[0]);

                    OnSetDimLED(btn, myEvent);
                }
            }
        }

        private void RenderLEDStatus(object state)
        {
            ScottPlot.Plottable.SignalPlot sig = state as ScottPlot.Plottable.SignalPlot;
            sig.OffsetX = GetCurrentTime().ToOADate();
            _view.formsLEDStatusPlot.Plot.SetAxisLimitsX(GetCurrentTime().ToOADate(), GetCurrentTime().AddHours(1.0).ToOADate());
            _view.formsLEDStatusPlot.Refresh();
        }

        private void UpdateLEDLiveData(object state)
        {
            int thisIndex = (int)(sw.Elapsed.TotalSeconds) % 3600;
            LEDPowerLiveData[thisIndex] = 1;
            LEDVoltageLiveData[thisIndex] = 1;
            LEDCurrentLiveData[thisIndex] = 1;
            /* // Replace the upper part with below 3 lines
            LEDPowerLiveData[thisIndex] = (this.currentLEDStatus.fixLEDPower + this.currentLEDStatus.dimLEDPower);
            LEDVoltageLiveData[thisIndex] = (this.currentLEDStatus.fixLEDVoltage + this.currentLEDStatus.dimLEDVoltage);
            LEDCurrentLiveData[thisIndex] = (this.currentLEDStatus.fixLEDCurrent + this.currentLEDStatus.dimLEDCurrent);
            */

            _view.formsLEDStatusPlot.Refresh();
        }

        private void OnConnectionBreak(object sender, RunWorkerCompletedEventArgs args)
        {
            recWorker.CancelAsync();
        }

        private void OnShowLEDStatus(object sender, EventArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Show LED status
            if ((connector != null) && (receiveBytes != null))
            {
                try
                {
                    // parsing status
                    LEDStatus thisLEDStatus = connector.ParsePackage(receiveBytes);
                    TextBox tbxMinValue = (TextBox)(this._view.Controls.Find("tbxMinValue", true)[0]);
                    TextBox tbxMaxValue = (TextBox)(this._view.Controls.Find("tbxMaxValue", true)[0]);
                    double minValue = Convert.ToDouble(tbxMinValue.Text);
                    double maxValue = Convert.ToDouble(tbxMaxValue.Text);

                    ComboBox cbxQueryParam = (ComboBox)(this._view.Controls.Find("cbxQueryParam", true)[0]);
                    int queryIndex = cbxQueryParam.SelectedIndex;

                    if (thisLEDStatus.isValidPackage)
                    {
                        // Convert values to color
                        double[] LEDPower = Enumerable.Concat(thisLEDStatus.fixLEDPower, thisLEDStatus.dimLEDPower).ToArray();
                        double[] LEDVoltage = Enumerable.Concat(thisLEDStatus.fixLEDVoltage, thisLEDStatus.dimLEDVoltage).ToArray();
                        double[] LEDCurrent = Enumerable.Concat(thisLEDStatus.fixLEDCurrent, thisLEDStatus.dimLEDCurrent).ToArray();
                        Color[] LEDControlColors = new Color[132];
                        for (int i = 0; i < 132; i++)
                        {
                            double LEDValue, normLEDValue;
                            if (queryIndex == 1)
                                LEDValue = LEDCurrent[i];
                            else if (queryIndex == 2)
                                LEDValue = LEDVoltage[i];
                            else if (queryIndex == 3)
                                LEDValue = LEDPower[i];
                            else
                                LEDValue = 0.0;

                            normLEDValue = (LEDValue - minValue) / (maxValue - minValue);
                            // specify LED status according to the showing message type
                            LEDControlColors[i] = ColorConv.HSL2RGB(normLEDValue, 0.5, 0.5);
                        }

                        // Set Colors
                        // _view.LEDStatusColors = LEDControlColors;
                    }
                }
                catch
                {
                    // show status
                    _view.toolStripLEDStatusText = "获取LED状态失败";
                }
            }
        }

        private void OnHandleDimLED(object sender, EventDimLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn on/off Dimmable LED Button
            TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
            try
            {
                Button btn = sender as Button;
                if (btn.BackColor.ToArgb().Equals(Color.Gray.ToArgb()))
                {
                    // Send control cmd
                    OnSetDimLED(sender, e);
                }
                else
                {
                    // Send control cmd
                    OnCloseDimLED(sender, e);
                }
            }
            catch (Exception ex)
            {
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }
        }

        private void OnHandleFixLED(object sender, EventLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn on/off Fix LED Button
            TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
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
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }
        }

        private void OnUpdateLEDTbx(object sender, EventDimLEDArgs e)
        {
            // Update LED Power setting text box
            TrackBar tbar = sender as TrackBar;

            double dimLEDPower = CalcDimLEDPower(e.LEDPower, e.LEDIndex, e.addrPLC);
            string LEDTbxText = Convert.ToString(dimLEDPower);

            TextBox tbx;
            switch (e.addrPLC)
            {
                case 1:
                    tbx = (TextBox)(this._view.Controls.Find($"tbxDimRedLED{e.LEDIndex}", true)[0]);
                    tbx.Text = LEDTbxText;
                    break;

                case 2:
                    tbx = (TextBox)(this._view.Controls.Find($"tbxDimDarkRedLED{e.LEDIndex}", true)[0]);
                    tbx.Text = LEDTbxText;
                    break;

                case 3:
                    tbx = (TextBox)(this._view.Controls.Find($"tbxDimGreenLED{e.LEDIndex}", true)[0]);
                    tbx.Text = LEDTbxText;
                    break;
            }
        }

        private double CalcDimLEDPower(double sbarValue, int LEDIndex, int addrPLC)
        {
            double LEDPower = 0.0;

            switch (addrPLC)
            {
                case 1:
                    // Red LED
                    LEDPower = (sbarValue - 0) / NumScrollBarLevel * (MaxRedLEDPower - MinRedLEDPower) + MinRedLEDPower;
                    break;

                case 2:
                    // DarkRed LED
                    LEDPower = (sbarValue - 0) / NumScrollBarLevel * (MaxDarkRedLEDPower - MinDarkRedLEDPower) + MinDarkRedLEDPower;
                    break;

                case 3:
                    // Green LED
                    LEDPower = (sbarValue - 0) / NumScrollBarLevel * (MaxGreenLEDPower - MinGreenLEDPower) + MinGreenLEDPower;
                    break;
            }

            return LEDPower;
        }

        private void OnUpdateScrollBar(object sender, EventDimLEDArgs e)
        {
            // Convert string to scroll bar value
            TextBox tbx = sender as TextBox;

            int sbarValue = CalcSbarValue(e.LEDPower, e.LEDIndex, e.addrPLC);
            TrackBar tbar;
            switch (e.addrPLC)
            {
                case 1:
                    tbar = (TrackBar)(this._view.Controls.Find($"sbarDimRedLED{e.addrPLC}", true)[0]);
                    tbar.Value = sbarValue;
                    break;

                case 2:
                    tbar = (TrackBar)(this._view.Controls.Find($"sbarDimDarkRedLED{e.addrPLC}", true)[0]);
                    tbar.Value = sbarValue;
                    break;

                case 3:
                    tbar = (TrackBar)(this._view.Controls.Find($"sbarDimGreenLED{e.addrPLC}", true)[0]);
                    tbar.Value = sbarValue;
                    break;
            }
        }

        private int CalcSbarValue(double LEDPower, int LEDIndex, int addrPLC)
        {
            // Calculate scroll bar value for Dimmable LED
            int sbarValue = 0;

            switch (addrPLC)
            {
                case 1:
                    // Red LED
                    if ((LEDPower < MinRedLEDPower) || (LEDPower > MaxRedLEDPower))
                    {
                        MessageBox.Show($"红光设定功率超出范围 (设定值:{LEDPower})", "警告");
                        return sbarValue;
                    }
                    else
                    {
                        sbarValue = Convert.ToInt32((LEDPower - MinRedLEDPower) / (MaxRedLEDPower - MinRedLEDPower) * NumScrollBarLevel);
                    }
                    break;

                case 2:
                    // DarkRed LED
                    if ((LEDPower < MinDarkRedLEDPower) || (LEDPower > MaxDarkRedLEDPower))
                    {
                        MessageBox.Show($"红外设定功率超出范围 (设定值:{LEDPower})", "警告");
                        return sbarValue;
                    }
                    else
                    {
                        sbarValue = Convert.ToInt32((LEDPower - MinDarkRedLEDPower) / (MaxDarkRedLEDPower - MinDarkRedLEDPower) * NumScrollBarLevel);
                    }
                    break;

                case 3:
                    // Green LED
                    if ((LEDPower < MinGreenLEDPower) || (LEDPower > MaxGreenLEDPower))
                    {
                        MessageBox.Show($"绿光LED设定功率超出范围 (设定值:{LEDPower})", "警告");
                        return sbarValue;
                    }
                    else
                    {
                        sbarValue = Convert.ToInt32((LEDPower - MinGreenLEDPower) / (MaxGreenLEDPower - MinGreenLEDPower) * NumScrollBarLevel);
                    }
                    break;

                default:
                    sbarValue = 0;
                    break;
            }

            return sbarValue;
        }

        private void OnSetDimLED(object sender, EventDimLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn on Dimmable LED
            int LEDPowerBit = (Int16)(CalcSbarValue(e.LEDPower, e.LEDIndex, e.addrPLC) / (double)NumScrollBarLevel * 255);   // Convert to 0-255
            try
            {
                connector.SetDimLED(e.addrPLC, e.LEDIndex, LEDPowerBit);
            }
            catch (Exception ex)
            {
                TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }

            ShowSendStatusAsync();

            Button btn = null;
            Color btnColor = Color.Gray;
            switch (e.addrPLC)
            {
                case 1:
                    btn = (Button)(this._view.Controls.Find($"btnDimRedLED{e.LEDIndex}", true)[0]);
                    btnColor = Color.Red;
                    break;

                case 2:
                    btn = (Button)(this._view.Controls.Find($"btnDimDarkRedLED{e.LEDIndex}", true)[0]);
                    btnColor = Color.DarkRed;
                    break;

                case 3:
                    btn = (Button)(this._view.Controls.Find($"btnDimGreenLED{e.LEDIndex}", true)[0]);
                    btnColor = Color.Green;
                    break;
            }

            if (LEDPowerBit == 0)
            {
                btn.BackColor = Color.Gray;
            }
            else
            {
                btn.BackColor = btnColor;
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
                connector.SetDimLED(e.addrPLC, e.LEDIndex, 0);
            }
            catch (Exception ex)
            {
                TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }

            ShowSendStatusAsync();

            // Change color
            Button btn = sender as Button;
            btn.BackColor = Color.Gray;
            btn.Refresh();
        }

        private void OnOpenFixLED(object sender, EventLEDArgs e)
        {
            if (connector.isAlive)
            {
                // Turn on Fix LED
                connector.TurnOnFixLED(e.LEDIndex);
                ShowSendStatusAsync();

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

        private void OnCloseFixLED(object sender, EventLEDArgs e)
        {
            // Turn off Fix LED
            if (connector.isAlive)
            {
                connector.TurnOffFixLED(e.LEDIndex);

                ShowSendStatusAsync();

                // Change color
                Button btn = sender as Button;
                btn.BackColor = Color.Gray;
                btn.Refresh();
            }
        }

        private void OnShowSingleLEDStatus(object sender, EventLEDArgs e)
        {
            // Show LED status

            if ((connector != null) && (receiveBytes != null))
            {
                try
                {
                    // parsing status
                    LEDStatus thisLEDStatus;
                    thisLEDStatus = connector.ParsePackage(receiveBytes);

                    if (thisLEDStatus.isValidPackage)
                    {
                        // show status
                        // determine whether it is fix led or dim led
                        // LED power, current, voltage, temperature ... in the bottom status bar
                    }
                    else
                    {
                        // show status
                        _view.toolStripLEDStatusText = "获取LED状态失败";
                    }
                }
                catch
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

        private void OnClearSingleLEDStatus(object sender, EventLEDArgs e)
        {
            // Clear LED status
            _view.toolStripLEDStatusText = "LED状态";
        }

        private void OnSendTestData(object sender, EventArgs e)
        {
            TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
            if (!connector.isAlive)
            {
                return;
            }

            // Send test data
            try
            {
                TextBox tbxTestCmd = (TextBox)(this._view.Controls.Find("tbxTestCmd", true)[0]);
                RadioButton rbnSendHEX = (RadioButton)(this._view.Controls.Find("rbnSendHEX", true)[0]);
                connector.SendCmd(tbxTestCmd.Text, rbnSendHEX.Checked);
                ShowSendStatusAsync();
            }
            catch (Exception ex)
            {
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }
        }

        public void ReceiveMsg(object sender, DoWorkEventArgs args)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (true)   // Receiving message from slave
            {
                int msgLen = 0;
                byte[] buffer = new byte[SendBufferSize];
                if ((this.connector.isAlive) && (this.connector.isTCP) && (connector.socketHost != null) && (!worker.CancellationPending))
                {
                    msgLen = connector.socketHost.Receive(buffer,0, buffer.Length, SocketFlags.None);

                    //数据接收不完整，故再次判断
                    Thread.Sleep(50);
                    while (connector.socketHost.Available > 0)
                    {
                        msgLen += connector.socketHost.Receive(buffer, msgLen, buffer.Length - msgLen, SocketFlags.None);

                        Thread.Sleep(50);
                    }
                }
                else if ((this.connector.isAlive) && (this.connector.isSerialPort) && (!worker.CancellationPending))
                {
                    RadioButton rbnRecHEX = (RadioButton)(this._view.Controls.Find("rbnRecHEX", true)[0]);
                    if (rbnRecHEX.Checked)
                    {
                        
                        byte[] recData = new byte[this.connector.serialPort.BytesToRead];
                        this.connector.serialPort.Read(recData, 0, recData.Length);
                        msgLen = recData.Length;
                        int value;
                        for (int i = 0; i < msgLen; i++)
                        {
                            value = Convert.ToInt32(recData[i]);
                            buffer[i] = Convert.ToByte(String.Format("{0:X}", value));
                        }
                    }
                    else
                    {
                        byte[] recData = new byte[this.connector.serialPort.BytesToRead];
                        this.connector.serialPort.Read(recData, 0, recData.Length);

                        msgLen = recData.Length;
                        for (int i = 0; i < msgLen; i++)
                        {
                            buffer[i] = recData[i];
                        }
                    }

                    this.connector.serialPort.DiscardInBuffer();
                }
                else
                {
                    args.Cancel = true;
                    break;
                }

                if (msgLen > 0)
                {
                    try
                    {
                        currentLEDStatus = connector.ParsePackage(buffer);
                        receiveBytes = buffer;
                        // TODO add timestamp

                        // showing receiving status with colorful light
                        worker.ReportProgress(1);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private async void ShowSendStatusAsync()
        {

            // Sync
            _view.btnSendStatus1Color = Color.Green;
            _view.btnSendStatus2Color = Color.Green;
            _view.btnSendStatus3Color = Color.Green;
            await Task.Delay(50);
            _view.btnSendStatus1Color = Color.DarkRed;
            _view.btnSendStatus2Color = Color.DarkRed;
            _view.btnSendStatus3Color = Color.DarkRed;

        }

        private async void ShowReceiveStatusAsync(object sender, ProgressChangedEventArgs args)
        {
            // format recMsg
            string recMsg = Encoding.UTF8.GetString(receiveBytes);
            recMsg = recMsg.Replace("\0", "");
            string formatRecMsg = "[" + GetCurrentTime() + "] " + "\r\n" + recMsg + "\r\n";
            TextBox tbxTestRec = (TextBox)(this._view.Controls.Find("tbxTestRec", true)[0]);
            tbxTestRec.AppendText(formatRecMsg);;

            StatusStrip statusStrip1 = (StatusStrip)(this._view.Controls.Find("statusStrip1", true)[0]);

            // showing total power
            ToolStripStatusLabel tsslGreenLEDTotalPower = statusStrip1.Items[0] as ToolStripStatusLabel;
            ToolStripStatusLabel tsslRedLEDTotalPower = statusStrip1.Items[1] as ToolStripStatusLabel;
            ToolStripStatusLabel tsslDarkRedLEDTotalPower = statusStrip1.Items[2] as ToolStripStatusLabel;
            tsslGreenLEDTotalPower.Text = $"绿光实时总功率: {currentLEDStatus.totalGreenLEDPower}W";
            tsslRedLEDTotalPower.Text = $"红光实时总功率: {currentLEDStatus.totalRedLEDPower}W";
            tsslDarkRedLEDTotalPower.Text = $"红外实时总功率: {currentLEDStatus.totalDarkredLEDPower}W";

            _view.btnRecStatus1Color = Color.Green;
            _view.btnRecStatus2Color = Color.Green;
            _view.btnRecStatus3Color = Color.Green;
            await Task.Delay(50);
            _view.btnRecStatus1Color = Color.DarkRed;
            _view.btnRecStatus2Color = Color.DarkRed;
            _view.btnRecStatus3Color = Color.DarkRed;
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

        private void OnConnectTCP(object sender, EventArgs e)
        {
            // Start TCP connection
            TextBox tbxIP = (TextBox)(this._view.Controls.Find("tbxIP", true)[0]);
            TextBox tbxPort = (TextBox)(this._view.Controls.Find("tbxPort", true)[0]);
            this.connector = new LEDBoardCom(tbxIP.Text, tbxPort.Text);

            TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
            Button btnOpenTCP = (Button)(this._view.Controls.Find("btnOpenTCP", true)[0]);
            Button btnCloseTCP = (Button)(this._view.Controls.Find("btnCloseTCP", true)[0]);
            Button btnSendTestMsg = (Button)(this._view.Controls.Find("btnSendTestMsg", true)[0]);
            try
            {
                connector.device.Connect(2000);
                ShowSendStatusAsync();
                _view.toolStripConnectionStatusText = "连接成功";
                // show connection message
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + " 连接成功\r\n");
                btnOpenTCP.BackColor = Color.Green;
                btnCloseTCP.BackColor = Color.Empty;
                btnSendTestMsg.Enabled = true;

                if (!recWorker.IsBusy)
                {
                    recWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                _view.toolStripConnectionStatusText = "连接失败";
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + " 连接失败\r\n");
                tbxConnectMsg.AppendText(ex.ToString() + "\r\n");
                btnOpenTCP.BackColor = Color.Empty;
            }
        }

        private void OnCloseTCP(object sender, EventArgs e)
        {
            TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
            Button btnOpenTCP = (Button)(this._view.Controls.Find("btnOpenTCP", true)[0]);
            Button btnCloseTCP = (Button)(this._view.Controls.Find("btnCloseTCP", true)[0]);
            Button btnSendTestMsg = (Button)(this._view.Controls.Find("btnSendTestMsg", true)[0]);

            // Close connection
            try
            {
                connector.device.Disconnect();
                recWorker.CancelAsync();
                ShowSendStatusAsync();
                _view.toolStripConnectionStatusText = "断开成功";
                // show disconnect message
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + " 断开成功\r\n");
                btnCloseTCP.BackColor = Color.Empty;
                btnOpenTCP.BackColor = Color.Empty;
                btnSendTestMsg.Enabled = false;
            }
            catch (Exception ex)
            {
                _view.toolStripConnectionStatusText = "断开失败";
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + " 断开失败\r\n");
                tbxConnectMsg.AppendText(ex.ToString() + "\r\n");
                btnCloseTCP.BackColor = Color.Red;
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
