using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

namespace LEDController.View
{
    interface ILEDControllerForm
    {
        string slaveIP { get; set; }
        string slavePort { get; set; }
        string tbxConnectMsgText { get; set; }
        string testCmdStr { get; set; }
        string toolStripConnectionStatusText { get; set; }
        Color btnRecStatus1Color { get; set; }
        Color btnRecStatus2Color { get; set; }
        Color btnRecStatus3Color { get; set; }
        string testMsgRecStr { get; set; }
        event EventHandler<EventArgs> Connect;
        event EventHandler<EventArgs> CloseConnect;
        event EventHandler<EventArgs> SendTestData;
        IAsyncResult BeginInvoke(Delegate method);
        object Invoke(Delegate method);
    }
}
