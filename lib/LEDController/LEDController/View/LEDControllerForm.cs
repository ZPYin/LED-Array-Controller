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
        public Color btnRecStatus1Color
        {
            get { return btnRecStatus1.BackColor; }
            set { btnRecStatus1.BackColor = value; }
        }
        public Color btnRecStatus2Color
        {
            get { return btnRecStatus2.BackColor; }
            set { btnRecStatus2.BackColor = value; }
        }
        public Color btnRecStatus3Color
        {
            get { return btnRecStatus3.BackColor; }
            set { btnRecStatus3.BackColor = value; }
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
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
    }
}
