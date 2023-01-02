using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using ScottPlot;

namespace LEDController.View
{
    interface ILEDControllerForm
    {
        string toolStripLEDStatusText { get; set; }
        int queryParamSelectItem { get; set; }
        int queryWaitTimeSelectItem { get; set; }
        Color btnRecStatus1Color { get; set; }
        Color btnRecStatus2Color { get; set; }
        Color btnRecStatus3Color { get; set; }
        Color btnSendStatus1Color { get; set; }
        Color btnSendStatus2Color { get; set; }
        Color btnSendStatus3Color { get; set; }
        Color[] LEDStatusColors { get; set; }
        FormsPlot LEDFormsPlot {get; set; }
        event EventHandler<EventArgs> ConnectSerialPort;
        event EventHandler<EventArgs> CloseSerialPort;
        event EventHandler<EventArgs> ConnectTCP;
        event EventHandler<EventArgs> CloseTCP;
        event EventHandler<EventArgs> SendTestData;
        event EventHandler<EventLEDArgs> ShowFixLEDStatus;
        event EventHandler<EventDimLEDArgs> ShowDimLEDStatus;
        event EventHandler<EventLEDArgs> ClearFixLEDStatus;
        event EventHandler<EventDimLEDArgs> ClearDimLEDStatus;
        event EventHandler<EventLEDArgs> OpenFixLED;
        event EventHandler<EventLEDArgs> CloseFixLED;
        event EventHandler<EventLEDArgs> HandleFixLED;
        event EventHandler<EventDimLEDArgs> SetDimLED;
        event EventHandler<EventDimLEDArgs> CloseDimLED;
        event EventHandler<EventDimLEDArgs> HandleDimLED;
        event EventHandler<EventDimLEDArgs> UpdateScrollBar;
        event EventHandler<EventDimLEDArgs> UpdateLEDTbx;
        event EventHandler<EventArgs> ShowLEDStatus;
    }
}
