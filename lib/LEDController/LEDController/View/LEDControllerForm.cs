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

namespace LEDController.View
{
    
    public partial class LEDControllerViewer : Form, ILEDControllerForm
    {
        public LEDControllerViewer()
        {
            InitializeComponent();
            this.KeyPreview = true;
        }

        public event EventHandler<EventArgs> Connect;
        public event EventHandler<EventArgs> CloseConnect;
        public event EventHandler<EventArgs> SendTestData;
        public event EventHandler<EventArgs> OpenFixLED;
        public event EventHandler<EventArgs> CloseFixLED;
        public event EventHandler<EventArgs> OpenDimLED;
        public event EventHandler<EventArgs> CloseDimLED;
        public event EventHandler<EventArgs> ShowSingleLEDStatus;
        public event EventHandler<EventArgs> ClearSingleLEDStatus;
        public event EventHandler<EventArgs> UpdateScrollBar;
        public event EventHandler<EventArgs> UpdateLEDTbx;
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void button94_Click(object sender, EventArgs e)
        {

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Connect?.Invoke(this, e);
                btnConnect.BackColor = Color.Green;
                btnClose.BackColor = Color.Empty;
            }
            catch (Exception ex)
            {
                btnConnect.BackColor = Color.Empty;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                CloseConnect?.Invoke(this, e);
                btnClose.BackColor = Color.Empty;
                btnConnect.BackColor = Color.Empty;
            }
            catch (Exception ex)
            {
                btnClose.BackColor = Color.Red;
            }
        }

        private void btnSendTestMsg_Click(object sender, EventArgs e)
        {
            SendTestData?.Invoke(sender, e);
        }


        private void CloseApplication(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnLED41_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn.BackColor.ToArgb().Equals(Color.Gray.ToArgb()))
                {
                    // Send control cmd
                    OpenFixLED?.Invoke(sender, e);
                }
                else if (btn.BackColor.ToArgb().Equals(Color.Red.ToArgb()))
                {
                    // Send control cmd
                    CloseFixLED?.Invoke(sender, e);
                }
                else
                {
                    // Exceptions
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnLED41_MouseHover(object sender, EventArgs e)
        {
            // show LED status
            ShowSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnLED41_MouseLeave(object sender, EventArgs e)
        {
            ClearSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnLED81_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn.BackColor.ToArgb().Equals(Color.Gray.ToArgb()))
                {
                    // Send control cmd
                    OpenFixLED?.Invoke(sender, e);
                }
                else if (btn.BackColor.ToArgb().Equals(Color.DarkRed.ToArgb()))
                {
                    // Send control cmd
                    CloseFixLED?.Invoke(sender, e);
                }
                else
                {
                    // Exceptions
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnDimLED9_MouseHover(object sender, EventArgs e)
        {
            // show LED status
            ShowSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnDimLED9_MouseLeave(object sender, EventArgs e)
        {
            ClearSingleLEDStatus?.Invoke(sender, e);
        }

        private void sbarDimLED1_Scroll(object sender, EventArgs e)
        {
            // update LED power textbox
            UpdateLEDTbx?.Invoke(sender, e);
        }

        private void sbarDimLED5_Scroll(object sender, EventArgs e)
        {
            // update LED power textbox
            UpdateLEDTbx?.Invoke(sender, e);
        }

        private void tbxDimLED1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // update scroll bar
                UpdateScrollBar?.Invoke(sender, e);
            }
        }

        private void btnLED81_MouseHover(object sender, EventArgs e)
        {
            // show LED status
            ShowSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnLED81_MouseLeave(object sender, EventArgs e)
        {
            ClearSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnDimLED1_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn.BackColor.ToArgb().Equals(Color.Gray.ToArgb()))
                {
                    // Send control cmd
                    OpenDimLED?.Invoke(sender, e);
                }
                else if (btn.BackColor.ToArgb().Equals(Color.Green.ToArgb()))
                {
                    // Send control cmd
                    CloseDimLED?.Invoke(sender, e);
                }
                else
                {
                    // Exceptions
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnDimLED1_MouseHover(object sender, EventArgs e)
        {
            // show LED status
            ShowSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnDimLED1_MouseLeave(object sender, EventArgs e)
        {
            ClearSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnDimLED5_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn.BackColor.ToArgb().Equals(Color.Gray.ToArgb()))
                {
                    // Send control cmd
                    OpenDimLED?.Invoke(sender, e);
                }
                else if (btn.BackColor.ToArgb().Equals(Color.Red.ToArgb()))
                {
                    // Send control cmd
                    CloseDimLED?.Invoke(sender, e);
                }
                else
                {
                    // Exceptions
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnDimLED5_MouseHover(object sender, EventArgs e)
        {
            // show LED status
            ShowSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnDimLED5_MouseLeave(object sender, EventArgs e)
        {
            ClearSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnDimLED9_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn.BackColor.ToArgb().Equals(Color.Gray.ToArgb()))
                {
                    // Send control cmd
                    OpenDimLED?.Invoke(sender, e);
                }
                else if (btn.BackColor.ToArgb().Equals(Color.DarkRed.ToArgb()))
                {
                    // Send control cmd
                    CloseDimLED?.Invoke(sender, e);
                }
                else
                {
                    // Exceptions
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void btnLED1_Click(object sender, EventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn.BackColor.ToArgb().Equals(Color.Gray.ToArgb()))
                {
                    // Send control cmd
                    OpenFixLED?.Invoke(sender, e);
                }
                else if (btn.BackColor.ToArgb().Equals(Color.Green.ToArgb()))
                {
                    // Send control cmd
                    CloseFixLED?.Invoke(sender, e);
                }
                else
                {
                    // Exceptions
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnLED1_MouseLeave(object sender, EventArgs e)
        {
            ClearSingleLEDStatus?.Invoke(sender, e);
        }

        private void btnLED1_MouseHover(object sender, EventArgs e)
        {
            ShowSingleLEDStatus?.Invoke(sender, e);
        }
        private void GetAllControl(Control c, List<Control> list)
        {
            foreach (Control control in c.Controls)
            {
                list.Add(control);

                if (control.GetType() == typeof(Panel))
                    GetAllControl(control, list);
            }
        }

        private void btnOpenGreenFixLED_Click(object sender, EventArgs e)
        {
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

        private void tsmRTControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 3;
        }

        private void tsmLightControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 4;
        }

        private void tsmAirConditionerControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 5;
        }

        private void tsmSkylightControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 6;
        }

        private void tsmCameraControl_Click(object sender, EventArgs e)
        {
            tabCtrlMain.SelectedIndex = 7;
        }
    }
}
