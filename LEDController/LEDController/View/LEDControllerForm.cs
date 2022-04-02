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

        public event EventHandler<EventArgs> Connect;
        public event EventHandler<EventArgs> CloseConnect;
        public event EventHandler<EventArgs> SendTestData;
        public event EventHandler<EventFixLEDArgs> OpenFixLED;
        public event EventHandler<EventFixLEDArgs> CloseFixLED;
        public event EventHandler<EventFixLEDArgs> HandleFixLED;
        public event EventHandler<EventDimLEDArgs> SetDimLED;
        public event EventHandler<EventDimLEDArgs> CloseDimLED;
        public event EventHandler<EventDimLEDArgs> HandleDimLED;
        public event EventHandler<EventLEDArgs> ShowSingleLEDStatus;
        public event EventHandler<EventLEDArgs> ClearSingleLEDStatus;
        public event EventHandler<EventArgs> UpdateScrollBar;
        public event EventHandler<EventArgs> UpdateLEDTbx;
        public event EventHandler<EventArgs> ShowLEDStatus;
        public event EventHandler<EventArgs> OpenWithCfgFile;
        public event EventHandler<EventArgs> SaveCfgFile;
        public event EventHandler<EventArgs> SaveasCfgFile;
        public event EventHandler<EventArgs> ShowVersion;
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
        public double minLEDStatusValue 
        { 
            get { return Convert.ToDouble(tbxMinValue.Text); }
            set { tbxMinValue.Text = Convert.ToString(value); }
        }
        public double maxLEDStatusValue
        {
            get { return Convert.ToDouble(tbxMaxValue.Text); }
            set { tbxMaxValue.Text = Convert.ToString(value); }
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
        public Color btnConnectColor
        {
            get { return btnConnect.BackColor; }
            set 
            { 
                btnConnect.BackColor = value; 
                btnConnect.Refresh(); 
            }
        }
        public Color btnCloseColor
        {
            get { return btnClose.BackColor; }
            set 
            { 
                btnClose.BackColor = value; 
                btnClose.Refresh(); 
            }
        }
        public Color[] LEDStatusColors
        {
            get 
            {
                List<Control> list = new List<Control>();
                GetAllControl(this.panelLEDStatus, list);
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
                GetAllControl(this.panelLEDStatus, list);

                foreach (Control control in list)
                {
                    if (control.GetType() == typeof(Button))
                    {
                        control.BackColor = value[Convert.ToInt32(control.Tag) - 1];
                    }
                }
            }
        }
        public string slaveIP
        {
            get { return tbxIP.Text; }
            set { tbxIP.Text = value; }
        }
        public string slavePort
        {
            get { return tbxPort.Text; }
            set { tbxPort.Text = value; }
        }
        public string tbxConnectMsgText
        {
            get { return tbxConnectMsg.Text; }
            set { tbxConnectMsg.AppendText(value); }
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
        public string tsslGreenLEDTPText
        {
            get { return tsslGreenLEDTotalPower.Text; }
            set { tsslGreenLEDTotalPower.Text = value; }
        }
        public string tsslRedLEDTPText
        {
            get { return tsslRedLEDTotalPower.Text; }
            set { tsslRedLEDTotalPower.Text = value; }
        }
        public string tsslDarkRedTPText
        {
            get { return tsslDarkRedLEDTotalPower.Text; }
            set { tsslDarkRedLEDTotalPower.Text = value; }
        }
        public string tsslTemp1Text
        {
            get { return tsslTemp1.Text; }
            set { tsslTemp1.Text = value; }
        }
        public string tsslTemp2Text
        {
            get { return tsslTemp2.Text; }
            set { tsslTemp2.Text = value; }
        }
        public string tsslTemp3Text
        {
            get { return tsslTemp3.Text; }
            set { tsslTemp3.Text = value; }
        }
        public string tsslTemp4Text
        {
            get { return tsslTemp4.Text; }
            set { tsslTemp4.Text = value; }
        }
        public string testCmdStr
        {
            get { return tbxTestCmd.Text; }
            set { tbxTestCmd.Text = value; }
        }
        public string testMsgRecStr
        {
            get { return tbxTestRec.Text; }
            set { tbxTestRec.AppendText(value); }
        }
        public int sbarDimLED1Value
        {
            get { return sbarDimLED1.Value; }
            set { sbarDimLED1.Value = value; }
        }
        public int sbarDimLED2Value
        {
            get { return sbarDimLED2.Value; }
            set { sbarDimLED2.Value = value; }
        }
        public int sbarDimLED3Value
        {
            get { return sbarDimLED3.Value; }
            set { sbarDimLED3.Value = value; }
        }
        public int sbarDimLED4Value
        {
            get { return sbarDimLED4.Value; }
            set { sbarDimLED4.Value = value; }
        }
        public int sbarDimLED5Value
        {
            get { return sbarDimLED5.Value; }
            set { sbarDimLED5.Value = value; }
        }
        public int sbarDimLED6Value
        {
            get { return sbarDimLED6.Value; }
            set { sbarDimLED6.Value = value; }
        }
        public int sbarDimLED7Value
        {
            get { return sbarDimLED7.Value; }
            set { sbarDimLED7.Value = value; }
        }
        public int sbarDimLED8Value
        {
            get { return sbarDimLED8.Value; }
            set { sbarDimLED8.Value = value; }
        }
        public int sbarDimLED9Value
        {
            get { return sbarDimLED9.Value; }
            set { sbarDimLED9.Value = value; }
        }
        public int sbarDimLED10Value
        {
            get { return sbarDimLED10.Value; }
            set { sbarDimLED10.Value = value; }
        }
        public int sbarDimLED11Value
        {
            get { return sbarDimLED11.Value; }
            set { sbarDimLED11.Value = value; }
        }
        public int sbarDimLED12Value
        {
            get { return sbarDimLED12.Value; }
            set { sbarDimLED12.Value = value; }
        }
        public string tbxDimLED1Text
        {
            get { return tbxDimLED1.Text; }
            set { tbxDimLED1.Text = value; }
        }
        public string tbxDimLED2Text
        {
            get { return tbxDimLED2.Text; }
            set { tbxDimLED2.Text = value; }
        }
        public string tbxDimLED3Text
        {
            get { return tbxDimLED3.Text; }
            set { tbxDimLED3.Text = value; }
        }
        public string tbxDimLED4Text
        {
            get { return tbxDimLED4.Text; }
            set { tbxDimLED4.Text = value; }
        }
        public string tbxDimLED5Text
        {
            get { return tbxDimLED5.Text; }
            set { tbxDimLED5.Text = value; }
        }
        public string tbxDimLED6Text
        {
            get { return tbxDimLED6.Text; }
            set { tbxDimLED6.Text = value; }
        }
        public string tbxDimLED7Text
        {
            get { return tbxDimLED7.Text; }
            set { tbxDimLED7.Text = value; }
        }
        public string tbxDimLED8Text
        {
            get { return tbxDimLED8.Text; }
            set { tbxDimLED8.Text = value; }
        }
        public string tbxDimLED9Text
        {
            get { return tbxDimLED9.Text; }
            set { tbxDimLED9.Text = value; }
        }
        public string tbxDimLED10Text
        {
            get { return tbxDimLED10.Text; }
            set { tbxDimLED10.Text = value; }
        }
        public string tbxDimLED11Text
        {
            get { return tbxDimLED11.Text; }
            set { tbxDimLED11.Text = value; }
        }
        public string tbxDimLED12Text
        {
            get { return tbxDimLED12.Text; }
            set { tbxDimLED12.Text = value; }
        }
        public string lblGreenLEDMaxLeftText
        {
            get { return lblGreenLEDMaxLeft.Text; }
            set { lblGreenLEDMaxLeft.Text = value; }
        }
        public string lblGreenLEDMinLeftText
        {
            get { return lblGreenLEDMinLeft.Text; }
            set { lblGreenLEDMinLeft.Text = value; }
        }
        public string lblGreenLEDMaxRightText
        {
            get { return lblGreenLEDMaxRight.Text; }
            set { lblGreenLEDMaxRight.Text = value; }
        }
        public string lblGreenLEDMinRightText
        {
            get { return lblGreenLEDMinRight.Text; }
            set { lblGreenLEDMinRight.Text = value; }
        }
        public string lblRedLEDMaxLeftText
        {
            get { return lblRedLEDMaxLeft.Text; }
            set { lblRedLEDMaxLeft.Text = value; }
        }
        public string lblRedLEDMinLeftText
        {
            get { return lblRedLEDMinLeft.Text; }
            set { lblRedLEDMinLeft.Text = value; }
        }
        public string lblRedLEDMaxRightText
        {
            get { return lblRedLEDMaxRight.Text; }
            set { lblRedLEDMaxRight.Text = value; }
        }
        public string lblRedLEDMinRightText
        {
            get { return lblRedLEDMinRight.Text; }
            set { lblRedLEDMinRight.Text = value; }
        }
        public string lblDarkRedLEDMaxLeftText
        {
            get { return lblDarkRedLEDMaxLeft.Text; }
            set { lblDarkRedLEDMaxLeft.Text = value; }
        }
        public string lblDarkRedLEDMinLeftText
        {
            get { return lblDarkRedLEDMinLeft.Text; }
            set { lblDarkRedLEDMinLeft.Text = value; }
        }
        public string lblDarkRedLEDMaxRightText
        {
            get { return lblDarkRedLEDMaxRight.Text; }
            set { lblDarkRedLEDMaxRight.Text = value; } 
        }
        public string lblDarkRedLEDMinRightText
        {
            get { return lblDarkRedLEDMinRight.Text; }
            set { lblDarkRedLEDMinRight.Text = value; }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Click connect button
            Connect?.Invoke(sender, e);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            // Click disconnect button
            CloseConnect?.Invoke(sender, e);
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

        private void sbarDimLED1_Scroll(object sender, EventArgs e)
        {
            // Update LED power textbox
            UpdateLEDTbx?.Invoke(sender, e);
        }

        private void sbarDimLED5_Scroll(object sender, EventArgs e)
        {
            // Update LED power textbox
            UpdateLEDTbx?.Invoke(sender, e);
        }

        private void tbxDimLED1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Update scroll bar
                UpdateScrollBar?.Invoke(sender, e);
            }
        }

        private void GetAllControl(Control c, List<Control> list)
        {
            // Get all controls
            foreach (Control control in c.Controls)
            {
                list.Add(control);

                if (control.GetType() == typeof(Panel))
                    GetAllControl(control, list);
            }
        }

        private void GetAllControl(Control c, List<Control> list, string substr)
        {
            // Get all controls
            foreach (Control control in c.Controls)
            {
                if (control.Name.Contains(substr))
                    list.Add(control);

                if (control.GetType() == typeof(Panel))
                    GetAllControl(control, list);
            }
        }

        private void btnOpenGreenFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Green Fix LEDs
            this.Cursor = Cursors.WaitCursor;
            List<Control> list = new List<Control>();
            GetAllControl(this.panelGreenFixLED, list, "btnLED");

            for (int i = 0; i < list.Count(); i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(panelGreenFixLED.Controls.Find($"btnLED{LEDIndex}", true)[0]);
                OpenFixLED?.Invoke(btn, new EventFixLEDArgs(Convert.ToInt16(btn.Tag)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseGreenFixLED_Click(object sender, EventArgs e)
        {
            // Turn off all Green LEDs
            this.Cursor = Cursors.WaitCursor;
            List<Control> list = new List<Control>();
            GetAllControl(this.panelGreenFixLED, list, "btnLED");

            for (int i = 0; i < list.Count(); i++)
            {
                int LEDIndex = i + 1;
                Button btn = (Button)(panelGreenFixLED.Controls.Find($"btnLED{LEDIndex}", true)[0]);
                CloseFixLED?.Invoke(btn, new EventFixLEDArgs(Convert.ToInt16(btn.Tag)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnOpenRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Fix LEDs
            this.Cursor = Cursors.WaitCursor;

            List<Control> greenLEDList = new List<Control>();
            GetAllControl(this.panelGreenFixLED, greenLEDList, "btnLED");
            int nGreenFixLED = greenLEDList.Count();

            List<Control> list = new List<Control>();
            GetAllControl(this.panelRedFixLED, list, "btnLED");

            for (int i = 0; i < list.Count(); i++)
            {
                int LEDIndex = i + 1 + nGreenFixLED;
                Button btn = (Button)(panelRedFixLED.Controls.Find($"btnLED{LEDIndex}", true)[0]);
                OpenFixLED?.Invoke(btn, new EventFixLEDArgs(Convert.ToInt16(btn.Tag)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Fix LEDs
            this.Cursor = Cursors.WaitCursor;

            List<Control> greenLEDList = new List<Control>();
            GetAllControl(this.panelGreenFixLED, greenLEDList, "btnLED");
            int nGreenFixLED = greenLEDList.Count();

            List<Control> list = new List<Control>();
            GetAllControl(this.panelRedFixLED, list, "btnLED");

            for (int i = 0; i < list.Count(); i++)
            {
                int LEDIndex = i + 1 + nGreenFixLED;
                Button btn = (Button)(panelRedFixLED.Controls.Find($"btnLED{LEDIndex}", true)[0]);
                CloseFixLED?.Invoke(btn, new EventFixLEDArgs(Convert.ToInt16(btn.Tag)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnOpenDarkRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Fix LEDs
            this.Cursor = Cursors.WaitCursor;

            List<Control> greenLEDList = new List<Control>();
            GetAllControl(this.panelGreenFixLED, greenLEDList, "btnLED");
            int nGreenFixLED = greenLEDList.Count();
            List<Control> redLEDList = new List<Control>();
            GetAllControl(this.panelRedFixLED, redLEDList, "btnLED");
            int nRedFixLED = redLEDList.Count();

            List<Control> list = new List<Control>();
            GetAllControl(this.panelDarkRedFixLED, list, "btnLED");

            for (int i = 0; i < list.Count(); i++)
            {
                int LEDIndex = i + 1 + nGreenFixLED + nRedFixLED;
                Button btn = (Button)(panelDarkRedFixLED.Controls.Find($"btnLED{LEDIndex}", true)[0]);
                OpenFixLED?.Invoke(btn, new EventFixLEDArgs(Convert.ToInt16(btn.Tag)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseDarkRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Fix LEDs
            this.Cursor = Cursors.WaitCursor;

            List<Control> greenLEDList = new List<Control>();
            GetAllControl(this.panelGreenFixLED, greenLEDList, "btnLED");
            int nGreenFixLED = greenLEDList.Count();
            List<Control> redLEDList = new List<Control>();
            GetAllControl(this.panelRedFixLED, redLEDList, "btnLED");
            int nRedFixLED = redLEDList.Count();

            List<Control> list = new List<Control>();
            GetAllControl(this.panelDarkRedFixLED, list, "btnLED");

            for (int i = 0; i < list.Count(); i++)
            {
                int LEDIndex = i + 1 + nGreenFixLED + nRedFixLED;
                Button btn = (Button)(panelDarkRedFixLED.Controls.Find($"btnLED{LEDIndex}", true)[0]);
                CloseFixLED?.Invoke(btn, new EventFixLEDArgs(Convert.ToInt16(btn.Tag)));
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnOpenGreenDimLED_Click(object sender, EventArgs e)
        {
            // Turn on all Green Dimmable LED
            this.Cursor = Cursors.WaitCursor;
            List<Button> list = new List<Button>();
            double[] LEDPowers = new double[] {Convert.ToDouble(tbxDimLED1.Text), Convert.ToDouble(tbxDimLED2.Text), Convert.ToDouble(tbxDimLED3.Text), Convert.ToDouble(tbxDimLED4.Text)};

            list.Add(btnDimLED1);
            list.Add(btnDimLED2);
            list.Add(btnDimLED3);
            list.Add(btnDimLED4);

            for (int i = 0; i < list.Count; i++)
            {
                Button btn = list[i];
                EventDimLEDArgs dimLEDEvent = new EventDimLEDArgs(Convert.ToInt32(btn.Tag) - 120, LEDPowers[i]);

                SetDimLED?.Invoke(list[i], dimLEDEvent);
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseGreenDimLED_Click(object sender, EventArgs e)
        {
            // Turn off all Green Dimmable LED
            this.Cursor = Cursors.WaitCursor;
            List<Button> list = new List<Button>();
            double[] LEDPowers = new double[] {Convert.ToDouble(tbxDimLED1.Text), Convert.ToDouble(tbxDimLED2.Text), Convert.ToDouble(tbxDimLED3.Text), Convert.ToDouble(tbxDimLED4.Text)};

            list.Add(btnDimLED1);
            list.Add(btnDimLED2);
            list.Add(btnDimLED3);
            list.Add(btnDimLED4);

            for (int i = 0; i < list.Count; i++)
            {
                Button btn = list[i];
                EventDimLEDArgs dimLEDEvent = new EventDimLEDArgs(Convert.ToInt32(btn.Tag) - 120, LEDPowers[i]);

                CloseDimLED?.Invoke(list[i], dimLEDEvent);
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnOpenRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Dimmable LED
            this.Cursor = Cursors.WaitCursor;
            List<Button> list = new List<Button>();
            double[] LEDPowers = new double[] {Convert.ToDouble(tbxDimLED5.Text), Convert.ToDouble(tbxDimLED6.Text), Convert.ToDouble(tbxDimLED7.Text), Convert.ToDouble(tbxDimLED8.Text)};

            list.Add(btnDimLED5);
            list.Add(btnDimLED6);
            list.Add(btnDimLED7);
            list.Add(btnDimLED8);

            for (int i = 0; i < list.Count; i++)
            {
                Button btn = list[i];
                EventDimLEDArgs dimLEDEvent = new EventDimLEDArgs(Convert.ToInt32(btn.Tag), LEDPowers[i]);

                SetDimLED?.Invoke(list[i], dimLEDEvent);
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn off all Red Dimmable LED
            this.Cursor = Cursors.WaitCursor;
            List<Button> list = new List<Button>();
            double[] LEDPowers = new double[] {Convert.ToDouble(tbxDimLED5.Text), Convert.ToDouble(tbxDimLED6.Text), Convert.ToDouble(tbxDimLED7.Text), Convert.ToDouble(tbxDimLED8.Text)};

            list.Add(btnDimLED5);
            list.Add(btnDimLED6);
            list.Add(btnDimLED7);
            list.Add(btnDimLED8);

            for (int i = 0; i < list.Count; i++)
            {
                Button btn = list[i];
                EventDimLEDArgs dimLEDEvent = new EventDimLEDArgs(Convert.ToInt32(btn.Tag), LEDPowers[i]);

                CloseDimLED?.Invoke(list[i], dimLEDEvent);
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnOpenDarkRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn on all DarkRed Dimmable LED
            this.Cursor = Cursors.WaitCursor;
            List<Button> list = new List<Button>();
            double[] LEDPowers = new double[] {Convert.ToDouble(tbxDimLED9.Text), Convert.ToDouble(tbxDimLED10.Text), Convert.ToDouble(tbxDimLED11.Text), Convert.ToDouble(tbxDimLED12.Text)};

            list.Add(btnDimLED9);
            list.Add(btnDimLED10);
            list.Add(btnDimLED11);
            list.Add(btnDimLED12);

            for (int i = 0; i < list.Count; i++)
            {
                Button btn = list[i];
                EventDimLEDArgs dimLEDEvent = new EventDimLEDArgs(Convert.ToInt32(btn.Tag), LEDPowers[i]);

                SetDimLED?.Invoke(list[i], dimLEDEvent);
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void btnCloseDarkRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn off all DarkRed Dimmable LED
            this.Cursor = Cursors.WaitCursor;
            List<Button> list = new List<Button>();
            double[] LEDPowers = new double[] {Convert.ToDouble(tbxDimLED9.Text), Convert.ToDouble(tbxDimLED10.Text), Convert.ToDouble(tbxDimLED11.Text), Convert.ToDouble(tbxDimLED12.Text)};

            list.Add(btnDimLED9);
            list.Add(btnDimLED10);
            list.Add(btnDimLED11);
            list.Add(btnDimLED12);

            for (int i = 0; i < list.Count; i++)
            {
                Button btn = list[i];
                EventDimLEDArgs dimLEDEvent = new EventDimLEDArgs(Convert.ToInt32(btn.Tag), LEDPowers[i]);

                CloseDimLED?.Invoke(list[i], dimLEDEvent);
                Thread.Sleep(100);
            }
            this.Cursor = Cursors.Default;
        }

        private void tsmConnect_Click(object sender, EventArgs e)
        {
            // select first panel through menu
            tabCtrlMain.SelectedIndex = 0;
        }

        private void tsmLEDControl_Click(object sender, EventArgs e)
        {
            // select second panel through menu
            tabCtrlMain.SelectedIndex = 1;
        }

        private void tsmLEDStatus_Click(object sender, EventArgs e)
        {
            // select third panel through menu
            tabCtrlMain.SelectedIndex = 2;
        }

        private void tsmRTControl_Click(object sender, EventArgs e)
        {
            // select 4th panel through menu
            tabCtrlMain.SelectedIndex = 3;
        }

        private void tsmLightControl_Click(object sender, EventArgs e)
        {
            // select 5th panel through menu
            tabCtrlMain.SelectedIndex = 4;
        }

        private void tsmAirConditionerControl_Click(object sender, EventArgs e)
        {
            // select 6th panel through menu
            tabCtrlMain.SelectedIndex = 5;
        }

        private void tsmSkylightControl_Click(object sender, EventArgs e)
        {
            // select 7th panel through menu
            tabCtrlMain.SelectedIndex = 6;
        }

        private void tsmCameraControl_Click(object sender, EventArgs e)
        {
            // select 8th panel through menu
            tabCtrlMain.SelectedIndex = 7;
        }

        private void btnLED_Click(object sender, EventArgs e)
        { 
            // Click Fix LED button
            Button btn = sender as Button;
            HandleFixLED?.Invoke(sender, new EventFixLEDArgs(Convert.ToInt16(btn.Tag)));
        }

        private void btnLED_MouseHover(object sender, EventArgs e)
        {
            // Show LED status
            Button btn = sender as Button;
            EventLEDArgs LEDEvent = new EventLEDArgs(Convert.ToInt32(btn.Tag));
            ShowSingleLEDStatus?.Invoke(sender, LEDEvent);
        }

        private void btnLED_MouseLeave(object sender, EventArgs e)
        {
            // Clear LED status
            Button btn = sender as Button;
            EventLEDArgs LEDEvent = new EventLEDArgs(Convert.ToInt32(btn.Tag));
            ClearSingleLEDStatus?.Invoke(sender, LEDEvent);
        }

        private void btnDimLED_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            EventLEDArgs LEDEvent = new EventLEDArgs(Convert.ToInt32(btn.Tag));
            ClearSingleLEDStatus?.Invoke(sender, LEDEvent);
        }

        private void btnDimLED_MouseHover(object sender, EventArgs e)
        {
            // Show LED status
            Button btn = sender as Button;
            EventLEDArgs LEDEvent = new EventLEDArgs(Convert.ToInt32(btn.Tag));
            ShowSingleLEDStatus?.Invoke(sender, LEDEvent);
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

        private void btnDimLED_Click(object sender, EventArgs e)
        {
            // Turn on Dimmable LED
            Button btn = sender as Button;
            int LEDIndex = Convert.ToInt32(btn.Tag) - 120;
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimLED{LEDIndex}", true)[0]);
            EventDimLEDArgs dimLEDEvent = new EventDimLEDArgs(LEDIndex, Convert.ToDouble(tbx.Text));

            HandleDimLED?.Invoke(sender, dimLEDEvent);
        }

        private void btnCfgDimLED_Click(object sender, EventArgs e)
        {
            // Turn on Dimmable LED
            Button btn = sender as Button;
            int LEDIndex = Convert.ToInt32(btn.Tag) - 120;
            Button LEDBtn = (Button)(this.Controls.Find($"btnDimLED{LEDIndex}", true)[0]);
            TextBox tbx = (TextBox)(this.Controls.Find($"tbxDimLED{LEDIndex}", true)[0]);
            EventDimLEDArgs dimLEDEvent = new EventDimLEDArgs(LEDIndex, Convert.ToDouble(tbx.Text));

            SetDimLED?.Invoke(LEDBtn, dimLEDEvent);
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
            SaveasCfgFile?.Invoke(sender, e);
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
    }

    public class EventDimLEDArgs : EventArgs
    {
        public int LEDIndex;
        public double LEDPower;

        public EventDimLEDArgs(int thisLEDIndex, double thisLEDPower)
        {
            LEDIndex = thisLEDIndex;
            LEDPower = thisLEDPower;
        }
    }

    public class EventFixLEDArgs : EventArgs
    {
        public int LEDIndex;

        public EventFixLEDArgs(int thisLEDIndex)
        {
            LEDIndex = thisLEDIndex;
        }
    }

    public class EventLEDArgs : EventArgs
    {
        public int LEDIndex;

        public EventLEDArgs(int thisLEDIndex)
        {
            LEDIndex = thisLEDIndex;
        }
    }

}
