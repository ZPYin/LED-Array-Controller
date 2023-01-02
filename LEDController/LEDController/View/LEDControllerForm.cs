using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Threading;
using ScottPlot;

namespace LEDController.View
{
    
    public partial class LEDControllerViewer : Form
    {
        public FormsPlot formsLEDStatusPlot;

        public LEDControllerViewer()
        {
            InitializeComponent();
            this.KeyPreview = true;

            // 初始化控件
            cbxQueryParam.SelectedIndex = 0;
            cbxQueryWaitTime.SelectedIndex = 0;
            formsLEDStatusPlot = this.formsPlotRTD;
        }

        public event EventHandler<EventArgs> ConnectTCP;
        public event EventHandler<EventArgs> CloseTCP;
        public event EventHandler<EventArgs> ConnectSerialPort;
        public event EventHandler<EventArgs> CloseSerialPort;
        public event EventHandler<EventArgs> SendTestData;
        public event EventHandler<EventLEDArgs> OpenFixLED;
        public event EventHandler<EventLEDArgs> CloseFixLED;
        public event EventHandler<EventLEDArgs> HandleFixLED;
        public event EventHandler<EventDimLEDArgs> SetDimLED;
        public event EventHandler<EventDimLEDArgs> CloseDimLED;
        public event EventHandler<EventDimLEDArgs> HandleDimLED;
        public event EventHandler<EventLEDArgs> ShowFixLEDStatus;
        public event EventHandler<EventDimLEDArgs> ShowDimLEDStatus;
        public event EventHandler<EventLEDArgs> ClearFixLEDStatus;
        public event EventHandler<EventDimLEDArgs> ClearDimLEDStatus;
        public event EventHandler<EventDimLEDArgs> UpdateScrollBar;
        public event EventHandler<EventDimLEDArgs> UpdateLEDTbx;
        public event EventHandler<EventArgs> ShowLEDStatus;
        public event EventHandler<EventArgs> OpenWithCfgFile;
        public event EventHandler<EventArgs> SaveCfgFile;
        public event EventHandler<EventArgs> SaveAsCfgFile;
        public event EventHandler<EventArgs> ShowVersion;
        public event EventHandler<EventArgs> ShowChillerStatus;
        public event EventHandler<EventSkyLightArgs> TurnOnSkyLight;
        public event EventHandler<EventSkyLightArgs> TurnOffSkyLight;
        
        public DispatcherTimer timer = new DispatcherTimer();

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

        public Color[] LEDStatusColors
        {
            get 
            {
                List<Control> list = new List<Control>();
                GetAllControl(this.panelLEDStatus, list, "LED");
                Color[] LEDColors = new Color[list.Count()];
                
                foreach (Control control in list)
                {
                    if (control.GetType() == typeof(Button))
                    {
                        LEDColors[Convert.ToInt32(control.Tag) - 1] = control.BackColor;
                    }
                }

                return LEDColors;
            }
            set 
            {
                List<Control> list = new List<Control>();
                GetAllControl(this.panelLEDStatus, list, "LED");

                foreach (Control control in list)
                {
                    if (control.GetType() == typeof(Button))
                    {
                        control.BackColor = value[Convert.ToInt32(control.Tag) - 1];
                    }
                }
            }
        }

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
                OpenFixLED?.Invoke(btn, new EventLEDArgs(LEDIndex, 3));
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
                CloseFixLED?.Invoke(btn, new EventLEDArgs(LEDIndex, 3));
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
                OpenFixLED?.Invoke(btn, new EventLEDArgs(LEDIndex, 1));
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
                CloseFixLED?.Invoke(btn, new EventLEDArgs(LEDIndex, 1));
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
                OpenFixLED?.Invoke(btn, new EventLEDArgs(LEDIndex, 2));
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
                CloseFixLED?.Invoke(btn, new EventLEDArgs(LEDIndex, 2));
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

                SetDimLED?.Invoke(btn, new EventDimLEDArgs(idxLED, 3, Convert.ToDouble(tbx.Text)));
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

                CloseDimLED?.Invoke(btn, new EventDimLEDArgs(idxLED, 3, Convert.ToDouble(tbx.Text)));
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

                SetDimLED?.Invoke(btn, new EventDimLEDArgs(idxLED, 1, Convert.ToDouble(tbx.Text)));
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

