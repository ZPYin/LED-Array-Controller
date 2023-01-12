using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace LEDController.View
{
    public partial class LEDControllerViewer : Form
    {

        public LEDControllerViewer()
        {
            InitializeComponent();
            this.KeyPreview = true;
        }

        public event EventHandler<EventDimLEDArgs> ClearDimGreenLEDStatus;
        public event EventHandler<EventDimLEDArgs> ClearDimRedLEDStatus;
        public event EventHandler<EventDimLEDArgs> ClearDimDarkRedLEDStatus;
        public event EventHandler<EventLEDArgs> ClearFixGreenLEDStatus;
        public event EventHandler<EventLEDArgs> ClearFixRedLEDStatus;
        public event EventHandler<EventLEDArgs> ClearFixDarkRedLEDStatus;
        public event EventHandler<EventDimLEDArgs> CloseDimGreenLED;
        public event EventHandler<EventDimLEDArgs> CloseDimRedLED;
        public event EventHandler<EventDimLEDArgs> CloseDimDarkRedLED;
        public event EventHandler<EventLEDArgs> CloseFixGreenLED;
        public event EventHandler<EventLEDArgs> CloseFixRedLED;
        public event EventHandler<EventLEDArgs> CloseFixDarkRedLED;
        public event EventHandler<EventArgs> CloseSerialPort;
        public event EventHandler<EventArgs> CloseTCP;
        public event EventHandler<EventArgs> ConnectSerialPort;
        public event EventHandler<EventArgs> ConnectTCP;
        public event EventHandler<EventDimLEDArgs> HandleDimGreenLED;
        public event EventHandler<EventDimLEDArgs> HandleDimRedLED;
        public event EventHandler<EventDimLEDArgs> HandleDimDarkRedLED;
        public event EventHandler<EventLEDArgs> HandleFixGreenLED;
        public event EventHandler<EventLEDArgs> HandleFixRedLED;
        public event EventHandler<EventLEDArgs> HandleFixDarkRedLED;
        public event EventHandler<EventLEDArgs> OpenFixGreenLED;
        public event EventHandler<EventLEDArgs> OpenFixRedLED;
        public event EventHandler<EventLEDArgs> OpenFixDarkRedLED;
        public event EventHandler<EventArgs> OpenWithCfgFile;
        public event EventHandler<EventArgs> SaveAsCfgFile;
        public event EventHandler<EventArgs> SaveCfgFile;
        public event EventHandler<EventArgs> SendTestData;
        public event EventHandler<EventDimLEDArgs> SetDimGreenLED;
        public event EventHandler<EventDimLEDArgs> SetDimRedLED;
        public event EventHandler<EventDimLEDArgs> SetDimDarkRedLED;
        public event EventHandler<EventDimLEDArgs> ShowDimGreenLEDStatus;
        public event EventHandler<EventDimLEDArgs> ShowDimRedLEDStatus;
        public event EventHandler<EventDimLEDArgs> ShowDimDarkRedLEDStatus;
        public event EventHandler<EventLEDArgs> ShowFixGreenLEDStatus;
        public event EventHandler<EventLEDArgs> ShowFixRedLEDStatus;
        public event EventHandler<EventLEDArgs> ShowFixDarkRedLEDStatus;
        public event EventHandler<EventArgs> ShowChillerStatus;
        public event EventHandler<EventArgs> TurnOnChiller;
        public event EventHandler<EventArgs> TurnOffChiller;
        public event EventHandler<EventArgs> ShowLEDStatus;
        public event EventHandler<EventArgs> ShowVersion;
        public event EventHandler<EventArgs> TurnOffLight;
        public event EventHandler<EventArgs> TurnOnLighMainSwitch;
        public event EventHandler<EventArgs> TurnOffLighMainSwitch;
        public event EventHandler<EventSkyLightArgs> TurnOffSkyLight;
        public event EventHandler<EventArgs> TurnOnLight;
        public event EventHandler<EventSkyLightArgs> TurnOnSkyLight;
        public event EventHandler<EventDimLEDArgs> UpdateDimGreenLEDTbx;
        public event EventHandler<EventDimLEDArgs> UpdateDimRedLEDTbx;
        public event EventHandler<EventDimLEDArgs> UpdateDimDarkRedLEDTbx;
        public event EventHandler<EventDimLEDArgs> UpdateGreenScrollBar;
        public event EventHandler<EventDimLEDArgs> UpdateRedScrollBar;
        public event EventHandler<EventDimLEDArgs> UpdateDarkRedScrollBar;
        public event EventHandler<EventArgs> SelectStatusDataSaveFolder;
        public event EventHandler<EventArgs> ChangeStatusDataSaveFolder;
        public event EventHandler<EventArgs> TurnOnRTPower;
        public event EventHandler<EventArgs> TurnOffRTPower;
        public event EventHandler<EventArgs> TurnOnAirConditionerPower;
        public event EventHandler<EventArgs> TurnOffAirConditionerPower;
        public event EventHandler<EventArgs> TurnOnCamPower;
        public event EventHandler<EventArgs> TurnOffCamPower;
        public event EventHandler<EventArgs> TurnOnPCPower;
        public event EventHandler<EventArgs> TurnOffPCPower;
        public event EventHandler<EventArgs> StartReceive;
        public event EventHandler<EventArgs> StopReceive;
        public event EventHandler<EventArgs> ChangeTabIndex;
        public event EventHandler<EventArgs> ChangeGreenLEDMainSwitch;
        public event EventHandler<EventArgs> ChangeRedLEDMainSwitch;
        public event EventHandler<EventArgs> ChangeDarkRedLEDMainSwitch;
        public event EventHandler<EventArgs> ChangeMinValue;
        public event EventHandler<EventArgs> ChangeMaxValue;
        public event EventHandler<EventArgs> ChangeMinNormValue;
        public event EventHandler<EventArgs> ChangeMaxNormValue;

        private Size m_szInit;   //初始窗体大小
        private Dictionary<Control, Rectangle> m_dicSize = new Dictionary<Control, Rectangle>();   // store control sizes

        public int queryParamSelectItem
        {
            get { return cbxQueryParam.SelectedIndex; }
            set { cbxQueryParam.SelectedIndex = value; }
        }
        public int queryWaitTimeSelectItem 
        {
            get { return cbxQueryWaitTime.SelectedIndex; }
            set { cbxQueryWaitTime.SelectedIndex = value; }
        }
        public Color btnRecStatus1Color
        {
            get { return btnRecStatus1.BackColor; }
            set 
            { 
                btnRecStatus1.BackColor = value; 
                btnRecStatus1.Refresh();
            }
        }
        public Color btnRecStatus2Color
        {
            get { return btnRecStatus2.BackColor; }
            set 
            { 
                btnRecStatus2.BackColor = value; 
                btnRecStatus2.Refresh(); 
            }
        }
        public Color btnRecStatus3Color
        {
            get { return btnRecStatus3.BackColor; }
            set 
            { 
                btnRecStatus3.BackColor = value; 
                btnRecStatus3.Refresh(); 
            }
        }
        public Color btnSendStatus1Color
        {
            get { return btnSendStatus1.BackColor; }
            set 
            { 
                btnSendStatus1.BackColor = value; 
                btnSendStatus1.Refresh(); 
            }
        }
        public Color btnSendStatus2Color
        {
            get { return btnSendStatus2.BackColor; }
            set 
            { 
                btnSendStatus2.BackColor = value; 
                btnSendStatus2.Refresh(); 
            }
        }
        public Color btnSendStatus3Color
        {
            get { return btnSendStatus3.BackColor; }
            set 
            { 
                btnSendStatus3.BackColor = value; 
                btnSendStatus3.Refresh(); 
            }
        }

        // public Color[] LEDStatusColors
        // {
        //     get 
        //     {
        //         List<Control> list = new List<Control>();
        //         GetAllControl(this.panelLEDStatus, list, "LED");
        //         Color[] LEDColors = new Color[list.Count()];
                
        //         foreach (Control control in list)
        //         {
        //             if (control.GetType() == typeof(Button))
        //             {
        //                 LEDColors[Convert.ToInt32(control.Tag) - 1] = control.BackColor;
        //             }
        //         }

        //         return LEDColors;
        //     }
        //     set 
        //     {
        //         List<Control> list = new List<Control>();
        //         GetAllControl(this.panelLEDStatus, list, "LED");

        //         foreach (Control control in list)
        //         {
        //             if (control.GetType() == typeof(Button))
        //             {
        //                 control.BackColor = value[Convert.ToInt32(control.Tag) - 1];
        //             }
        //         }
        //     }
        // }

        public string toolStripConnectionStatusText
        {
            get { return toolStripConnectionStatus.Text; }
            set { toolStripConnectionStatus.Text = value; }
        }

        public string toolStripLEDStatusText
        {
            get { return toolStripLEDStatus.Text; }
            set { toolStripLEDStatus.Text = value; }
        }

        private void btnSendTestMsg_Click(object sender, EventArgs e)
        {
            // Click send test data button
            SendTestData?.Invoke(sender, e);
        }

        private void CloseApplication(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void GetAllControl(Control c, List<Control> list)
        {
            // Get all controls
            foreach (Control control in c.Controls)
            {
                list.Add(control);

                if (control.GetType() == typeof(Panel))
                {
                    GetAllControl(control, list);
                }
            }
        }

        public void GetAllControl(Control c, List<Control> list, string substr)
        {
            // Get all controls
            foreach (Control control in c.Controls)
            {
                if (control.Name.Contains(substr))
                {
                    list.Add(control);
                }

                if (control.GetType() == typeof(Panel))
                {
                    GetAllControl(control, list);
                }
            }
        }

        public int GetLEDIndex(string source, string value)
        {
            return Convert.ToInt16(source.Substring(EndIndexOf(source, value)));
        }

        public int EndIndexOf(string source, string value)
        {
            int index = source.IndexOf(value);
            if (index >= 0)
            {
                index += value.Length;
            }

            return index;
        }

        private void btnOpenGreenFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Green Fix LEDs
            this.Cursor = Cursors.WaitCursor;
            List<Control> list = new List<Control>();
            GetAllControl(this.panelGreenFixLED, list, "btnGreenLED");

            for (int i = 0; i < list.Count(); i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(panelGreenFixLED.Controls.Find($"btnGreenLED{LEDIndex}", true)[0]);
                OpenFixGreenLED?.Invoke(btn, new EventLEDArgs(LEDIndex));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseGreenFixLED_Click(object sender, EventArgs e)
        {
            // Turn off all Green LEDs
            this.Cursor = Cursors.WaitCursor;
            List<Control> greenLEDList = new List<Control>();
            GetAllControl(this.panelGreenFixLED, greenLEDList, "btnGreenLED");

            for (int i = 0; i < greenLEDList.Count(); i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(panelGreenFixLED.Controls.Find($"btnGreenLED{LEDIndex}", true)[0]);
                CloseFixGreenLED?.Invoke(btn, new EventLEDArgs(LEDIndex));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnOpenRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Fix LEDs
            this.Cursor = Cursors.WaitCursor;

            List<Control> redLEDList = new List<Control>();
            GetAllControl(this.panelRedFixLED, redLEDList, "btnRedLED");

            for (int i = 0; i < redLEDList.Count(); i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(panelRedFixLED.Controls.Find($"btnRedLED{LEDIndex}", true)[0]);
                OpenFixRedLED?.Invoke(btn, new EventLEDArgs(LEDIndex));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Fix LEDs
            this.Cursor = Cursors.WaitCursor;

            List<Control> redLEDList = new List<Control>();
            GetAllControl(this.panelRedFixLED, redLEDList, "btnRedLED");

            for (int i = 0; i < redLEDList.Count(); i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(panelRedFixLED.Controls.Find($"btnRedLED{LEDIndex}", true)[0]);
                CloseFixRedLED?.Invoke(btn, new EventLEDArgs(LEDIndex));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnOpenDarkRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Fix LEDs
            this.Cursor = Cursors.WaitCursor;

            List<Control> darkRedLEDList = new List<Control>();
            GetAllControl(this.panelDarkRedFixLED, darkRedLEDList, "btnDarkRedLED");

            for (int i = 0; i < darkRedLEDList.Count(); i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(panelDarkRedFixLED.Controls.Find($"btnDarkRedLED{LEDIndex}", true)[0]);
                OpenFixDarkRedLED?.Invoke(btn, new EventLEDArgs(LEDIndex));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseDarkRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Fix LEDs
            this.Cursor = Cursors.WaitCursor;

            List<Control> darkRedLEDList = new List<Control>();
            GetAllControl(this.panelDarkRedFixLED, darkRedLEDList, "btnDarkRedLED");

            for (int i = 0; i < darkRedLEDList.Count(); i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(panelDarkRedFixLED.Controls.Find($"btnDarkRedLED{LEDIndex}", true)[0]);
                CloseFixDarkRedLED?.Invoke(btn, new EventLEDArgs(LEDIndex));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnOpenGreenDimLED_Click(object sender, EventArgs e)
        {
            // Turn on all Green Dimmable LED
            this.Cursor = Cursors.WaitCursor;

            List<Control> dimGreenLEDList = new List<Control>();
            GetAllControl(this.panelDimGreenLED, dimGreenLEDList, "btnDimGreenLED");

            for (int i = 0; i < dimGreenLEDList.Count(); i++)
            {
                Button btn = (Button)(this.Controls.Find($"btnDimGreenLED{i + 1}", true)[0]);
                int idxLED = GetLEDIndex(btn.Name, "btnDimGreenLED");
                TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimGreenLED{idxLED}", true)[0]);

                SetDimGreenLED?.Invoke(btn, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseGreenDimLED_Click(object sender, EventArgs e)
        {
            // Turn off all Green Dimmable LED
            this.Cursor = Cursors.WaitCursor;

            List<Control> dimGreenLEDList = new List<Control>();
            GetAllControl(this.panelDimGreenLED, dimGreenLEDList, "btnDimGreenLED");

            for (int i = 0; i < dimGreenLEDList.Count(); i++)
            {
                Button btn = (Button)(this.Controls.Find($"btnDimGreenLED{i + 1}", true)[0]);
                int idxLED = GetLEDIndex(btn.Name, "btnDimGreenLED");
                TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimGreenLED{idxLED}", true)[0]);

                CloseDimGreenLED?.Invoke(btn, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnOpenRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Dimmable LED
            this.Cursor = Cursors.WaitCursor;

            List<Control> dimRedLEDList = new List<Control>();
            GetAllControl(this.panelDimRedLED, dimRedLEDList, "btnDimRedLED");

            for (int i = 0; i < dimRedLEDList.Count(); i++)
            {
                Button btn = (Button)(this.Controls.Find($"btnDimRedLED{i + 1}", true)[0]);
                int idxLED = GetLEDIndex(btn.Name, "btnDimRedLED");
                TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimRedLED{idxLED}", true)[0]);

                SetDimRedLED?.Invoke(btn, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn off all Red Dimmable LED
            this.Cursor = Cursors.WaitCursor;

            List<Control> dimRedLEDList = new List<Control>();
            GetAllControl(this.panelDimRedLED, dimRedLEDList, "btnDimRedLED");

            for (int i = 0; i < dimRedLEDList.Count(); i++)
            {
                Button btn = (Button)(this.Controls.Find($"btnDimRedLED{i + 1}", true)[0]);
                int idxLED = GetLEDIndex(btn.Name, "btnDimRedLED");
                TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimRedLED{idxLED}", true)[0]);

                CloseDimRedLED?.Invoke(btn, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnOpenDarkRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn on all DarkRed Dimmable LED
            this.Cursor = Cursors.WaitCursor;

            List<Control> dimDarkRedLEDList = new List<Control>();
            GetAllControl(this.panelDimDarkRedLED, dimDarkRedLEDList, "btnDimDarkRedLED");

            for (int i = 0; i < dimDarkRedLEDList.Count(); i++)
            {
                Button btn = (Button)(this.Controls.Find($"btnDimDarkRedLED{i + 1}", true)[0]);
                int idxLED = GetLEDIndex(btn.Name, "btnDimDarkRedLED");
                TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimDarkRedLED{idxLED}", true)[0]);

                SetDimDarkRedLED?.Invoke(btn, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseDarkRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn off all DarkRed Dimmable LED
            this.Cursor = Cursors.WaitCursor;

            List<Control> dimDarkRedLEDList = new List<Control>();
            GetAllControl(this.panelDimDarkRedLED, dimDarkRedLEDList, "btnDimDarkRedLED");

            for (int i = 0; i < dimDarkRedLEDList.Count(); i++)
            {
                Button btn = (Button)(this.Controls.Find($"btnDimDarkRedLED{i + 1}", true)[0]);
                int idxLED = GetLEDIndex(btn.Name, "btnDimDarkRedLED");
                TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimDarkRedLED{idxLED}", true)[0]);

                CloseDimDarkRedLED?.Invoke(btn, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void tsmConnect_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 0;
        }

        private void tsmLEDControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 1;
            StopReceive?.Invoke(sender, e);
        }

        private void tsmLEDStatus_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 2;
            StopReceive?.Invoke(sender, e);
        }

        private void tsmChillerControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 3;
            StopReceive?.Invoke(sender, e);
        }

        private void tsmSkylightControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 4;
            StopReceive?.Invoke(sender, e);
        }

        private void tsmLightControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 5;
            StopReceive?.Invoke(sender, e);
        }

        private void tsmRTControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 6;
            StopReceive?.Invoke(sender, e);
        }

        private void tsmAirConditionerControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 7;
            StopReceive?.Invoke(sender, e);
        }

        private void tsmCameraControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 8;
            StopReceive?.Invoke(sender, e);
        }

        private void btnShowLEDStatus_Click(object sender, EventArgs e)
        {
            ShowLEDStatus?.Invoke(sender, e);
        }

        private void Open_Click(object sender, EventArgs e)
        {
            OpenWithCfgFile?.Invoke(sender, e);
        }

        private void Save_Click(object sender, EventArgs e)
        {
            SaveCfgFile?.Invoke(sender, e);
        }

        private void Saveas_Click(object sender, EventArgs e)
        {
            SaveAsCfgFile?.Invoke(sender, e);
        }

        protected void LEDControllerViewer_Load(object sender, EventArgs e)
        {
            m_szInit = this.Size;
            this.GetInitSize(this);
        }

        private void GetInitSize(Control c)
        {
            foreach (Control control in c.Controls)
            {
                m_dicSize.Add(control, new Rectangle(control.Location, control.Size));
                this.GetInitSize(control);
            }
        }

        private void LEDControllerViewer_Resize(object sender, EventArgs e)
        {
            float fx = (float)this.Width / m_szInit.Width;
            float fy = (float)this.Height / m_szInit.Height;
            foreach (var v in m_dicSize)
            {
                v.Key.Left = (int)(v.Value.Left * fx);
                v.Key.Top = (int)(v.Value.Top * fy);
                v.Key.Width = (int)(v.Value.Width * fx);
                v.Key.Height = (int)(v.Value.Height * fy);
            }

        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.baidu.com/");
        }

        private void VersionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowVersion?.Invoke(sender, e);
        }

        private void btnOpenCOM_Click(object sender, EventArgs e)
        {
            ConnectSerialPort?.Invoke(sender, e);
        }

        private void btnCloseCOM_Click(object sender, EventArgs e)
        {
            CloseSerialPort?.Invoke(sender, e);
        }

        private void btnOpenTCP_Click(object sender, EventArgs e)
        {
            ConnectTCP?.Invoke(sender, e);
        }

        private void btnCloseTCP_Click(object sender, EventArgs e)
        {
            // Click disconnect button
            CloseTCP?.Invoke(sender, e);
        }

        private void btnDarkRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDarkRedLED");
            HandleFixDarkRedLED?.Invoke(sender, new EventLEDArgs(idxLED));
        }

        private void btnDarkRedLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDarkRedLED");

            ShowFixDarkRedLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED));
        }

        private void btnDarkRedLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDarkRedLED");

            ClearFixDarkRedLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED));
        }

        private void btnGreenLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnGreenLED");
            HandleFixGreenLED?.Invoke(sender, new EventLEDArgs(idxLED));
        }

        private void btnGreenLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnGreenLED");

            ShowFixGreenLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED));
        }

        private void btnGreenLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnGreenLED");

            ClearFixGreenLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED));
        }

        private void btnRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnRedLED");
            HandleFixRedLED?.Invoke(sender, new EventLEDArgs(idxLED));
        }

        private void btnRedLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnRedLED");

            ShowFixRedLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED));
        }

        private void btnRedLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnRedLED");

            ClearFixRedLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED));
        }

        private void btnDimGreenLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimGreenLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimGreenLED{idxLED}", true)[0]);

            HandleDimGreenLED?.Invoke(sender, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
        }

        private void btnDimGreenLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimGreenLED");

            ShowDimGreenLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 0));
        }

        private void btnDimGreenLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimGreenLED");

            ClearDimGreenLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 0));
        }

        private void tbxDimGreenLED_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tbx = sender as TextBox;
                int idxLED = GetLEDIndex(tbx.Name, "tbxDimGreenLED");
                UpdateGreenScrollBar?.Invoke(sender, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
            }
        }

        private void sbarDimGreenLED_Scroll(object sender, EventArgs e)
        {
            TrackBar tbar = sender as TrackBar;
            int idxLED = GetLEDIndex(tbar.Name, "sbarDimGreenLED");
            UpdateDimGreenLEDTbx?.Invoke(sender, new EventDimLEDArgs(idxLED, (double)tbar.Value));
        }

        private void btnCfgDimGreenLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnOpenDimGreenLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimGreenLED{idxLED}", true)[0]);

            SetDimGreenLED?.Invoke(sender, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
        }

        private void btnDimDarkRedLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimDarkRedLED");

            ShowDimDarkRedLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 0));
        }

        private void tbxDimDarkRedLED_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tbx = sender as TextBox;
                int idxLED = GetLEDIndex(tbx.Name, "tbxDimDarkRedLED");
                UpdateDarkRedScrollBar?.Invoke(sender, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
            }
        }

        private void btnDimDarkRedLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimDarkRedLED");

            ClearDimDarkRedLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 0));
        }

        private void btnDimDarkRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimDarkRedLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimDarkRedLED{idxLED}", true)[0]);

            HandleDimDarkRedLED?.Invoke(sender, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
        }

        private void sbarDimDarkRedLED_Scroll(object sender, EventArgs e)
        {
            TrackBar tbar = sender as TrackBar;
            int idxLED = GetLEDIndex(tbar.Name, "sbarDimDarkRedLED");
            UpdateDimDarkRedLEDTbx?.Invoke(sender, new EventDimLEDArgs(idxLED, (double)tbar.Value));
        }

        private void btnCfgDimDarkRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnOpenDimDarkRedLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimDarkRedLED{idxLED}", true)[0]);

            SetDimDarkRedLED?.Invoke(sender, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
        }

        private void btnCfgDimRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnOpenDimRedLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimRedLED{idxLED}", true)[0]);

            SetDimRedLED?.Invoke(sender, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
        }

        private void sbarDimRedLED_Scroll(object sender, EventArgs e)
        {
            TrackBar tbar = sender as TrackBar;
            int idxLED = GetLEDIndex(tbar.Name, "sbarDimRedLED");
            UpdateDimRedLEDTbx?.Invoke(sender, new EventDimLEDArgs(idxLED, (double)tbar.Value));
        }

        private void tbxDimRedLED_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tbx = sender as TextBox;
                int idxLED = GetLEDIndex(tbx.Name, "tbxDimRedLED");
                UpdateRedScrollBar?.Invoke(sender, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
            }
        }

        private void btnDimRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimRedLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimRedLED{idxLED}", true)[0]);

            HandleDimRedLED?.Invoke(sender, new EventDimLEDArgs(idxLED, Convert.ToDouble(tbx.Text)));
        }

        private void btnDimRedLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimRedLED");

            ShowDimRedLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 0));
        }

        private void btnDimRedLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimRedLED");

            ClearDimRedLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 0));
        }

        private void btnUpdateChiller_Click(object sender, EventArgs e)
        {
            ShowChillerStatus?.Invoke(sender, e);
        }

        private void btnOpenSkylight1_Click(object sender, EventArgs e)
        {
            // TurnOnSkyLight?.Invoke(sender, new EventSkyLightArgs(1));
        }

        private void btnCloseSkylight1_Click(object sender, EventArgs e)
        {
            // TurnOffSkyLight?.Invoke(sender, new EventSkyLightArgs(1));
        }

        private void btnOpenSkylight2_Click(object sender, EventArgs e)
        {
            // TurnOnSkyLight?.Invoke(sender, new EventSkyLightArgs(2));
        }

        private void btnCloseSkylight2_Click(object sender, EventArgs e)
        {
            // TurnOffSkyLight?.Invoke(sender, new EventSkyLightArgs(2));
        }

        private void btnOpenSkylight3_Click(object sender, EventArgs e)
        {
            // TurnOnSkyLight?.Invoke(sender, new EventSkyLightArgs(3));
        }

        private void btnCloseSkylight3_Click(object sender, EventArgs e)
        {
            // TurnOffSkyLight?.Invoke(sender, new EventSkyLightArgs(3));
        }

        private void toolStripMenuConfiguration_Click(object sender, EventArgs e)
        {

        }

        private void btnStatusSaveFolder_Click(object sender, EventArgs e)
        {
            SelectStatusDataSaveFolder?.Invoke(sender, e);
        }

        private void tbxStatusSaveFolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChangeStatusDataSaveFolder?.Invoke(sender, e);
            }
        }

        private void tbxStatusSaveFolder_Leave(object sender, EventArgs e)
        {
            ChangeStatusDataSaveFolder?.Invoke(sender, e);
        }

        private void btnTurnOnChiller_Click(object sender, EventArgs e)
        {
            TurnOnChiller?.Invoke(sender, e);
        }

        private void btnTurnOffChiller_Click(object sender, EventArgs e)
        {
            TurnOffChiller?.Invoke(sender, e);
        }

        private void btnLightMainSwitchOn_Click(object sender, EventArgs e)
        {
            TurnOnLighMainSwitch?.Invoke(sender, e);
        }

        private void btnLightMainSwitchOff_Click(object sender, EventArgs e)
        {
            TurnOffLighMainSwitch?.Invoke(sender, e);
        }

        private void btnOpenLight_Click(object sender, EventArgs e)
        {
            TurnOnLight?.Invoke(sender, e);
        }

        private void btnCloseLight_Click(object sender, EventArgs e)
        {
            TurnOffLight?.Invoke(sender, e);
        }

        private void btnRTPowerOn_Click(object sender, EventArgs e)
        {
            TurnOnRTPower?.Invoke(sender, e);
        }

        private void btnRTPowerOff_Click(object sender, EventArgs e)
        {
            TurnOffRTPower?.Invoke(sender, e);
        }

        private void btnAirConditionerOn_Click(object sender, EventArgs e)
        {
            TurnOnAirConditionerPower?.Invoke(sender, e);
        }

        private void btnAirConditionerOff_Click(object sender, EventArgs e)
        {
            TurnOffAirConditionerPower?.Invoke(sender, e);
        }

        private void btnCamOn_Click(object sender, EventArgs e)
        {
            TurnOnCamPower?.Invoke(sender, e);
        }

        private void btnCamOff_Click(object sender, EventArgs e)
        {
            TurnOffCamPower?.Invoke(sender, e);
        }

        private void btnPCOn_Click(object sender, EventArgs e)
        {
            TurnOnPCPower?.Invoke(sender, e);
        }

        private void btnPCOff_Click(object sender, EventArgs e)
        {
            TurnOffPCPower?.Invoke(sender, e);
        }

        private void btnStartReceive_Click(object sender, EventArgs e)
        {
            StartReceive?.Invoke(sender, e);
        }

        private void btnStopReceive_Click(object sender, EventArgs e)
        {
            StopReceive?.Invoke(sender, e);
        }

        private void tabCtrlMain_TabIndexChanged(object sender, EventArgs e)
        {
            ChangeTabIndex?.Invoke(sender, e);
        }

        private void cbxDarkRedLEDMainSwitch_Click(object sender, EventArgs e)
        {
            ChangeDarkRedLEDMainSwitch?.Invoke(sender, e);
        }

        private void cbxRedLEDMainSwitch_Click(object sender, EventArgs e)
        {
            ChangeRedLEDMainSwitch?.Invoke(sender, e);
        }

        private void cbxGreenLEDMainSwitch_Click(object sender, EventArgs e)
        {
            ChangeGreenLEDMainSwitch?.Invoke(sender, e);
        }

        private void tbxMinValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChangeMinValue?.Invoke(sender, e);
            }
        }

        private void tbxMinValue_Leave(object sender, EventArgs e)
        {
            ChangeMinValue?.Invoke(sender, e);
        }

        private void tbxMaxValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChangeMaxValue.Invoke(sender, e);
            }
        }

        private void tbxMaxValue_Leave(object sender, EventArgs e)
        {
            ChangeMaxValue?.Invoke(sender, e);
        }

        private void tbxMinNormValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChangeMinNormValue?.Invoke(sender, e);
            }
        }

        private void tbxMinNormValue_Leave(object sender, EventArgs e)
        {
            ChangeMinNormValue?.Invoke(sender, e);
        }

        private void tbxMaxNormValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ChangeMaxNormValue?.Invoke(sender, e);
            }
        }

        private void tbxMaxNormValue_Leave(object sender, EventArgs e)
        {
            ChangeMaxNormValue?.Invoke(sender, e);
        }
    }

    public class EventDimLEDArgs : EventArgs
    {
        public int LEDIndex;
        public double LEDPower;

        public EventDimLEDArgs(int thisLEDIndex, double thisLEDPower)
        {
            this.LEDIndex = thisLEDIndex;
            this.LEDPower = thisLEDPower;
        }
    }

    public class EventLEDArgs : EventArgs
    {
        public int LEDIndex;

        public EventLEDArgs(int thisLEDIndex)
        {
            this.LEDIndex = thisLEDIndex;
        }
    }

    public class EventSkyLightArgs : EventArgs
    {
        public int SkyLightIndex;

        public EventSkyLightArgs(int thisSkyLightIndex)
        {
            this.SkyLightIndex = thisSkyLightIndex;
        }
    }

    public class EventLightArgs : EventArgs
    {
        public int LightIndex;

        public EventLightArgs(int thisLightIndex)
        {
            this.LightIndex = thisLightIndex;
        }
    }

}
