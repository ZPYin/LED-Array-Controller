using System;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using LEDController.View;
using LEDController.Model;
using System.Drawing;
using System.Windows.Threading;
using System.ComponentModel;
using System.Diagnostics;

namespace LEDController.Presenter
{
    class LEDControllerPresenter
    {
        private LEDControllerViewer _view;
        private LEDBoardCom connector = new LEDBoardCom();
        private FileSysIOClass fileIOer;
        private LEDStatus currentLEDStatus;
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
        BackgroundWorker recWorker = new BackgroundWorker();
        private const int NumLiveData = 3600;
        private double[] LEDPowerLiveData = new double[NumLiveData];
        private double[] LEDCurrentLiveData = new double[NumLiveData];
        private double[] LEDVoltageLiveData = new double[NumLiveData];
        public Stopwatch sw = Stopwatch.StartNew();
        public System.Threading.Timer updateLEDStatusTimer;
        public System.Threading.Timer renderLEDStatusTimer;
        public string cfgFileName;

        public LEDControllerPresenter(LEDControllerViewer newView)
        {
            recWorker.WorkerReportsProgress = true;
            recWorker.WorkerSupportsCancellation = true;
            recWorker.DoWork += ReceiveMsg;
            recWorker.ProgressChanged += ShowReceiveStatusAsync;
            recWorker.RunWorkerCompleted += OnConnectionBreak;
            _view = newView;
            _view.Connect += new EventHandler<EventArgs>(OnConnect);
            _view.CloseConnect += new EventHandler<EventArgs>(OnCloseConnect);
            _view.SendTestData += new EventHandler<EventArgs>(OnSendTestData);
            _view.ShowSingleLEDStatus += new EventHandler<EventLEDArgs>(OnShowSingleLEDStatus);
            _view.ClearSingleLEDStatus += new EventHandler<EventLEDArgs>(OnClearSingleLEDStatus);
            _view.OpenFixLED += new EventHandler<EventFixLEDArgs>(OnOpenFixLED);
            _view.CloseFixLED += new EventHandler<EventFixLEDArgs>(OnCloseFixLED);
            _view.HandleFixLED += new EventHandler<EventFixLEDArgs>(OnHandleFixLED);
            _view.SetDimLED += new EventHandler<EventDimLEDArgs>(OnSetDimLED);
            _view.CloseDimLED += new EventHandler<EventDimLEDArgs>(OnCloseDimLED);
            _view.HandleDimLED += new EventHandler<EventDimLEDArgs>(OnHandleDimLED);
            _view.UpdateScrollBar += new EventHandler<EventArgs>(OnUpdateScrollBar);
            _view.UpdateLEDTbx += new EventHandler<EventArgs>(OnUpdateLEDTbx);
            _view.ShowLEDStatus += new EventHandler<EventArgs>(OnShowLEDStatus);
            _view.OpenWithCfgFile += new EventHandler<EventArgs>(OnOpenWithCfgFile);
            _view.SaveCfgFile += new EventHandler<EventArgs>(OnSaveCfgFile);
            _view.SaveasCfgFile += new EventHandler<EventArgs>(OnSaveasCfgFile);
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

            // Initialize LED status plot
            sw.Start();
            Random rand = new Random(0);
            LEDPowerLiveData = ScottPlot.DataGen.RandomWalk(rand, NumLiveData);
            LEDCurrentLiveData = ScottPlot.DataGen.RandomWalk(rand, NumLiveData);
            LEDVoltageLiveData = ScottPlot.DataGen.RandomWalk(rand, NumLiveData);
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

                if (connector != null) connector.Close();

                // Show IP and Port
                TextBox slaveIPTbx = (TextBox)(_view.Controls.Find("tbxIP", true)[0]);
                slaveIPTbx.Text = LEDCfgReader.slaveIP;
                TextBox slavePortTbx = (TextBox)(_view.Controls.Find("tbxPort", true)[0]);

                // Start connection
                try
                {
                    Connect(LEDCfgReader.slaveIP, LEDCfgReader.slavePort);
                }
                catch (Exception ex)
                {
                    _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n";
                    return;
                }

                // Turn on Fix LEDs
                for (int i = 0; i < LEDCfgReader.isGreenFixLEDOn.Length; i++)
                {
                    int LEDIndex = i + 1;
                    EventFixLEDArgs myEvent = new EventFixLEDArgs(LEDIndex);
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
                    EventFixLEDArgs myEvent = new EventFixLEDArgs(LEDIndex);
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
                    EventFixLEDArgs myEvent = new EventFixLEDArgs(LEDIndex);
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
                    int LEDPowerBit = (Int16)((double)CalcSbarValue(LEDCfgReader.greenDimLEDPower[i], LEDIndex) / (double)NumScrollBarLevel * 255);

                    // configure dimmable LED textbox and scrollbar
                    TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimLED{dimLEDIndex}", true)[0]);
                    tbx.Text = Convert.ToString(LEDCfgReader.greenDimLEDPower[i]);

                    TrackBar tbar = (TrackBar)(_view.Controls.Find($"sbarDimLED{dimLEDIndex}", true)[0]);
                    int sbarVal = CalcSbarValue(LEDCfgReader.greenDimLEDPower[i], dimLEDIndex);
                    tbar.Value = sbarVal;

                    // turn on or turn off the LED button
                    EventDimLEDArgs myEvent = new EventDimLEDArgs(LEDIndex, LEDCfgReader.greenDimLEDPower[i]);
                    Button btn = (Button)(_view.Controls.Find($"btnDimLED{dimLEDIndex}", true)[0]);

                    OnSetDimLED(btn, myEvent);
                }

                for (int i = 0; i < LEDCfgReader.redDimLEDPower.Length; i++)
                {
                    int dimLEDIndex = i + 1 + LEDBoardCom.NumGreenDimLED;
                    int LEDIndex = dimLEDIndex;
                    int LEDPowerBit = (Int16)((double)CalcSbarValue(LEDCfgReader.redDimLEDPower[i], LEDIndex) / (double)NumScrollBarLevel * 255);

                    // configure dimmable LED textbox and scrollbar
                    TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimLED{dimLEDIndex}", true)[0]);
                    tbx.Text = Convert.ToString(LEDCfgReader.redDimLEDPower[i]);

                    TrackBar tbar = (TrackBar)(_view.Controls.Find($"sbarDimLED{dimLEDIndex}", true)[0]);
                    int sbarVal = CalcSbarValue(LEDCfgReader.redDimLEDPower[i], dimLEDIndex);
                    tbar.Value = sbarVal;

                    // turn on or turn off the LED button
                    EventDimLEDArgs myEvent = new EventDimLEDArgs(LEDIndex, LEDCfgReader.redDimLEDPower[i]);
                    Button btn = (Button)(_view.Controls.Find($"btnDimLED{dimLEDIndex}", true)[0]);

                    OnSetDimLED(btn, myEvent);
                }

                for (int i = 0; i < LEDCfgReader.darkRedDimLEDPower.Length; i++)
                {
                    int dimLEDIndex = i + 1 + LEDBoardCom.NumGreenDimLED + LEDBoardCom.NumRedDimLED;
                    int LEDIndex = dimLEDIndex;
                    int LEDPowerBit = (Int16)((double)CalcSbarValue(LEDCfgReader.darkRedDimLEDPower[i], LEDIndex) / (double)NumScrollBarLevel * 255);

                    // configure dimmable LED textbox and scrollbar
                    TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimLED{dimLEDIndex}", true)[0]);
                    tbx.Text = Convert.ToString(LEDCfgReader.darkRedDimLEDPower[i]);

                    TrackBar tbar = (TrackBar)(_view.Controls.Find($"sbarDimLED{dimLEDIndex}", true)[0]);
                    int sbarVal = CalcSbarValue(LEDCfgReader.darkRedDimLEDPower[i], dimLEDIndex);
                    tbar.Value = sbarVal;

                    // turn on or turn off the LED button
                    EventDimLEDArgs myEvent = new EventDimLEDArgs(LEDIndex, LEDCfgReader.darkRedDimLEDPower[i]);
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
                    MessageBox.Show($"绿光LED设定功率超出范围 (设定值:{LEDPower})", "警告");
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
                    MessageBox.Show($"红光设定功率超出范围 (设定值:{LEDPower})", "警告");
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
                    MessageBox.Show($"红外设定功率超出范围 (设定值:{LEDPower})", "警告");
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

        private void OnSetDimLED(object sender, EventDimLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn on Dimmable LED
            int LEDPowerBit;
            LEDPowerBit = (Int16)((double)CalcSbarValue(e.LEDPower, e.LEDIndex) / (double)NumScrollBarLevel * 255);   // Convert to 0-255
            try
            {
                connector.SetDimLED(e.LEDIndex, LEDPowerBit);
            }
            catch (Exception ex)
            {
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n";
            }

            ShowSendStatusAsync();

            int numTotalFixLED = LEDBoardCom.NumGreenFixLED + LEDBoardCom.NumRedFixLED + LEDBoardCom.NumDarkRedFixLED;

            // set button color
            Button btn = sender as Button;
            int tagLED = Int32.Parse(btn.Tag as string);
            if ((LEDPowerBit == 0))
            {
                // Turn off the button
                btn.BackColor = Color.Gray;
            }
            else if ((tagLED >= (numTotalFixLED + 1)) && (tagLED <= (numTotalFixLED + LEDBoardCom.NumGreenDimLED)))
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
                connector.SetDimLED(e.LEDIndex, 0);
            }
            catch (Exception ex)
            {
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n";
            }

            ShowSendStatusAsync();

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

        private void OnCloseFixLED(object sender, EventFixLEDArgs e)
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

        private void OnClearSingleLEDStatus(object sender, EventLEDArgs e)
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
                ShowSendStatusAsync();
            }
            catch (Exception ex)
            {
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n";
            }
        }

