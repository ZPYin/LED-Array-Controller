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

namespace LEDController.View
{
    
    public partial class LEDControllerViewer : Form, ILEDControllerForm
    {
        public LEDControllerViewer()
        {
            InitializeComponent();
            this.KeyPreview = true;

            cbxQueryParam.SelectedIndex = 0;
            cbxQueryWaitTime.SelectedIndex = 0;
        }

        public event EventHandler<EventArgs> Connect;
        public event EventHandler<EventArgs> CloseConnect;
        public event EventHandler<EventArgs> SendTestData;
        public event EventHandler<EventArgs> OpenFixLED;
        public event EventHandler<EventArgs> CloseFixLED;
        public event EventHandler<EventArgs> HandleFixLED;
        public event EventHandler<EventArgs> OpenDimLED;
        public event EventHandler<EventArgs> CloseDimLED;
        public event EventHandler<EventArgs> HandleDimLED;
        public event EventHandler<EventArgs> ShowSingleLEDStatus;
        public event EventHandler<EventArgs> ClearSingleLEDStatus;
        public event EventHandler<EventArgs> UpdateScrollBar;
        public event EventHandler<EventArgs> UpdateLEDTbx;
        public event EventHandler<EventArgs> ShowLEDStatus;
        public DispatcherTimer timer = new DispatcherTimer();
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
            set { btnRecStatus1.BackColor = value; btnRecStatus1.Refresh(); }
        }
        public Color btnRecStatus2Color
        {
            get { return btnRecStatus2.BackColor; }
            set { btnRecStatus2.BackColor = value; btnRecStatus2.Refresh(); }
        }
        public Color btnRecStatus3Color
        {
            get { return btnRecStatus3.BackColor; }
            set { btnRecStatus3.BackColor = value; btnRecStatus3.Refresh(); }
        }
        public Color btnSendStatus1Color
        {
            get { return btnSendStatus1.BackColor; }
            set { btnSendStatus1.BackColor = value; btnSendStatus1.Refresh(); }
        }
        public Color btnSendStatus2Color
        {
            get { return btnSendStatus2.BackColor; }
            set { btnSendStatus2.BackColor = value; btnSendStatus2.Refresh(); }
        }
        public Color btnSendStatus3Color
        {
            get { return btnSendStatus3.BackColor; }
            set { btnSendStatus3.BackColor = value; btnSendStatus3.Refresh(); }
        }
        public Color btnConnectColor
        {
            get { return btnConnect.BackColor; }
            set { btnConnect.BackColor = value; btnConnect.Refresh(); }
        }
        public Color btnCloseColor
        {
            get { return btnClose.BackColor; }
            set { btnClose.BackColor = value; btnClose.Refresh(); }
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

        private void btnOpenGreenFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Green Fix LEDs
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            GetAllControl(this.panelGreenFixLED, list);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    OpenFixLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnCloseGreenFixLED_Click(object sender, EventArgs e)
        {
            // Turn off all Green LEDs
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            GetAllControl(this.panelGreenFixLED, list);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    CloseFixLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnOpenRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Fix LEDs
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            GetAllControl(this.panelRedFixLED, list);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    OpenFixLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnCloseRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn off all Red Fix LEDs
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            GetAllControl(this.panelRedFixLED, list);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    CloseFixLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnOpenDarkRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn on all DarkRed Fix LEDs
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            GetAllControl(this.panelDarkRedFixLED, list);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    OpenFixLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnCloseDarkRedFixLED_Click(object sender, EventArgs e)
        {
            // Turn off all DarkRed LEDs
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            GetAllControl(this.panelDarkRedFixLED, list);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    CloseFixLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnOpenDimGreenLED1_Click(object sender, EventArgs e)
        {
            // Turn on 1st Green Dimmable LED
            try
            {
                Button btn = btnDimLED1;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimGreenLED2_Click(object sender, EventArgs e)
        {
            // Turn off 1st Green Dimmable LED
            try
            {
                Button btn = btnDimLED2;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimGreenLED3_Click(object sender, EventArgs e)
        {
            // Turn on 3 Green Dimmable LED
            try
            {
                Button btn = btnDimLED3;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimGreenLED4_Click(object sender, EventArgs e)
        {
            // Turn off 4 Green Dimmable LED
            try
            {
                Button btn = btnDimLED4;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimRedLED1_Click(object sender, EventArgs e)
        {
            // Turn on 1st Red Dimmable LED
            try
            {
                Button btn = btnDimLED5;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimRedLED2_Click(object sender, EventArgs e)
        {
            // Turn on 2 Red Dimmable LED
            try
            {
                Button btn = btnDimLED6;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimRedLED3_Click(object sender, EventArgs e)
        {
            // Turn on 3 Red Dimmable LED
            try
            {
                Button btn = btnDimLED7;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimRedLED4_Click(object sender, EventArgs e)
        {
            // Turn on 4 Red Dimmable LED
            try
            {
                Button btn = btnDimLED8;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimDarkRedLED1_Click(object sender, EventArgs e)
        {
            // Turn on 1st DarkRed Dimmable LED
            try
            {
                Button btn = btnDimLED9;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimDarkRedLED2_Click(object sender, EventArgs e)
        {
            // Turn on 2 DarkRed Dimmable LED
            try
            {
                Button btn = btnDimLED10;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimDarkRedLED3_Click(object sender, EventArgs e)
        {
            // Turn on 3 DarkRed Dimmable LED
            try
            {
                Button btn = btnDimLED11;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenDimDarkRedLED4_Click(object sender, EventArgs e)
        {
            // Turn on 4 DarkRed Dimmable LED
            try
            {
                Button btn = btnDimLED12;
                OpenDimLED?.Invoke(btn, e);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnOpenGreenDimLED_Click(object sender, EventArgs e)
        {
            // Turn on all Green Dimmable LED
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            list.Add(btnDimLED1);
            list.Add(btnDimLED2);
            list.Add(btnDimLED3);
            list.Add(btnDimLED4);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    OpenDimLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnCloseGreenDimLED_Click(object sender, EventArgs e)
        {
            // Turn off all Green Dimmable LED
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            list.Add(btnDimLED1);
            list.Add(btnDimLED2);
            list.Add(btnDimLED3);
            list.Add(btnDimLED4);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    CloseDimLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnOpenRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn on all Red Dimmable LED
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            list.Add(btnDimLED5);
            list.Add(btnDimLED6);
            list.Add(btnDimLED7);
            list.Add(btnDimLED8);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    OpenDimLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnCloseRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn off all Red Dimmable LED
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            list.Add(btnDimLED5);
            list.Add(btnDimLED6);
            list.Add(btnDimLED7);
            list.Add(btnDimLED8);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    CloseDimLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnOpenDarkRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn on all DarkRed Dimmable LED
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            list.Add(btnDimLED9);
            list.Add(btnDimLED10);
            list.Add(btnDimLED11);
            list.Add(btnDimLED12);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    OpenDimLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void btnCloseDarkRedDimLED_Click(object sender, EventArgs e)
        {
            // Turn off all DarkRed Dimmable LED
            Cursor.Current = Cursors.WaitCursor;
            List<Control> list = new List<Control>();

            list.Add(btnDimLED9);
            list.Add(btnDimLED10);
            list.Add(btnDimLED11);
            list.Add(btnDimLED12);

            foreach (Control control in list)
            {
                if (control.GetType() == typeof(Button))
                {

                    CloseDimLED?.Invoke(control, e);
                    Thread.Sleep(100);
                }
            }
            Cursor.Current = Cursors.Default;
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
            HandleFixLED?.Invoke(sender, e);
        }

        private void btnLED_MouseHover(object sender, EventArgs e)
        {
            // Show LED status
            ShowSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnLED_MouseLeave(object sender, EventArgs e)
        {
            // Clear LED status
            ClearSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnDimLED_Click(object sender, EventArgs e)
        {
            // Turn on Green Dimmable LED
            HandleDimLED?.Invoke(sender, e);
        }

        private void btnDimLED_MouseLeave(object sender, EventArgs e)
        {
            ClearSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnDimLED_MouseHover(object sender, EventArgs e)
        {
            // Show LED status
            ShowSingleLEDStatus?.Invoke(sender, e);
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
    }
}
