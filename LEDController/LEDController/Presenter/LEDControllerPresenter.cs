using LEDController.View;
using LEDController.Model;
using NLog;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace LEDController.Presenter
{
    class LEDControllerPresenter
    {
        private const int SendBufferSize = 2 * 1024;
        private const double MaxGreenLEDPower = 10.0;
        private const double MinGreenLEDPower = 0.0;
        private const double MaxRedLEDPower = 10.0;
        private const double MinRedLEDPower = 0.0;
        private const double MaxDarkRedLEDPower = 10.0;
        private const double MinDarkRedLEDPower = 0.0;
        private const int NumScrollBarLevel = 50;
        private const int NumLiveData = 3600;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private LEDControllerViewer _view;
        private LEDBoardCom connector;
        private byte[] receiveBytes = null;
        private double[] LEDPowerLiveData = new double[NumLiveData];
        private double[] LEDCurrentLiveData = new double[NumLiveData];
        private double[] LEDVoltageLiveData = new double[NumLiveData];
        public Stopwatch sw;
        public System.Threading.Timer updateLEDStatusTimer;
        public System.Threading.Timer renderLEDStatusTimer;
        BackgroundWorker recWorker = new BackgroundWorker();
        public string cfgFileName;
        public string statusDataFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
        FileStream statusDataFS = null;
        private AllLEDStatus ledStatus;

        public LEDControllerPresenter(LEDControllerViewer newView)
        {
            this.sw = Stopwatch.StartNew();
            this.connector = new LEDBoardCom();

            _view = newView;
            _view.ConnectTCP += new EventHandler<EventArgs>(OnConnectTCP);
            _view.CloseTCP += new EventHandler<EventArgs>(OnCloseTCP);
            _view.ConnectSerialPort += new EventHandler<EventArgs>(OnConnectSerialPort);
            _view.CloseSerialPort += new EventHandler<EventArgs>(OnCloseSerialPort);
            _view.SendTestData += new EventHandler<EventArgs>(OnSendTestData);
            _view.ShowFixGreenLEDStatus += new EventHandler<EventLEDArgs>(OnShowFixGreenLEDStatus);
            _view.ShowFixRedLEDStatus += new EventHandler<EventLEDArgs>(OnShowFixRedLEDStatus);
            _view.ShowFixDarkRedLEDStatus += new EventHandler<EventLEDArgs>(OnShowFixDarkRedLEDStatus);
            _view.ShowDimGreenLEDStatus += new EventHandler<EventDimLEDArgs>(OnShowDimGreenLEDStatus);
            _view.ShowDimRedLEDStatus += new EventHandler<EventDimLEDArgs>(OnShowDimRedLEDStatus);
            _view.ShowDimDarkRedLEDStatus += new EventHandler<EventDimLEDArgs>(OnShowDimDarkRedLEDStatus);
            _view.ClearFixGreenLEDStatus += new EventHandler<EventLEDArgs>(OnClearFixLEDStatus);
            _view.ClearFixRedLEDStatus += new EventHandler<EventLEDArgs>(OnClearFixLEDStatus);
            _view.ClearFixDarkRedLEDStatus += new EventHandler<EventLEDArgs>(OnClearFixLEDStatus);
            _view.ClearDimGreenLEDStatus += new EventHandler<EventDimLEDArgs>(OnClearDimLEDStatus);
            _view.ClearDimRedLEDStatus += new EventHandler<EventDimLEDArgs>(OnClearDimLEDStatus);
            _view.ClearDimDarkRedLEDStatus += new EventHandler<EventDimLEDArgs>(OnClearDimLEDStatus);
            _view.OpenFixGreenLED += new EventHandler<EventLEDArgs>(OnOpenFixGreenLED);
            _view.OpenFixRedLED += new EventHandler<EventLEDArgs>(OnOpenFixRedLED);
            _view.OpenFixDarkRedLED += new EventHandler<EventLEDArgs>(OnOpenFixDarkRedLED);
            _view.CloseFixGreenLED += new EventHandler<EventLEDArgs>(OnCloseFixGreenLED);
            _view.CloseFixRedLED += new EventHandler<EventLEDArgs>(OnCloseFixRedLED);
            _view.CloseFixDarkRedLED += new EventHandler<EventLEDArgs>(OnCloseFixDarkRedLED);
            _view.HandleFixGreenLED += new EventHandler<EventLEDArgs>(OnHandleFixGreenLED);
            _view.HandleFixRedLED += new EventHandler<EventLEDArgs>(OnHandleFixRedLED);
            _view.HandleFixDarkRedLED += new EventHandler<EventLEDArgs>(OnHandleFixDarkRedLED);
            _view.SetDimGreenLED += new EventHandler<EventDimLEDArgs>(OnSetDimGreenLED);
            _view.SetDimRedLED += new EventHandler<EventDimLEDArgs>(OnSetDimRedLED);
            _view.SetDimDarkRedLED += new EventHandler<EventDimLEDArgs>(OnSetDimDarkRedLED);
            _view.CloseDimGreenLED += new EventHandler<EventDimLEDArgs>(OnCloseDimGreenLED);
            _view.CloseDimRedLED += new EventHandler<EventDimLEDArgs>(OnCloseDimRedLED);
            _view.CloseDimDarkRedLED += new EventHandler<EventDimLEDArgs>(OnCloseDimDarkRedLED);
            _view.HandleDimGreenLED += new EventHandler<EventDimLEDArgs>(OnHandleDimGreenLED);
            _view.HandleDimRedLED += new EventHandler<EventDimLEDArgs>(OnHandleDimRedLED);
            _view.HandleDimDarkRedLED += new EventHandler<EventDimLEDArgs>(OnHandleDimDarkRedLED);
            _view.UpdateGreenScrollBar += new EventHandler<EventDimLEDArgs>(OnUpdateGreenScrollBar);
            _view.UpdateRedScrollBar += new EventHandler<EventDimLEDArgs>(OnUpdateRedScrollBar);
            _view.UpdateDarkRedScrollBar += new EventHandler<EventDimLEDArgs>(OnUpdateDarkRedScrollBar);
            _view.UpdateDimGreenLEDTbx += new EventHandler<EventDimLEDArgs>(OnUpdateDimGreenLEDTbx);
            _view.UpdateDimRedLEDTbx += new EventHandler<EventDimLEDArgs>(OnUpdateDimRedLEDTbx);
            _view.UpdateDimDarkRedLEDTbx += new EventHandler<EventDimLEDArgs>(OnUpdateDimDarkRedLEDTbx);
            _view.ShowFixGreenLEDStatus += new EventHandler<EventLEDArgs>(OnShowFixGreenLEDStatus);
            _view.ShowLEDStatus += new EventHandler<EventArgs>(OnShowLEDStatus);
            _view.StopShowLEDStatus += new EventHandler<EventArgs>(OnStopShowLEDStatus);
            _view.OpenWithCfgFile += new EventHandler<EventArgs>(OnOpenWithCfgFile);
            _view.SaveCfgFile += new EventHandler<EventArgs>(OnSaveCfgFile);
            _view.SaveAsCfgFile += new EventHandler<EventArgs>(OnSaveAsCfgFile);
            _view.ShowVersion += new EventHandler<EventArgs>(OnShowVersion);
            _view.ShowChillerStatus += new EventHandler<EventArgs>(OnShowChillerStatus);
            _view.TurnOnChiller += new EventHandler<EventArgs>(OnTurnOnChiller);
            _view.TurnOffChiller += new EventHandler<EventArgs>(OnTurnOffChiller);
            _view.TurnOnSkyLight += new EventHandler<EventSkyLightArgs>(OnTurnOnSkyLight);
            _view.TurnOffSkyLight += new EventHandler<EventSkyLightArgs>(OnTurnOffSkyLight);
            _view.TurnOnLight += new EventHandler<EventArgs>(OnTurnOnLight);
            _view.TurnOffLight += new EventHandler<EventArgs>(OnTurnOffLight);
            _view.TurnOnLighMainSwitch += new EventHandler<EventArgs>(OnTurnOnLighMainSwitch);
            _view.TurnOffLighMainSwitch += new EventHandler<EventArgs>(OnTurnOffLighMainSwitch);
            _view.SelectStatusDataSaveFolder += new EventHandler<EventArgs>(OnSelectStatusDataSaveFolder);
            _view.ChangeStatusDataSaveFolder += new EventHandler<EventArgs>(OnChangeStatusDataSaveFolder);
            _view.ChangeStatusDataSaveFolder += new EventHandler<EventArgs>(OnChangeStatusDataSaveFolder);
            _view.TurnOnRTPower += new EventHandler<EventArgs>(OnTurnOnRTPower);
            _view.TurnOffRTPower += new EventHandler<EventArgs>(OnTurnOffRTPower);
            _view.TurnOnAirConditionerPower += new EventHandler<EventArgs>(OnTurnOnAirConditionerPower);
            _view.TurnOffAirConditionerPower += new EventHandler<EventArgs>(OnTurnOffAirConditionerPower);
            _view.TurnOnCamPower += new EventHandler<EventArgs>(OnTurnOnCamPower);
            _view.TurnOffCamPower += new EventHandler<EventArgs>(OnTurnOffCamPower);
            _view.TurnOnPCPower += new EventHandler<EventArgs>(OnTurnOnPCPower);
            _view.TurnOffPCPower += new EventHandler<EventArgs>(OnTurnOffPCPower);
            _view.StartCountDown += new EventHandler<EventArgs>(OnStartCountDown);
            _view.StopCountDown += new EventHandler<EventArgs>(OnStopCountDown);
            _view.StartReceive += new EventHandler<EventArgs>(OnStartReceive);
            _view.StopReceive += new EventHandler<EventArgs>(OnStopReceive);
            _view.ChangeTabIndex += new EventHandler<EventArgs>(OnChangeTabIndex);
            _view.ChangeGreenLEDMainSwitch += new EventHandler<EventArgs>(OnChangeGreenLEDMainSwitch);
            _view.ChangeRedLEDMainSwitch += new EventHandler<EventArgs>(OnChangeRedLEDMainSwitch);
            _view.ChangeDarkRedLEDMainSwitch += new EventHandler<EventArgs>(OnChangeDarkRedLEDMainSwitch);
            _view.ChangeMinValue += new EventHandler<EventArgs>(OnChangeMinValue);
            _view.ChangeMaxValue += new EventHandler<EventArgs>(OnChangeMaxValue);

            // Initialize Form
            InitialForm();

            // Initialize LED status plot
            this.ledStatus = new AllLEDStatus();
            sw.Start();
        }

        public void OnChangeMaxValue(object sender, EventArgs e)
        {
            FormsPlot formsLEDStatusPlot = (FormsPlot)(this._view.Controls.Find("formsLEDStatusPlot", true)[0]);
            TextBox tbxMinValue = (TextBox)(this._view.Controls.Find("tbxMinValue", true)[0]);
            TextBox tbxMaxValue = (TextBox)(this._view.Controls.Find("tbxMaxValue", true)[0]);
            formsLEDStatusPlot.Plot.SetAxisLimitsY(Convert.ToDouble(tbxMinValue.Text), Convert.ToDouble(tbxMaxValue.Text));

            formsLEDStatusPlot.Refresh();
        }

        public void OnChangeMinValue(object sender, EventArgs e)
        {
            FormsPlot formsLEDStatusPlot = (FormsPlot)(this._view.Controls.Find("formsLEDStatusPlot", true)[0]);
            TextBox tbxMinValue = (TextBox)(this._view.Controls.Find("tbxMinValue", true)[0]);
            TextBox tbxMaxValue = (TextBox)(this._view.Controls.Find("tbxMaxValue", true)[0]);
            formsLEDStatusPlot.Plot.SetAxisLimitsY(Convert.ToDouble(tbxMinValue.Text), Convert.ToDouble(tbxMaxValue.Text));

            formsLEDStatusPlot.Refresh();
        }

        public void OnChangeGreenLEDMainSwitch(object sender, EventArgs e)
        {
            CheckBox cbxGreenLEDMainSwitch = (CheckBox)(this._view.Controls.Find("cbxGreenLEDMainSwitch", true)[0]);

            if (this.connector.isAlive)
            {
                if (cbxGreenLEDMainSwitch.Checked)
                {
                    this.connector.TurnOffGreenLEDMainSwitch();
                    cbxGreenLEDMainSwitch.Checked = false;
                }
                else
                {
                    this.connector.TurnOnGreenLEDMainSwitch();
                    cbxGreenLEDMainSwitch.Checked = true;
                }
            }
            else
            {
                MessageBox.Show("Connection is not ready!", "Warning");
                logger.Error("Connection is not set up.");
            }
        }

        public void OnChangeRedLEDMainSwitch(object sender, EventArgs e)
        {
            CheckBox cbxRedLEDMainSwitch = (CheckBox)(this._view.Controls.Find("cbxRedLEDMainSwitch", true)[0]);

            if (this.connector.isAlive)
            {
                if (cbxRedLEDMainSwitch.Checked)
                {
                    this.connector.TurnOffRedLEDMainSwitch();
                    cbxRedLEDMainSwitch.Checked = false;
                }
                else
                {
                    this.connector.TurnOnRedLEDMainSwitch();
                    cbxRedLEDMainSwitch.Checked = true;
                }
            }
            else
            {
                MessageBox.Show("Connection is not ready!", "Warning");
                throw new IOException("Connection is not set up.");
            }
        }

        public void OnChangeDarkRedLEDMainSwitch(object sender, EventArgs e)
        {
            CheckBox cbxDarkRedLEDMainSwitch = (CheckBox)(this._view.Controls.Find("cbxDarkRedLEDMainSwitch", true)[0]);

            if (this.connector.isAlive)
            {
                if (cbxDarkRedLEDMainSwitch.Checked)
                {
                    this.connector.TurnOffDarkRedLEDMainSwitch();
                    cbxDarkRedLEDMainSwitch.Checked = false;
                }
                else
                {
                    this.connector.TurnOnDarkRedLEDMainSwitch();
                    cbxDarkRedLEDMainSwitch.Checked = true;
                }
            }
            else
            {
                MessageBox.Show("Connection is not ready!", "Warning");
                logger.Error("Connection is not set up.");
            }
        }

        public void OnChangeTabIndex(object sender, EventArgs e)
        {
            TabControl tabCtrlMain = sender as TabControl;

            if ((tabCtrlMain.SelectedIndex != 0) && (recWorker.IsBusy))
            {
                OnStopReceive(sender, e);
            }
        }

        public void OnStopReceive(object sender, EventArgs e)
        {
            Button btnStartReceive = (Button)(this._view.Controls.Find("btnStartReceive", true)[0]);
            btnStartReceive.BackColor = Color.Transparent;

            if (recWorker.IsBusy)
            {
                recWorker.CancelAsync();
            }
        }

        public void OnStartReceive(object sender, EventArgs e)
        {
            Button btnStartReceive = (Button)(this._view.Controls.Find("btnStartReceive", true)[0]);
            btnStartReceive.BackColor = Color.Green;

            recWorker.WorkerReportsProgress = true;
            recWorker.WorkerSupportsCancellation = true;
            recWorker.DoWork += ReceiveMsg;
            recWorker.ProgressChanged += ShowReceiveStatusAsync;
            recWorker.RunWorkerCompleted += OnConnectionBreak;

            if (!recWorker.IsBusy)
            {
                recWorker.RunWorkerAsync();
            }
        }

        public void OnStopCountDown(object sender, EventArgs e)
        {
            Label lblCountDown = (Label)(this._view.Controls.Find("lblCountDown", true)[0]);
            lblCountDown.Text = "0 s";
        }

        public void OnStartCountDown(object sender, EventArgs e)
        {
            Label lblCountDown = (Label)(this._view.Controls.Find("lblCountDown", true)[0]);
            ComboBox cbxQueryWaitTime = (ComboBox)(this._view.Controls.Find("cbxQueryWaitTime", true)[0]);

            double countTime = Convert.ToDouble(lblCountDown.Text.Replace(" s", "")) + 1;

            lblCountDown.Text = $"{(countTime % Convert.ToDouble(cbxQueryWaitTime.Items[cbxQueryWaitTime.SelectedIndex].ToString()))} s";
        }

        public void OnTurnOnCamPower(object sender, EventArgs e)
        {
            this.connector.TurnOnCamPower(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxCamPower = (PictureBox)(this._view.Controls.Find("pbxCamPower", true)[0]);
            pbxCamPower.Image = Properties.Resources.power_on;
        }

        public void OnTurnOffCamPower(object sender, EventArgs e)
        {
            this.connector.TurnOffCamPower(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxCamPower = (PictureBox)(this._view.Controls.Find("pbxCamPower", true)[0]);
            pbxCamPower.Image = Properties.Resources.power_off;
        }

        public void OnTurnOnPCPower(object sender, EventArgs e)
        {
            this.connector.TurnOnPCPower(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxPCPower = (PictureBox)(this._view.Controls.Find("pbxPCPower", true)[0]);
            pbxPCPower.Image = Properties.Resources.power_on;
        }

        public void OnTurnOffPCPower(object sender, EventArgs e)
        {
            this.connector.TurnOffPCPower(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxPCPower = (PictureBox)(this._view.Controls.Find("pbxPCPower", true)[0]);
            pbxPCPower.Image = Properties.Resources.power_off;
        }

        public void OnTurnOnAirConditionerPower(object sender, EventArgs e)
        {
            this.connector.TurnOnAirConditionerPower(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxAirConditionerPower = (PictureBox)(this._view.Controls.Find("pbxAirConditionerPower", true)[0]);
            pbxAirConditionerPower.Image = Properties.Resources.power_on;
        }

        public void OnTurnOffAirConditionerPower(object sender, EventArgs e)
        {
            this.connector.TurnOffAirConditionerPower(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxAirConditionerPower = (PictureBox)(this._view.Controls.Find("pbxAirConditionerPower", true)[0]);
            pbxAirConditionerPower.Image = Properties.Resources.power_off;
        }

        public void OnTurnOnRTPower(object sender, EventArgs e)
        {
            this.connector.TurnOnRTPower(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxRTPower = (PictureBox)(this._view.Controls.Find("pbxRTPower", true)[0]);
            pbxRTPower.Image = Properties.Resources.power_on;
        }

        public void OnTurnOffRTPower(object sender, EventArgs e)
        {
            this.connector.TurnOffRTPower(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxRTPower = (PictureBox)(this._view.Controls.Find("pbxRTPower", true)[0]);
            pbxRTPower.Image = Properties.Resources.power_off;
        }

        public void OnTurnOffLighMainSwitch(object sender, EventArgs e)
        {
            this.connector.TurnOffLightMainSwitch(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxLightMainPower = (PictureBox)(this._view.Controls.Find("pbxLightMainPower", true)[0]);
            pbxLightMainPower.Image = Properties.Resources.power_off;
        }

        public void OnTurnOnLighMainSwitch(object sender, EventArgs e)
        {
            this.connector.TurnOnLightMainSwitch(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxLightMainPower = (PictureBox)(this._view.Controls.Find("pbxLightMainPower", true)[0]);
            pbxLightMainPower.Image = Properties.Resources.power_on;
        }

        public void OnTurnOnChiller(object sender, EventArgs e)
        {
            this.connector.TurnOnChiller(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxChillerPower = (PictureBox)(this._view.Controls.Find("pbxChillerPower", true)[0]);
            pbxChillerPower.Image = Properties.Resources.power_on;
        }

        public void OnTurnOffChiller(object sender, EventArgs e)
        {
            this.connector.TurnOffChiller(LEDConfig.addrPLCDarkRedLED);
            ShowSendStatusAsync();

            PictureBox pbxChillerPower = (PictureBox)(this._view.Controls.Find("pbxChillerPower", true)[0]);
            pbxChillerPower.Image = Properties.Resources.power_off;
        }

        public void OnChangeStatusDataSaveFolder(object sender, EventArgs e)
        {
            TextBox tbxStatusSaveFolder = (TextBox)(this._view.Controls.Find("tbxStatusSaveFolder", true)[0]);

            if (tbxStatusSaveFolder.Text != "")
            {
                statusDataFolder = tbxStatusSaveFolder.Text;
            }
        }

        public void OnSelectStatusDataSaveFolder(object sender, EventArgs e)
        {
            FolderBrowserDialog saveFolderDlg = new FolderBrowserDialog();
            saveFolderDlg.ShowNewFolderButton = true;
            saveFolderDlg.Description = "请选择LED状态数据保存目录";
            saveFolderDlg.SelectedPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");

            if (saveFolderDlg.ShowDialog() == DialogResult.OK)
            {
                statusDataFolder = saveFolderDlg.SelectedPath;
                TextBox tbxStatusSaveFolder = (TextBox)(this._view.Controls.Find("tbxStatusSaveFolder", true)[0]);
                tbxStatusSaveFolder.Text = statusDataFolder;
            }
        }

        public void OnTurnOnLight(object sender, EventArgs e)
        {
            this.connector.TurnOnLight(1);
            ShowSendStatusAsync();

            PictureBox pbxLight = (PictureBox)(this._view.Controls.Find($"pbxLight", true)[0]);
            pbxLight.Image = Properties.Resources.light_on;
        }

        public void OnTurnOffLight(object sender, EventArgs e)
        {
            this.connector.TurnOffLight(1);
            ShowSendStatusAsync();

            PictureBox pbxLight = (PictureBox)(this._view.Controls.Find($"pbxLight", true)[0]);
            pbxLight.Image = Properties.Resources.light_off;
        }

        public void OnShowChillerStatus(object sender, EventArgs e)
        {
            for (int i = 1; i <= 2; i++)
            {
                PictureBox pbxChiller = (PictureBox)(this._view.Controls.Find($"pbxChiller{i}", true)[0]);
                ShowSendStatusAsync();

                try
                {
                    if (this.connector.IsChillerWarning(1, i))
                    {
                        pbxChiller.Image = Properties.Resources.chiller_error;
                    }
                    else
                    {
                        pbxChiller.Image = Properties.Resources.chiller_normal;
                    }
                }
                catch
                {
                    pbxChiller.Image = Properties.Resources.question_circle_fill;
                }
            }

            for (int j = 1; j <= 3; j++)
            {
                PictureBox pbxPump = (PictureBox)(this._view.Controls.Find($"pbxPump{j}", true)[0]);
                ShowSendStatusAsync();

                try
                {
                    if (this.connector.IsPumpWarning(1, j))
                    {
                        pbxPump.Image = Properties.Resources.chiller_error;
                    }
                    else
                    {
                        pbxPump.Image = Properties.Resources.chiller_normal;
                    }
                }
                catch
                {
                    pbxPump.Image = Properties.Resources.question_circle_fill;
                }
            }
        }

        public void OnTurnOnSkyLight(object sender, EventSkyLightArgs e)
        {
            this.connector.TurnOnSkyLight(1, e.SkyLightIndex);
            ShowSendStatusAsync();

            PictureBox pbxSkyLight = (PictureBox)(this._view.Controls.Find($"pbxSkyLight{e.SkyLightIndex}", true)[0]);
            pbxSkyLight.Image = Properties.Resources.window_open;
        }

        public void OnTurnOffSkyLight(object sender, EventSkyLightArgs e)
        {
            this.connector.TurnOffSkyLight(1, e.SkyLightIndex);
            ShowSendStatusAsync();

            PictureBox pbxSkyLight = (PictureBox)(this._view.Controls.Find($"pbxSkyLight{e.SkyLightIndex}", true)[0]);
            pbxSkyLight.Image = Properties.Resources.window_closed;
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
                cbxDataBit.SelectedItem.ToString(),
                cbxCheckBit.SelectedItem.ToString(),
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
                this.connector.Connect(200);
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
            }
            catch (Exception ex)
            {
                logger.Warn("Connection failure");
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
            Button btnStartReceive = (Button)(this._view.Controls.Find("btnStartReceive", true)[0]);

            // Close connection
            try
            {
                this.connector.Disconnect();
                if (recWorker.IsBusy)
                {
                    recWorker.CancelAsync();
                }
                btnStartReceive.BackColor = Color.Transparent;
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
                logger.Warn("Connection break");
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

            RadioButton rbnSendHEX = (RadioButton)(this._view.Controls.Find("rbnSendHEX", true)[0]);
            RadioButton rbnRecHEX = (RadioButton)(this._view.Controls.Find("rbnRecHEX", true)[0]);
            rbnSendHEX.Checked = true;
            rbnRecHEX.Checked = true;

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

            TextBox tbxStatusSaveFolder = (TextBox)(this._view.Controls.Find("tbxStatusSaveFolder", true)[0]);
            tbxStatusSaveFolder.Text = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");

            ComboBox cbxQueryWaitTime = (ComboBox)(this._view.Controls.Find("cbxQueryWaitTime", true)[0]);
            string[] waitTStr = {"1", "2", "5", "10", "15", "30", "60"};
            foreach (string waitT in waitTStr)
            {
                cbxQueryWaitTime.Items.Add(waitT);
            }
            cbxQueryWaitTime.SelectedIndex = 2;

            ComboBox cbxQueryParam = (ComboBox)(this._view.Controls.Find("cbxQueryParam", true)[0]);
            string[] queryParams = {"LED电流", "LED电压", "LED功率"};
            foreach (string queryParam in queryParams)
            {
                cbxQueryParam.Items.Add(queryParam);
            }
            cbxQueryParam.SelectedIndex = 0;

            FormsPlot formsLEDStatusPlot = (FormsPlot)(this._view.Controls.Find("formsLEDStatusPlot", true)[0]);
            TextBox tbxMinValue = (TextBox)(this._view.Controls.Find("tbxMinValue", true)[0]);
            TextBox tbxMaxValue = (TextBox)(this._view.Controls.Find("tbxMaxValue", true)[0]);
            var sig = formsLEDStatusPlot.Plot.AddSignal(LEDPowerLiveData, sampleRate: 3600 * 24.0, label: "功率");
            formsLEDStatusPlot.Plot.AddSignal(LEDCurrentLiveData, sampleRate: 3600 * 24.0, label: "电流");
            formsLEDStatusPlot.Plot.AddSignal(LEDVoltageLiveData, sampleRate: 3600 * 24.0, label: "电压");
            formsLEDStatusPlot.Plot.Title("LED状态");
            formsLEDStatusPlot.Plot.XLabel("时间");
            formsLEDStatusPlot.Plot.YLabel("强度");
            formsLEDStatusPlot.Plot.Grid(true);
            formsLEDStatusPlot.Plot.SetAxisLimitsY(Convert.ToDouble(tbxMinValue.Text), (Convert.ToDouble(tbxMaxValue.Text)));
            sig.OffsetX = GetCurrentTime().ToOADate();
            formsLEDStatusPlot.Plot.SetAxisLimitsX(GetCurrentTime().ToOADate(), GetCurrentTime().AddHours(1.0).ToOADate());
            formsLEDStatusPlot.Plot.XAxis.ManualTickSpacing(5, ScottPlot.Ticks.DateTimeUnit.Minute);
            formsLEDStatusPlot.Plot.XAxis.TickLabelFormat("HH:mm", true);
            formsLEDStatusPlot.Plot.XAxis.DateTimeFormat(true);
            var legend = formsLEDStatusPlot.Plot.Legend(location: ScottPlot.Alignment.UpperRight);
            formsLEDStatusPlot.Refresh();

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
            ComboBox cbxCOMPort = (ComboBox)(this._view.Controls.Find("cbxCOMPort", true)[0]);
            ComboBox cbxDataBit = (ComboBox)(this._view.Controls.Find("cbxDataBit", true)[0]);
            ComboBox cbxCheckBit = (ComboBox)(this._view.Controls.Find("cbxCheckBit", true)[0]);
            ComboBox cbxStopBit = (ComboBox)(this._view.Controls.Find("cbxStopBit", true)[0]);
            ComboBox cbxBaudRate = (ComboBox)(this._view.Controls.Find("cbxBaudRate", true)[0]);
            LEDCfgWriter.serialName = cbxCOMPort.SelectedItem.ToString();
            LEDCfgWriter.baudRate = cbxBaudRate.SelectedItem.ToString();
            LEDCfgWriter.checkBit = cbxCheckBit.SelectedItem.ToString();
            LEDCfgWriter.stopBit = cbxStopBit.SelectedItem.ToString();
            LEDCfgWriter.dataBit = cbxDataBit.SelectedItem.ToString();

            RadioButton rbnSendHEX = (RadioButton)(this._view.Controls.Find("rbnSendHEX", true)[0]);
            RadioButton rbnRecHEX = (RadioButton)(this._view.Controls.Find("rbnRecHEX", true)[0]);
            LEDCfgWriter.isSendASCII = !rbnSendHEX.Checked;
            LEDCfgWriter.isRecASCII = !rbnRecHEX.Checked;

            LEDCfgWriter.protocol = "SerialPort";

            for (int i = 0; i < LEDConfig.NumFixGreenLED; i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(_view.Controls.Find($"btnGreenLED{LEDIndex}", true)[0]);

                LEDCfgWriter.isFixGreenLEDOn[i] = (btn.BackColor == Color.Green);
            }

            for (int i = 0; i < LEDConfig.NumFixRedLED; i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(_view.Controls.Find($"btnRedLED{LEDIndex}", true)[0]);

                LEDCfgWriter.isFixRedLEDOn[i] = (btn.BackColor == Color.Red);
            }

            for (int i = 0; i < LEDConfig.NumFixDarkRedLED; i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(_view.Controls.Find($"btnDarkRedLED{LEDIndex}", true)[0]);

                LEDCfgWriter.isFixDarkRedLEDOn[i] = (btn.BackColor == Color.DarkRed);
            }

            for (int i = 0; i < LEDConfig.NumDimGreenLED; i++)
            {
                int LEDIndex = i + 1;
                TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimGreenLED{LEDIndex}", true)[0]);

                LEDCfgWriter.dimGreenLEDPower[i] = Convert.ToDouble(tbx.Text);
            }

            for (int i = 0; i < LEDConfig.NumDimRedLED; i++)
            {
                int LEDIndex = i + 1;
                TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimRedLED{LEDIndex}", true)[0]);

                LEDCfgWriter.dimRedLEDPower[i] = Convert.ToDouble(tbx.Text);
            }

            for (int i = 0; i < LEDConfig.NumDimDarkRedLED; i++)
            {
                int LEDIndex = i + 1;
                TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimDarkRedLED{LEDIndex}", true)[0]);

                LEDCfgWriter.dimDarkRedLEDPower[i] = Convert.ToDouble(tbx.Text);
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
                saveFileDlg.CheckFileExists = false;
                saveFileDlg.RestoreDirectory = true;

                if (saveFileDlg.ShowDialog() == DialogResult.OK)
                {
                    if (saveFileDlg.FileName != "")
                    {
                        cfgFileName = saveFileDlg.FileName;

                        LEDControllerCfg LEDCfgWriter = GetUISettings(_view);
                        LEDCfgWriter.LEDControllerCfgSave(cfgFileName);

                        MessageBox.Show("保存配置文件成功!");
                    }
                }
            }
        }

        public void OnSaveAsCfgFile(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDlg = new SaveFileDialog();
            saveFileDlg.Filter = "LEDController Configuration file|*.ini";
            saveFileDlg.Title = "Save as LEDController Configuration file...";
            saveFileDlg.CheckFileExists = false;
            saveFileDlg.RestoreDirectory = true;

            if (saveFileDlg.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDlg.FileName != "")
                {
                    cfgFileName = saveFileDlg.FileName;

                    LEDControllerCfg LEDCfgWriter = GetUISettings(_view);
                    LEDCfgWriter.LEDControllerCfgSave(cfgFileName);

                    MessageBox.Show("保存配置文件成功!");
                }
            }
        }

        private void OnOpenWithCfgFile(object sender, EventArgs e)
        {
            // Choose a configuration file
            OpenFileDialog openFileDlg = new OpenFileDialog();

            openFileDlg.InitialDirectory = "c:\\";
            openFileDlg.Filter = "LEDController Configuration files (*.ini)|*.ini";
            openFileDlg.FilterIndex = 0;
            openFileDlg.RestoreDirectory = true;

            if (openFileDlg.ShowDialog() == DialogResult.OK)
            {
                LEDControllerCfg LEDCfgReader = new LEDControllerCfg(openFileDlg.FileName);
                cfgFileName = openFileDlg.FileName;

                if (connector.isAlive) connector.Disconnect();

                // Show IP and Port
                TextBox slaveIPTbx = (TextBox)(_view.Controls.Find("tbxIP", true)[0]);
                TextBox slavePortTbx = (TextBox)(_view.Controls.Find("tbxPort", true)[0]);
                slaveIPTbx.Text = LEDCfgReader.slaveIP;
                slavePortTbx.Text = LEDCfgReader.slavePort;

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
                    switch (LEDCfgReader.protocol)
                    {
                        case "SerialPort":
                            OnConnectSerialPort(sender, e);
                            break;

                        case "TCP":
                            OnConnectTCP(sender, e);
                            break;

                        default:
                            logger.Error($"Unknown protocol {LEDCfgReader.protocol}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                    tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
                    return;
                }

                // Turn on Fix LEDs
                for (int i = 0; i < LEDCfgReader.isFixGreenLEDOn.Length; i++)
                {
                    int LEDIndex = i + 1;
                    EventLEDArgs myEvent = new EventLEDArgs(LEDIndex);
                    Button btn = (Button)(_view.Controls.Find($"btnGreenLED{LEDIndex}", true)[0]);

                    if (LEDCfgReader.isFixGreenLEDOn[i])
                    {
                        OnOpenFixGreenLED(btn, myEvent);
                    }
                    else
                    {
                        OnCloseFixGreenLED(btn, myEvent);
                    }
                }

                for (int i = 0; i < LEDCfgReader.isFixRedLEDOn.Length; i++)
                {
                    int LEDIndex = i + 1;
                    EventLEDArgs myEvent = new EventLEDArgs(LEDIndex);
                    Button btn = (Button)(_view.Controls.Find($"btnRedLED{LEDIndex}", true)[0]);

                    if (LEDCfgReader.isFixRedLEDOn[i])
                    {
                        OnOpenFixRedLED(btn, myEvent);
                    }
                    else
                    {
                        OnCloseFixRedLED(btn, myEvent);
                    }
                }

                for (int i = 0; i < LEDCfgReader.isFixDarkRedLEDOn.Length; i++)
                {
                    int LEDIndex = i + 1;
                    EventLEDArgs myEvent = new EventLEDArgs(LEDIndex);
                    Button btn = (Button)(_view.Controls.Find($"btnDarkRedLED{LEDIndex}", true)[0]);

                    if (LEDCfgReader.isFixDarkRedLEDOn[i])
                    {
                        OnOpenFixDarkRedLED(btn, myEvent);
                    }
                    else
                    {
                        OnCloseFixDarkRedLED(btn, myEvent);
                    }
                }

                for (int i = 0; i < LEDCfgReader.dimGreenLEDPower.Length; i++)
                {
                    int dimLEDIndex = i + 1;
                    int LEDIndex = dimLEDIndex;
                    int LEDPowerBit = (Int16)((double)CalcSbarValue(LEDCfgReader.dimGreenLEDPower[i], LEDIndex, 3) / (double)NumScrollBarLevel * 1000);

                    // configure dimmable LED textbox and scrollbar
                    TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimGreenLED{dimLEDIndex}", true)[0]);
                    tbx.Text = Convert.ToString(LEDCfgReader.dimGreenLEDPower[i]);

                    TrackBar tbar = (TrackBar)(_view.Controls.Find($"sbarDimGreenLED{dimLEDIndex}", true)[0]);
                    int sbarVal = CalcSbarValue(LEDCfgReader.dimGreenLEDPower[i], dimLEDIndex, 3);
                    tbar.Value = sbarVal;

                    // turn on or turn off the LED button
                    EventDimLEDArgs myEvent = new EventDimLEDArgs(LEDIndex, LEDCfgReader.dimGreenLEDPower[i]);
                    Button btn = (Button)(_view.Controls.Find($"btnDimGreenLED{dimLEDIndex}", true)[0]);

                    OnSetDimGreenLED(btn, myEvent);
                }

                for (int i = 0; i < LEDCfgReader.dimRedLEDPower.Length; i++)
                {
                    int dimLEDIndex = i + 1;
                    int LEDIndex = dimLEDIndex;
                    int LEDPowerBit = (Int16)((double)CalcSbarValue(LEDCfgReader.dimRedLEDPower[i], LEDIndex, 1) / (double)NumScrollBarLevel * 1000);

                    // configure dimmable LED textbox and scrollbar
                    TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimRedLED{dimLEDIndex}", true)[0]);
                    tbx.Text = Convert.ToString(LEDCfgReader.dimRedLEDPower[i]);

                    TrackBar tbar = (TrackBar)(_view.Controls.Find($"sbarDimRedLED{dimLEDIndex}", true)[0]);
                    int sbarVal = CalcSbarValue(LEDCfgReader.dimRedLEDPower[i], dimLEDIndex, 1);
                    tbar.Value = sbarVal;

                    // turn on or turn off the LED button
                    EventDimLEDArgs myEvent = new EventDimLEDArgs(LEDIndex, LEDCfgReader.dimRedLEDPower[i]);
                    Button btn = (Button)(_view.Controls.Find($"btnDimRedLED{dimLEDIndex}", true)[0]);

                    OnSetDimRedLED(btn, myEvent);
                }

                for (int i = 0; i < LEDCfgReader.dimDarkRedLEDPower.Length; i++)
                {
                    int dimLEDIndex = i + 1;
                    int LEDIndex = dimLEDIndex;
                    int LEDPowerBit = (Int16)((double)CalcSbarValue(LEDCfgReader.dimDarkRedLEDPower[i], LEDIndex, 2) / (double)NumScrollBarLevel * 1000);

                    // configure dimmable LED textbox and scrollbar
                    TextBox tbx = (TextBox)(_view.Controls.Find($"tbxDimDarkRedLED{dimLEDIndex}", true)[0]);
                    tbx.Text = Convert.ToString(LEDCfgReader.dimDarkRedLEDPower[i]);

                    TrackBar tbar = (TrackBar)(_view.Controls.Find($"sbarDimDarkRedLED{dimLEDIndex}", true)[0]);
                    int sbarVal = CalcSbarValue(LEDCfgReader.dimDarkRedLEDPower[i], dimLEDIndex, 2);
                    tbar.Value = sbarVal;

                    // turn on or turn off the LED button
                    EventDimLEDArgs myEvent = new EventDimLEDArgs(LEDIndex, LEDCfgReader.dimDarkRedLEDPower[i]);
                    Button btn = (Button)(_view.Controls.Find($"btnDimDarkRedLED{dimLEDIndex}", true)[0]);

                    OnSetDimDarkRedLED(btn, myEvent);
                }
            }
        }

        private void RenderLEDStatus(object state)
        {
            SignalPlot sig = state as SignalPlot;
            sig.OffsetX = GetCurrentTime().ToOADate();

            FormsPlot formsLEDStatusPlot = (FormsPlot)(this._view.Controls.Find("formsLEDStatusPlot", true)[0]);
            formsLEDStatusPlot.Plot.SetAxisLimitsX(GetCurrentTime().ToOADate(), GetCurrentTime().AddHours(1.0).ToOADate());
            formsLEDStatusPlot.Refresh();
        }

        private void UpdateLEDLiveData(object state)
        {
            int thisIndex = (int)(sw.Elapsed.TotalSeconds) % 3600;

            // Replace the upper part with below 3 lines
            LEDPowerLiveData[thisIndex] = (this.ledStatus.CalcLEDTotalPower());
            LEDVoltageLiveData[thisIndex] = (this.ledStatus.CalcLEDTotalVoltage());
            LEDCurrentLiveData[thisIndex] = (this.ledStatus.CalcLEDTotalCurrent());
            FormsPlot formsLEDStatusPlot = (FormsPlot)(this._view.Controls.Find("formsLEDStatusPlot", true)[0]);

            if (formsLEDStatusPlot.IsDisposed)
            {
                formsLEDStatusPlot.Refresh();
            }
        }

        private void OnConnectionBreak(object sender, RunWorkerCompletedEventArgs args)
        {
            if (recWorker.IsBusy)
            {
                recWorker.CancelAsync();
            }
            
            Button btnStartReceive = (Button)(this._view.Controls.Find("btnStartReceive", true)[0]);
            btnStartReceive.BackColor = Color.Transparent;
        }

        private void OnStopShowLEDStatus(object sender, EventArgs e)
        {
            if (statusDataFS != null)
            {
                statusDataFS.Close();
                statusDataFS = null;
            }
        }

        private void OnShowLEDStatus(object sender, EventArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            if ((statusDataFolder != null) && (statusDataFS == null))
            {
                string statusDataFile = Path.Combine(statusDataFolder, $"LED-Status-{GetCurrentTime().ToString("yyyymmdd-HHMMSS")}.txt");
                statusDataFS = File.Create(statusDataFile);
            }

            try
            {
                this.ledStatus = this.connector.QueryAllLEDStatus();
            }
            catch
            {
                // do something
            }

            // Show LED status
            if (this.ledStatus.isValidStatus)
            {
                StatusStrip statusStrip1 = (StatusStrip)(this._view.Controls.Find("statusStrip1", true)[0]);

                // showing total power
                ToolStripStatusLabel tsslGreenLEDTotalPower = statusStrip1.Items[0] as ToolStripStatusLabel;
                ToolStripStatusLabel tsslRedLEDTotalPower = statusStrip1.Items[1] as ToolStripStatusLabel;
                ToolStripStatusLabel tsslDarkRedLEDTotalPower = statusStrip1.Items[2] as ToolStripStatusLabel;
                tsslGreenLEDTotalPower.Text = $"绿光实时总功率: " + string.Format("{0,6:F3}", this.ledStatus.CalcTotalGreenLEDPower()) + " V";
                tsslRedLEDTotalPower.Text = $"红光实时总功率: " + string.Format("{0,6:F3}", this.ledStatus.CalcTotalRedLEDPower()) + " V";
                tsslDarkRedLEDTotalPower.Text = $"红外实时总功率: " + string.Format("{0,6:F3}", this.ledStatus.CalcTotalDarkRedLEDPower()) + " V";

                Label lblGreenLEDTempLU = (Label)(this._view.Controls.Find("lblGreenLEDTempLU", true)[0]);
                Label lblGreenLEDTempLD = (Label)(this._view.Controls.Find("lblGreenLEDTempLD", true)[0]);
                Label lblGreenLEDTempRD = (Label)(this._view.Controls.Find("lblGreenLEDTempRD", true)[0]);
                Label lblGreenLEDTempRU = (Label)(this._view.Controls.Find("lblGreenLEDTempRU", true)[0]);
                Label lblRedLEDTempLU = (Label)(this._view.Controls.Find("lblRedLEDTempLU", true)[0]);
                Label lblRedLEDTempLD = (Label)(this._view.Controls.Find("lblRedLEDTempLD", true)[0]);
                Label lblRedLEDTempRD = (Label)(this._view.Controls.Find("lblRedLEDTempRD", true)[0]);
                Label lblRedLEDTempRU = (Label)(this._view.Controls.Find("lblRedLEDTempRU", true)[0]);
                Label lblDarkRedLEDTempLU = (Label)(this._view.Controls.Find("lblDarkRedLEDTempLU", true)[0]);
                Label lblDarkRedLEDTempLD = (Label)(this._view.Controls.Find("lblDarkRedLEDTempLD", true)[0]);
                Label lblDarkRedLEDTempRD = (Label)(this._view.Controls.Find("lblDarkRedLEDTempRD", true)[0]);
                Label lblDarkRedLEDTempRU = (Label)(this._view.Controls.Find("lblDarkRedLEDTempRU", true)[0]);
                lblGreenLEDTempLU.Text = this.ledStatus.tempGreenLED[0].ToString("F1");
                lblGreenLEDTempLD.Text = this.ledStatus.tempGreenLED[1].ToString("F1");
                lblGreenLEDTempRD.Text = this.ledStatus.tempGreenLED[2].ToString("F1");
                lblGreenLEDTempRU.Text = this.ledStatus.tempGreenLED[3].ToString("F1");
                lblRedLEDTempLU.Text = this.ledStatus.tempRedLED[0].ToString("F1");
                lblRedLEDTempLD.Text = this.ledStatus.tempRedLED[1].ToString("F1");
                lblRedLEDTempRD.Text = this.ledStatus.tempRedLED[2].ToString("F1");
                lblRedLEDTempRU.Text = this.ledStatus.tempRedLED[3].ToString("F1");
                lblDarkRedLEDTempLU.Text = this.ledStatus.tempDarkRedLED[0].ToString("F1");
                lblDarkRedLEDTempLD.Text = this.ledStatus.tempDarkRedLED[1].ToString("F1");
                lblDarkRedLEDTempRD.Text = this.ledStatus.tempDarkRedLED[2].ToString("F1");
                lblDarkRedLEDTempRU.Text = this.ledStatus.tempDarkRedLED[3].ToString("F1");

                if (statusDataFS != null)
                {
                    byte[] info = new UTF8Encoding(true).GetBytes($"[{this.ledStatus.updatedTime.ToString("yyyy-mm-dd HH:MM:SS")}] 绿光总功率= {this.ledStatus.CalcTotalGreenLEDPower().ToString("F3")} V; 红光总功率: {this.ledStatus.CalcTotalRedLEDPower().ToString("F3")} V; 红外总功率: {this.ledStatus.CalcTotalDarkRedLEDPower().ToString("F03")} V" + "\r\n");
                    statusDataFS.Write(info, 0, info.Length);
                }

                try
                {
                    double[] LEDCurrent = this.ledStatus.GetLEDCurrentArray();
                    double[] LEDVoltage = this.ledStatus.GetLEDVoltageArray();
                    double[] LEDPower = this.ledStatus.GetLEDPowerArray();

                    TextBox tbxMinNormValue = (TextBox)(this._view.Controls.Find("tbxMinNormValue", true)[0]);
                    TextBox tbxMaxNormValue = (TextBox)(this._view.Controls.Find("tbxMaxNormValue", true)[0]);
                    double minValue = Convert.ToDouble(tbxMinNormValue.Text);
                    double maxValue = Convert.ToDouble(tbxMaxNormValue.Text);

                    ComboBox cbxQueryParam = (ComboBox)(this._view.Controls.Find("cbxQueryParam", true)[0]);
                    string queryType = (string)cbxQueryParam.SelectedItem;

                    // Convert values to color
                    List<Control> btnList = new List<Control>();
                    Panel panelLEDStatus = (Panel)(this._view.Controls.Find("panelLEDStatus", true)[0]);
                    this._view.GetAllControl(panelLEDStatus, btnList, "btnStatusLED");
                    Color[] LEDControlColors = new Color[btnList.Count()];
                    bool isLEDWarning = false;
                    bool isUpdate = false;
                    for (int i = 0; i < btnList.Count(); i++)
                    {
                        switch (queryType)
                        {
                            case "LED电流":
                                isLEDWarning = (LEDCurrent[i] > maxValue) || (LEDCurrent[i] < minValue) ? true : false;
                                break;

                            case "LED电压":
                                isLEDWarning = (LEDVoltage[i] > maxValue) || (LEDVoltage[i] < minValue) ? true : false;
                                break;

                            case "LED功率":
                                isLEDWarning = (LEDPower[i] > maxValue) || (LEDPower[i] < minValue) ? true : false;
                                break;
                        }

                        if (isLEDWarning)
                        {
                            isUpdate = (btnList[i].BackColor.ToArgb().Equals(Color.Purple.ToArgb())) ? false : true;
                        }
                        else
                        {
                            isUpdate = (btnList[i].BackColor.ToArgb().Equals(Color.Transparent.ToArgb())) ? false : true;
                        }

                        if (isUpdate)
                        {
                            if (isLEDWarning)
                            {
                                btnList[i].BackColor = Color.Purple;
                            }
                            else
                            {
                                btnList[i].BackColor = Color.Transparent;
                            }

                            btnList[i].Refresh();
                        }
                    }
                }
                catch
                {
                    // show status
                    _view.toolStripLEDStatusText = "获取LED状态失败";
                }
            }
        }

        private void OnHandleDimGreenLED(object sender, EventDimLEDArgs e)
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
                    OnSetDimGreenLED(sender, e);
                }
                else
                {
                    // Send control cmd
                    OnCloseDimGreenLED(sender, e);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }
        }

        private void OnHandleDimRedLED(object sender, EventDimLEDArgs e)
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
                    OnSetDimRedLED(sender, e);
                }
                else
                {
                    // Send control cmd
                    OnCloseDimRedLED(sender, e);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }
        }

        private void OnHandleDimDarkRedLED(object sender, EventDimLEDArgs e)
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
                    OnSetDimDarkRedLED(sender, e);
                }
                else
                {
                    // Send control cmd
                    OnCloseDimDarkRedLED(sender, e);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }
        }

        private void OnHandleFixGreenLED(object sender, EventLEDArgs e)
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
                    OnOpenFixGreenLED(sender, e);
                }
                else
                {
                    OnCloseFixGreenLED(sender, e);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }
        }

        private void OnHandleFixRedLED(object sender, EventLEDArgs e)
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
                    OnOpenFixRedLED(sender, e);
                }
                else
                {
                    OnCloseFixRedLED(sender, e);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }
        }

        private void OnHandleFixDarkRedLED(object sender, EventLEDArgs e)
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
                    OnOpenFixDarkRedLED(sender, e);
                }
                else
                {
                    OnCloseFixDarkRedLED(sender, e);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }
        }

        private void OnUpdateDimGreenLEDTbx(object sender, EventDimLEDArgs e)
        {
            // Update LED Power setting text box
            TrackBar tbar = sender as TrackBar;

            double dimLEDPower = CalcDimLEDPower(e.LEDPower, e.LEDIndex, LEDConfig.addrPLCGreenLED);
            string LEDTbxText = Convert.ToString(dimLEDPower);

            TextBox tbx;
            tbx = (TextBox)(this._view.Controls.Find($"tbxDimGreenLED{e.LEDIndex}", true)[0]);
            tbx.Text = LEDTbxText;
        }

        private void OnUpdateDimRedLEDTbx(object sender, EventDimLEDArgs e)
        {
            // Update LED Power setting text box
            TrackBar tbar = sender as TrackBar;

            double dimLEDPower = CalcDimLEDPower(e.LEDPower, e.LEDIndex, LEDConfig.addrPLCRedLED);
            string LEDTbxText = Convert.ToString(dimLEDPower);

            TextBox tbx;
            tbx = (TextBox)(this._view.Controls.Find($"tbxDimRedLED{e.LEDIndex}", true)[0]);
            tbx.Text = LEDTbxText;
        }

        private void OnUpdateDimDarkRedLEDTbx(object sender, EventDimLEDArgs e)
        {
            // Update LED Power setting text box
            TrackBar tbar = sender as TrackBar;

            double dimLEDPower = CalcDimLEDPower(e.LEDPower, e.LEDIndex, LEDConfig.addrPLCDarkRedLED);
            string LEDTbxText = Convert.ToString(dimLEDPower);

            TextBox tbx;
            tbx = (TextBox)(this._view.Controls.Find($"tbxDimDarkRedLED{e.LEDIndex}", true)[0]);
            tbx.Text = LEDTbxText;
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

        private void OnUpdateGreenScrollBar(object sender, EventDimLEDArgs e)
        {
            // Convert string to scroll bar value
            TextBox tbx = sender as TextBox;

            int sbarValue = CalcSbarValue(e.LEDPower, e.LEDIndex, LEDConfig.addrPLCGreenLED);
            TrackBar tbar;
            tbar = (TrackBar)(this._view.Controls.Find($"sbarDimGreenLED{e.LEDIndex}", true)[0]);
            tbar.Value = sbarValue;
        }

        private void OnUpdateRedScrollBar(object sender, EventDimLEDArgs e)
        {
            // Convert string to scroll bar value
            TextBox tbx = sender as TextBox;

            int sbarValue = CalcSbarValue(e.LEDPower, e.LEDIndex, LEDConfig.addrPLCRedLED);
            TrackBar tbar;
            tbar = (TrackBar)(this._view.Controls.Find($"sbarDimRedLED{e.LEDIndex}", true)[0]);
            tbar.Value = sbarValue;
        }

        private void OnUpdateDarkRedScrollBar(object sender, EventDimLEDArgs e)
        {
            // Convert string to scroll bar value
            TextBox tbx = sender as TextBox;

            int sbarValue = CalcSbarValue(e.LEDPower, e.LEDIndex, LEDConfig.addrPLCDarkRedLED);
            TrackBar tbar;
            tbar = (TrackBar)(this._view.Controls.Find($"sbarDimDarkRedLED{e.LEDIndex}", true)[0]);
            tbar.Value = sbarValue;
        }

        private int CalcSbarValue(double LEDPower, int LEDIndex, int addrPLC)
        {
            // Calculate scroll bar value for Dimmable LED
            int sbarValue = 0;

            if (addrPLC == LEDConfig.addrPLCRedLED)
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
            }
            else if (addrPLC == LEDConfig.addrPLCDarkRedLED)
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
            }
            else if (addrPLC == LEDConfig.addrPLCGreenLED)
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
            }
            else
            {
                logger.Error("Wrong PLC number");
            }

            return sbarValue;
        }

        private void OnSetDimGreenLED(object sender, EventDimLEDArgs e)
        {
            CheckBox cbxGreenLEDMainSwitch = (CheckBox)(this._view.Controls.Find("cbxGreenLEDMainSwitch", true)[0]);
            
            if ((!cbxGreenLEDMainSwitch.Checked) || (!connector.isAlive))
            {
                return;
            }

            // Turn on Dimmable LED
            int LEDPowerBit = (Int16)(CalcSbarValue(e.LEDPower, e.LEDIndex, LEDConfig.addrPLCGreenLED) / (double)NumScrollBarLevel * 1000);
            try
            {
                connector.SetDimLED(LEDConfig.addrPLCGreenLED, e.LEDIndex, LEDPowerBit);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }

            ShowSendStatusAsync();

            Button btn = null;
            Color btnColor = Color.Gray;
            btn = (Button)(this._view.Controls.Find($"btnDimGreenLED{e.LEDIndex}", true)[0]);
            btnColor = Color.Green;

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

        private void OnSetDimRedLED(object sender, EventDimLEDArgs e)
        {
            CheckBox cbxRedLEDMainSwitch = (CheckBox)(this._view.Controls.Find("cbxRedLEDMainSwitch", true)[0]);

            if ((!cbxRedLEDMainSwitch.Checked) || (!connector.isAlive))
            {
                return;
            }

            // Turn on Dimmable LED
            int LEDPowerBit = (Int16)(CalcSbarValue(e.LEDPower, e.LEDIndex, LEDConfig.addrPLCRedLED) / (double)NumScrollBarLevel * 1000);
            try
            {
                connector.SetDimLED(LEDConfig.addrPLCRedLED, e.LEDIndex, LEDPowerBit);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }

            ShowSendStatusAsync();

            Button btn = null;
            Color btnColor = Color.Gray;
            btn = (Button)(this._view.Controls.Find($"btnDimRedLED{e.LEDIndex}", true)[0]);
            btnColor = Color.Red;

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

        private void OnSetDimDarkRedLED(object sender, EventDimLEDArgs e)
        {
            CheckBox cbxDarkRedLEDMainSwitch = (CheckBox)(this._view.Controls.Find("cbxDarkRedLEDMainSwitch", true)[0]);
            
            if ((!cbxDarkRedLEDMainSwitch.Checked) || (!connector.isAlive))
            {
                return;
            }

            // Turn on Dimmable LED
            int LEDPowerBit = (Int16)(CalcSbarValue(e.LEDPower, e.LEDIndex, LEDConfig.addrPLCDarkRedLED) / (double)NumScrollBarLevel * 1000);
            try
            {
                connector.SetDimLED(LEDConfig.addrPLCDarkRedLED, e.LEDIndex, LEDPowerBit);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }

            ShowSendStatusAsync();

            Button btn = null;
            Color btnColor = Color.Gray;
            btn = (Button)(this._view.Controls.Find($"btnDimDarkRedLED{e.LEDIndex}", true)[0]);
            btnColor = Color.DarkRed;

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

        private void OnCloseDimGreenLED(object sender, EventDimLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn off Dimmable LED
            try
            {
                connector.TurnOffDimLED(LEDConfig.addrPLCGreenLED, e.LEDIndex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }

            ShowSendStatusAsync();

            // Change color
            Button btn = sender as Button;
            btn.BackColor = Color.Gray;
            btn.Refresh();
        }

        private void OnCloseDimRedLED(object sender, EventDimLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn off Dimmable LED
            try
            {
                connector.TurnOffDimLED(LEDConfig.addrPLCRedLED, e.LEDIndex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }

            ShowSendStatusAsync();

            // Change color
            Button btn = sender as Button;
            btn.BackColor = Color.Gray;
            btn.Refresh();
        }

        private void OnCloseDimDarkRedLED(object sender, EventDimLEDArgs e)
        {
            if (!connector.isAlive)
            {
                return;
            }

            // Turn off Dimmable LED
            try
            {
                connector.TurnOffDimLED(LEDConfig.addrPLCDarkRedLED, e.LEDIndex);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                TextBox tbxConnectMsg = (TextBox)(this._view.Controls.Find("tbxConnectMsg", true)[0]);
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }

            ShowSendStatusAsync();

            // Change color
            Button btn = sender as Button;
            btn.BackColor = Color.Gray;
            btn.Refresh();
        }

        private void OnOpenFixGreenLED(object sender, EventLEDArgs e)
        {
            CheckBox cbxGreenLEDMainSwitch = (CheckBox)(this._view.Controls.Find("cbxGreenLEDMainSwitch", true)[0]);

            if (connector.isAlive && cbxGreenLEDMainSwitch.Checked)
            {
                // Turn on Fix LED
                connector.TurnOnFixLED(LEDConfig.addrPLCGreenLED, e.LEDIndex);
                ShowSendStatusAsync();

                // set button color
                Button btn = sender as Button;
                btn.BackColor = Color.Green;
                btn.Refresh();
            }
        }

        private void OnOpenFixRedLED(object sender, EventLEDArgs e)
        {
            CheckBox cbxRedLEDMainSwitch = (CheckBox)(this._view.Controls.Find("cbxRedLEDMainSwitch", true)[0]);

            if (connector.isAlive && cbxRedLEDMainSwitch.Checked)
            {
                // Turn on Fix LED
                connector.TurnOnFixLED(LEDConfig.addrPLCRedLED, e.LEDIndex);
                ShowSendStatusAsync();

                // set button color
                Button btn = sender as Button;
                btn.BackColor = Color.Red;
                btn.Refresh();
            }
        }

        private void OnOpenFixDarkRedLED(object sender, EventLEDArgs e)
        {
            CheckBox cbxDarkRedLEDMainSwitch = (CheckBox)(this._view.Controls.Find("cbxDarkRedLEDMainSwitch", true)[0]);

            if (connector.isAlive && cbxDarkRedLEDMainSwitch.Checked)
            {
                // Turn on Fix LED
                connector.TurnOnFixLED(LEDConfig.addrPLCDarkRedLED, e.LEDIndex);
                ShowSendStatusAsync();

                // set button color
                Button btn = sender as Button;
                btn.BackColor = Color.DarkRed;
                btn.Refresh();
            }
        }

        private void OnCloseFixGreenLED(object sender, EventLEDArgs e)
        {
            // Turn off Fix LED
            if (connector.isAlive)
            {
                connector.TurnOffFixLED(LEDConfig.addrPLCGreenLED, e.LEDIndex);

                ShowSendStatusAsync();

                // Change color
                Button btn = sender as Button;
                btn.BackColor = Color.Gray;
                btn.Refresh();
            }
        }

        private void OnCloseFixRedLED(object sender, EventLEDArgs e)
        {
            // Turn off Fix LED
            if (connector.isAlive)
            {
                connector.TurnOffFixLED(LEDConfig.addrPLCRedLED, e.LEDIndex);

                ShowSendStatusAsync();

                // Change color
                Button btn = sender as Button;
                btn.BackColor = Color.Gray;
                btn.Refresh();
            }
        }

        private void OnCloseFixDarkRedLED(object sender, EventLEDArgs e)
        {
            // Turn off Fix LED
            if (connector.isAlive)
            {
                connector.TurnOffFixLED(LEDConfig.addrPLCDarkRedLED, e.LEDIndex);

                ShowSendStatusAsync();

                // Change color
                Button btn = sender as Button;
                btn.BackColor = Color.Gray;
                btn.Refresh();
            }
        }

        private void OnShowFixGreenLEDStatus(object sender, EventLEDArgs e)
        {
            // Show LED status
            QueryType qType = QueryType.FixGreenLED;

            if (this.connector.isAlive)
            {
                try
                {
                    // parsing status
                    LEDStatus thisLEDStatus = this.connector.QueryLEDStatus(e.LEDIndex, qType);

                    if (thisLEDStatus.isValidStatus)
                    {
                        this._view.toolStripLEDStatusText = $"电压:{string.Format("{0,6:F2}", thisLEDStatus.LEDVoltage)} V | 电流: {string.Format("{0,6:F3}", thisLEDStatus.LEDCurrent)} A | 功率: {string.Format("{0,6:F3}", thisLEDStatus.LEDPower)} V";
                    }
                    else
                    {
                        logger.Warn("Failure in requesting Green LED status");
                        _view.toolStripLEDStatusText = "获取LED状态失败";
                    }
                }
                catch
                {
                    logger.Warn("Failure in requesting Green LED status");
                    _view.toolStripLEDStatusText = "获取LED状态失败";
                }
            }
            else
            {
                logger.Warn("Connection break");
                _view.toolStripLEDStatusText = "获取LED状态失败";
            }
        }

        private void OnShowFixRedLEDStatus(object sender, EventLEDArgs e)
        {
            // Show LED status
            QueryType qType = QueryType.FixRedLED;

            if (this.connector.isAlive)
            {
                try
                {
                    // parsing status
                    LEDStatus thisLEDStatus = this.connector.QueryLEDStatus(e.LEDIndex, qType);

                    if (thisLEDStatus.isValidStatus)
                    {
                        this._view.toolStripLEDStatusText = $"电压:{string.Format("{0,6:F2}", thisLEDStatus.LEDVoltage)} V | 电流: {string.Format("{0,6:F3}", thisLEDStatus.LEDCurrent)} A | 功率: {string.Format("{0,6:F3}", thisLEDStatus.LEDPower)} V";
                    }
                    else
                    {
                        logger.Warn("Failure in requesting Red LED status");
                        _view.toolStripLEDStatusText = "获取LED状态失败";
                    }
                }
                catch
                {
                    logger.Warn("Failure in requesting Red LED status");
                    _view.toolStripLEDStatusText = "获取LED状态失败";
                }
            }
            else
            {
                logger.Warn("Connection break");
                _view.toolStripLEDStatusText = "获取LED状态失败";
            }
        }

        private void OnShowFixDarkRedLEDStatus(object sender, EventLEDArgs e)
        {
            // Show LED status
            QueryType qType = QueryType.FixDarkRedLED;

            if (this.connector.isAlive)
            {
                try
                {
                    // parsing status
                    LEDStatus thisLEDStatus = this.connector.QueryLEDStatus(e.LEDIndex, qType);

                    if (thisLEDStatus.isValidStatus)
                    {
                        this._view.toolStripLEDStatusText = $"电压:{string.Format("{0,6:F2}", thisLEDStatus.LEDVoltage)} V | 电流: {string.Format("{0,6:F3}", thisLEDStatus.LEDCurrent)} A | 功率: {string.Format("{0,6:F3}", thisLEDStatus.LEDPower)} V";
                    }
                    else
                    {
                        logger.Warn("Failure in requesting Infrared LED status");
                        _view.toolStripLEDStatusText = "获取LED状态失败";
                    }
                }
                catch
                {
                    logger.Warn("Failure in requesting Infrared LED status");
                    _view.toolStripLEDStatusText = "获取LED状态失败";
                }
            }
            else
            {
                logger.Warn("Connection break");
                _view.toolStripLEDStatusText = "获取LED状态失败";
            }
        }

        private void OnShowDimGreenLEDStatus(object sender, EventDimLEDArgs e)
        {
            // Show LED status
            QueryType qType = QueryType.DimGreenLED;

            if (this.connector.isAlive)
            {
                try
                {
                    // parsing status
                    LEDStatus thisLEDStatus = this.connector.QueryLEDStatus(e.LEDIndex, qType);

                    if (thisLEDStatus.isValidStatus)
                    {
                        this._view.toolStripLEDStatusText = $"电压:{string.Format("{0,6:F2}", thisLEDStatus.LEDVoltage)} V | 电流: {string.Format("{0,6:F3}", thisLEDStatus.LEDCurrent)} A | 功率: {string.Format("{0,6:F3}", thisLEDStatus.LEDPower)} V";
                    }
                    else
                    {
                        logger.Warn("Failure in requesting Green LED status");
                        _view.toolStripLEDStatusText = "获取LED状态失败";
                    }
                }
                catch
                {
                    logger.Warn("Failure in requesting Green LED status");
                    _view.toolStripLEDStatusText = "获取LED状态失败";
                }
            }
            else
            {
                logger.Warn("Connection break");
                _view.toolStripLEDStatusText = "获取LED状态失败";
            }
        }

        private void OnShowDimRedLEDStatus(object sender, EventDimLEDArgs e)
        {
            // Show LED status
            QueryType qType = QueryType.DimRedLED;

            if (this.connector.isAlive)
            {
                try
                {
                    // parsing status
                    LEDStatus thisLEDStatus = this.connector.QueryLEDStatus(e.LEDIndex, qType);

                    if (thisLEDStatus.isValidStatus)
                    {
                        this._view.toolStripLEDStatusText = $"电压:{string.Format("{0,6:F2}", thisLEDStatus.LEDVoltage)} V | 电流: {string.Format("{0,6:F3}", thisLEDStatus.LEDCurrent)} A | 功率: {string.Format("{0,6:F3}", thisLEDStatus.LEDPower)} V";
                    }
                    else
                    {
                        logger.Warn("Failure in requesting Red LED status");
                        _view.toolStripLEDStatusText = "获取LED状态失败";
                    }
                }
                catch
                {
                    logger.Warn("Failure in requesting Red LED status");
                    _view.toolStripLEDStatusText = "获取LED状态失败";
                }
            }
            else
            {
                logger.Warn("Connection break");
                _view.toolStripLEDStatusText = "获取LED状态失败";
            }
        }

        private void OnShowDimDarkRedLEDStatus(object sender, EventDimLEDArgs e)
        {
            // Show LED status
            QueryType qType = QueryType.DimDarkRedLED;

            if (this.connector.isAlive)
            {
                try
                {
                    // parsing status
                    LEDStatus thisLEDStatus = this.connector.QueryLEDStatus(e.LEDIndex, qType);

                    if (thisLEDStatus.isValidStatus)
                    {
                        this._view.toolStripLEDStatusText = $"电压:{string.Format("{0,6:F2}", thisLEDStatus.LEDVoltage)} V | 电流: {string.Format("{0,6:F3}", thisLEDStatus.LEDCurrent)} A | 功率: {string.Format("{0,6:F3}", thisLEDStatus.LEDPower)} V";
                    }
                    else
                    {
                        logger.Warn("Failure in requesting Infrared LED status");
                        _view.toolStripLEDStatusText = "获取LED状态失败";
                    }
                }
                catch
                {
                    logger.Warn("Failure in requesting Infrared LED status");
                    _view.toolStripLEDStatusText = "获取LED状态失败";
                }
            }
            else
            {
                logger.Warn("Connection break");
                _view.toolStripLEDStatusText = "获取LED状态失败";
            }
        }

        private void OnClearFixLEDStatus(object sender, EventLEDArgs e)
        {
            // Clear LED status
            _view.toolStripLEDStatusText = "LED状态";
        }

        private void OnClearDimLEDStatus(object sender, EventDimLEDArgs e)
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
                logger.Error(ex.ToString());
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + ex.ToString() + "\r\n");
            }
        }

        public void ReceiveMsg(object sender, DoWorkEventArgs args)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            byte[] buffer;

            RadioButton rbnRecHEX = (RadioButton)(this._view.Controls.Find("rbnRecHEX", true)[0]);
            while (true)   // Receiving message from slave
            {
                if (this.connector.isAlive)
                {
                    try
                    {
                        buffer = this.connector.device.ReadMsg(rbnRecHEX.Checked);
                    }
                    catch
                    {
                        buffer = new byte[0];
                    }
                }
                else
                {
                    buffer = new byte[0];
                    args.Cancel = true;
                    break;
                }

                if (buffer.Length > 0)
                {
                    try
                    {
                        //currentLEDStatus = connector.ParsePackage(buffer);
                        receiveBytes = buffer;
                        // TODO add timestamp

                        // showing receiving status with colorful light
                        worker.ReportProgress(1);
                    }
                    catch
                    {
                        logger.Error("Failure in receiving message");
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
                connector.Connect(200);
                ShowSendStatusAsync();
                _view.toolStripConnectionStatusText = "连接成功";
                // show connection message
                tbxConnectMsg.AppendText("[" + GetCurrentTime() + "]" + " 连接成功\r\n");
                btnOpenTCP.BackColor = Color.Green;
                btnCloseTCP.BackColor = Color.Empty;
                btnSendTestMsg.Enabled = true;
            }
            catch (Exception ex)
            {
                logger.Error("Failure in setting up connection");
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
            Button btnStartReceive = (Button)(this._view.Controls.Find("btnStartReceive", true)[0]);

            // Close connection
            try
            {
                connector.Disconnect();
                if (recWorker.IsBusy)
                {
                    recWorker.CancelAsync();
                }
                btnStartReceive.BackColor = Color.Transparent;
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
                logger.Error("Failure in disconnection");
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