        public void ReceiveMsg(object sender, DoWorkEventArgs args)
        {

            BackgroundWorker worker = sender as BackgroundWorker;

            // Socket socketSlave = socketHostObj as Socket;
            byte[] buffer = new byte[SendBufferSize];

            while (true)   // Receiving message from slave
            {
                int msgLen = 0;
                if ((connector.socketHost != null) && (!worker.CancellationPending)) 
                {
                    msgLen = connector.socketHost.Receive(buffer);
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
                    catch (Exception ex)
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
            _view.testMsgRecStr = formatRecMsg;;

            // showing total power
            _view.tsslGreenLEDTPText = $"绿光实时总功率: {currentLEDStatus.totalGreenLEDPower}W";
            _view.tsslRedLEDTPText = $"红光实时总功率: {currentLEDStatus.totalRedLEDPower}W";
            _view.tsslDarkRedTPText = $"红外实时总功率: {currentLEDStatus.totalDarkredLEDPower}W";

            // showing temperature sensors
            _view.tsslTemp1Text = $"测温点1: {currentLEDStatus.temperature[0]}°C";
            _view.tsslTemp2Text = $"测温点2: {currentLEDStatus.temperature[1]}°C";
            _view.tsslTemp3Text = $"测温点3: {currentLEDStatus.temperature[2]}°C";
            _view.tsslTemp4Text = $"测温点4: {currentLEDStatus.temperature[3]}°C";

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

        private void OnConnect(object sender, EventArgs e)
        {
            Connect(_view.slaveIP, _view.slavePort);
        }

        private void Connect(string thisIP, string thisPort)
        {
            // Start TCP connection
            connector.slaveIP = thisIP;
            connector.slavePort = thisPort;

            try
            {
                connector.Connect();
                ShowSendStatusAsync();
                _view.toolStripConnectionStatusText = "连接成功";
                // show connection message
                _view.tbxConnectMsgText = "[" + GetCurrentTime() + "]" + " 连接成功\r\n";
                _view.btnConnectColor = Color.Green;
                _view.btnCloseColor = Color.Empty;

                if (!recWorker.IsBusy)
                {
                    recWorker.RunWorkerAsync();
                }
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
                connector.Close();
                recWorker.CancelAsync();
                ShowSendStatusAsync();
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
