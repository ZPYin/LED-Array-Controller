using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LEDController.View;
using LEDController.Model;
using System.Threading;
using System.Net.Sockets;
using System.Drawing;

namespace LEDController.Presenter
{
    class LEDControllerPresenter
    {
        private ILEDControllerForm _view;
        private LEDBoardCom connector;
        private FileSysIOClass fileIOer;
        private Thread threadListen = null;
        private string recMsg = null;
        private const int SendBufferSize = 2 * 1024;
        private const int RecBufferSize = 8 * 1024;

        public LEDControllerPresenter(ILEDControllerForm newView)
        {
            _view = newView;
            _view.Connect += new EventHandler<EventArgs>(Connect);
            _view.CloseConnect += new EventHandler<EventArgs>(CloseConnect);
            _view.SendTestData += new EventHandler<EventArgs>(SendTestData);
        }

        private void SendTestData(object sender, EventArgs e)
        {
            try
            {
                connector.SendCmd(_view.testCmdStr);
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
                        // TODO add timestamp
                        recMsg = Encoding.UTF8.GetString(buffer);

                        // format recMsg
                        string formatRecMsg = "[" + GetCurrentTime() + "] " + "\r\n" + recMsg + "\r\n";
                        _view.Invoke(new Action(() => { _view.testMsgRecStr = formatRecMsg; }));

                        // showing receiving status with colorful light
                        _view.btnRecStatus1Color = Color.Green;

                        Thread.Sleep(100);
                        _view.btnRecStatus2Color = Color.Green;

                        Thread.Sleep(100);
                        _view.btnRecStatus3Color = Color.Green;

                        Thread.Sleep(200);
                        _view.btnRecStatus1Color = Color.DarkRed;
                        _view.btnRecStatus2Color = Color.DarkRed;
                        _view.btnRecStatus3Color = Color.DarkRed;
                    }
                }
                catch (SocketException ex)
                {
                    // TODO: Write Log
                    throw ex;
                }
            }
        }

        private void Connect(object sender, EventArgs e)
        {
            connector = new LEDBoardCom(_view.slaveIP, _view.slavePort);

            try
            {
                connector.Connect();
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
