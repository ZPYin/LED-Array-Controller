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
        string toolStripLEDStatusText { get; set; }
        string lblGreenLEDMaxLeftText { get; set; }
        string lblGreenLEDMinLeftText { get; set; }
        string lblRedLEDMaxLeftText { get; set; }
        string lblRedLEDMinLeftText { get; set; }
        string lblDarkRedLEDMaxLeftText { get; set; }
        string lblDarkRedLEDMinLeftText { get; set; }
        string lblGreenLEDMaxRightText { get; set; }
        string lblGreenLEDMinRightText { get; set; }
        string lblRedLEDMaxRightText { get; set; }
        string lblRedLEDMinRightText { get; set; }
        string lblDarkRedLEDMaxRightText { get; set; }
        string lblDarkRedLEDMinRightText { get; set; }
        int sbarDimLED1Value { get; set; }
        int sbarDimLED2Value { get; set; }
        int sbarDimLED3Value { get; set; }
        int sbarDimLED4Value { get; set; }
        int sbarDimLED5Value { get; set; }
        int sbarDimLED6Value { get; set; }
        int sbarDimLED7Value { get; set; }
        int sbarDimLED8Value { get; set; }
        int sbarDimLED9Value { get; set; }
        int sbarDimLED10Value { get; set; }
        int sbarDimLED11Value { get; set; }
        int sbarDimLED12Value { get; set; }
        string tbxDimLED1Text { get; set; }
        string tbxDimLED2Text { get; set; }
        string tbxDimLED3Text { get; set; }
        string tbxDimLED4Text { get; set; }
        string tbxDimLED5Text { get; set; }
        string tbxDimLED6Text { get; set; }
        string tbxDimLED7Text { get; set; }
        string tbxDimLED8Text { get; set; }
        string tbxDimLED9Text { get; set; }
        string tbxDimLED10Text { get; set; }
        string tbxDimLED11Text { get; set; }
        string tbxDimLED12Text { get; set; }
        string tsslGreenLEDTPText { get; set; }
        string tsslRedLEDTPText { get; set; }
        string tsslDarkRedTPText { get; set; }
        string tsslTemp1Text { get; set; }
        string tsslTemp2Text { get; set; }
        string tsslTemp3Text { get; set; }
        string tsslTemp4Text { get; set; }
        int queryParamSelectItem { get; set; }
        int queryWaitTimeSelectItem { get; set; }
        double minLEDStatusValue { get; set; }
        double maxLEDStatusValue { get; set; }
        Color btnRecStatus1Color { get; set; }
        Color btnRecStatus2Color { get; set; }
        Color btnRecStatus3Color { get; set; }
        Color btnSendStatus1Color { get; set; }
        Color btnSendStatus2Color { get; set; }
        Color btnSendStatus3Color { get; set; }
        Color btnConnectColor { get; set; }
        Color btnCloseColor { get; set; }
        Color[] LEDStatusColors { get; set; }
        string testMsgRecStr { get; set; }
        event EventHandler<EventArgs> Connect;
        event EventHandler<EventArgs> CloseConnect;
        event EventHandler<EventArgs> SendTestData;
        event EventHandler<EventEDArgs> ShowSingleLEDStatus;
        event EventHandler<EventEDArgs> ClearSingleLEDStatus;
        event EventHandler<EventFixLEDArgs> OpenFixLED;
        event EventHandler<EventFixLEDArgs> CloseFixLED;
        event EventHandler<EventFixLEDArgs> HandleFixLED;
        event EventHandler<EventDimLEDArgs> OpenDimLED;
        event EventHandler<EventDimLEDArgs> CloseDimLED;
        event EventHandler<EventDimLEDArgs> HandleDimLED;
        event EventHandler<EventArgs> UpdateScrollBar;
        event EventHandler<EventArgs> UpdateLEDTbx;   // update Dimmable led text box
        event EventHandler<EventArgs> ShowLEDStatus;
        object Invoke(Delegate method);
    }
}