                CloseDimLED?.Invoke(btn, new EventDimLEDArgs(idxLED, 1, Convert.ToDouble(tbx.Text)));
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

                SetDimLED?.Invoke(btn, new EventDimLEDArgs(idxLED, 2, Convert.ToDouble(tbx.Text)));
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

                CloseDimLED?.Invoke(btn, new EventDimLEDArgs(idxLED, 2, Convert.ToDouble(tbx.Text)));
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
        }

        private void tsmLEDStatus_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 2;
        }

        private void tsmChillerControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 3;
        }

        private void tsmSkylightControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 4;
        }

        private void tsmLightControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 5;
        }

        private void tsmRTControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 6;
        }

        private void tsmAirConditionerControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 7;
        }

        private void tsmCameraControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 8;
        }

        private void btnShowLEDStatus_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if (btn.Text == "开始获取")
            {
                btn.BackColor = Color.Green;
                btn.Text = "停止获取";

                timer.Interval = TimeSpan.FromSeconds(Convert.ToDouble(cbxQueryWaitTime.GetItemText(cbxQueryWaitTime.SelectedItem)));
                timer.Tick += StartShowLEDStatus;

                timer.Start();
            }
            else
            {
                btn.BackColor = Color.Empty;
                btn.Text = "开始获取";

                timer.Stop();
            }
        }

        private void StartShowLEDStatus(object sender, EventArgs e)
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
            HandleFixLED?.Invoke(sender, new EventLEDArgs(idxLED, 2));
        }

        private void btnDarkRedLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDarkRedLED");

            ShowFixLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED, 2));
        }

        private void btnDarkRedLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDarkRedLED");

            ClearFixLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED, 2));
        }

        private void btnGreenLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnGreenLED");
            HandleFixLED?.Invoke(sender, new EventLEDArgs(idxLED, 3));
        }

        private void btnGreenLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnGreenLED");

            ShowFixLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED, 3));
        }

        private void btnGreenLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnGreenLED");

            ClearFixLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED, 3));
        }

        private void btnRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnRedLED");
            HandleFixLED?.Invoke(sender, new EventLEDArgs(idxLED, 1));
        }

        private void btnRedLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnRedLED");

            ShowFixLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED, 1));
        }

        private void btnRedLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnRedLED");

            ClearFixLEDStatus?.Invoke(sender, new EventLEDArgs(idxLED, 1));
        }

        private void btnDimGreenLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimGreenLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimGreenLED{idxLED}", true)[0]);

            HandleDimLED?.Invoke(sender, new EventDimLEDArgs(idxLED, 3, Convert.ToDouble(tbx.Text)));
        }

        private void btnDimGreenLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimGreenLED");

            ShowDimLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 3, 0));
        }

        private void btnDimGreenLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimGreenLED");

            ClearDimLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 3, 0));
        }

        private void tbxDimGreenLED_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tbx = sender as TextBox;
                int idxLED = GetLEDIndex(tbx.Name, "tbxDimGreenLED");
                UpdateScrollBar?.Invoke(sender, new EventDimLEDArgs(idxLED, 3, Convert.ToDouble(tbx.Text)));
            }
        }

        private void sbarDimGreenLED_Scroll(object sender, EventArgs e)
        {
            TrackBar tbar = sender as TrackBar;
            int idxLED = GetLEDIndex(tbar.Name, "sbarDimGreenLED");
            UpdateLEDTbx?.Invoke(sender, new EventDimLEDArgs(idxLED, 3, (double)tbar.Value));
        }

        private void btnCfgDimGreenLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnOpenDimGreenLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimGreenLED{idxLED}", true)[0]);

            SetDimLED?.Invoke(sender, new EventDimLEDArgs(idxLED, 3, Convert.ToDouble(tbx.Text)));
        }

        private void btnDimDarkRedLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimDarkRedLED");

            ShowDimLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 2, 0));
        }

        private void tbxDimDarkRedLED_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tbx = sender as TextBox;
                int idxLED = GetLEDIndex(tbx.Name, "tbxDimDarkRedLED");
                UpdateScrollBar?.Invoke(sender, new EventDimLEDArgs(idxLED, 2, Convert.ToDouble(tbx.Text)));
            }
        }

        private void btnDimDarkRedLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimDarkRedLED");

            ClearDimLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 2, 0));
        }

        private void btnDimDarkRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDarkRedLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimDarkRedLED{idxLED}", true)[0]);

            HandleDimLED?.Invoke(sender, new EventDimLEDArgs(idxLED, 2, Convert.ToDouble(tbx.Text)));
        }

        private void sbarDimDarkRedLED_Scroll(object sender, EventArgs e)
        {
            TrackBar tbar = sender as TrackBar;
            int idxLED = GetLEDIndex(tbar.Name, "sbarDimDarkRedLED");
            UpdateLEDTbx?.Invoke(sender, new EventDimLEDArgs(idxLED, 2, (double)tbar.Value));
        }

        private void btnCfgDimDarkRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnOpenDimDarkRedLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimDarkRedLED{idxLED}", true)[0]);

            SetDimLED?.Invoke(sender, new EventDimLEDArgs(idxLED, 2, Convert.ToDouble(tbx.Text)));
        }

        private void btnCfgDimRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnOpenDimRedLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimRedLED{idxLED}", true)[0]);

            SetDimLED?.Invoke(sender, new EventDimLEDArgs(idxLED, 1, Convert.ToDouble(tbx.Text)));
        }

        private void sbarDimRedLED_Scroll(object sender, EventArgs e)
        {
            TrackBar tbar = sender as TrackBar;
            int idxLED = GetLEDIndex(tbar.Name, "sbarDimRedLED");
            UpdateLEDTbx?.Invoke(sender, new EventDimLEDArgs(idxLED, 1, (double)tbar.Value));
        }

        private void tbxDimRedLED_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox tbx = sender as TextBox;
                int idxLED = GetLEDIndex(tbx.Name, "tbxDimRedLED");
                UpdateScrollBar?.Invoke(sender, new EventDimLEDArgs(idxLED, 1, Convert.ToDouble(tbx.Text)));
            }
        }

        private void btnDimRedLED_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimRedLED");
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimRedLED{idxLED}", true)[0]);

            HandleDimLED?.Invoke(sender, new EventDimLEDArgs(idxLED, 1, Convert.ToDouble(tbx.Text)));
        }

        private void btnDimRedLED_MouseHover(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimRedLED");

            ShowDimLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 1, 0));
        }

        private void btnDimRedLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int idxLED = GetLEDIndex(btn.Name, "btnDimRedLED");

            ClearDimLEDStatus?.Invoke(sender, new EventDimLEDArgs(idxLED, 1, 0));
        }

        private void btnUpdateChiller_Click(object sender, EventArgs e)
        {
            ShowChillerStatus?.Invoke(sender, e);
        }

        private void btnOpenSkylight1_Click(object sender, EventArgs e)
        {
            TurnOnSkyLight?.Invoke(sender, new EventSkyLightArgs(1));
        }

        private void btnCloseSkylight1_Click(object sender, EventArgs e)
        {
            TurnOffSkyLight?.Invoke(sender, new EventSkyLightArgs(1));
        }

        private void btnOpenSkylight2_Click(object sender, EventArgs e)
        {
            TurnOnSkyLight?.Invoke(sender, new EventSkyLightArgs(2));
        }

        private void btnCloseSkylight2_Click(object sender, EventArgs e)
        {
            TurnOffSkyLight?.Invoke(sender, new EventSkyLightArgs(2));
        }

        private void btnOpenSkylight3_Click(object sender, EventArgs e)
        {
            TurnOnSkyLight?.Invoke(sender, new EventSkyLightArgs(3));
        }

        private void btnCloseSkylight3_Click(object sender, EventArgs e)
        {
            TurnOffSkyLight?.Invoke(sender, new EventSkyLightArgs(3));
        }
    }

    public class EventDimLEDArgs : EventArgs
    {
        public int LEDIndex;
        public int addrPLC;
        public double LEDPower;

        public EventDimLEDArgs(int thisLEDIndex, int thisAddrPLC, double thisLEDPower)
        {
            this.addrPLC = thisAddrPLC;
            this.LEDIndex = thisLEDIndex;
            this.LEDPower = thisLEDPower;
        }
    }

    public class EventLEDArgs : EventArgs
    {
        public int LEDIndex;
        public int addrPLC;

        public EventLEDArgs(int thisLEDIndex, int thisAddrPLC)
        {
            this.LEDIndex = thisLEDIndex;
            this.addrPLC = thisAddrPLC;
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

}
