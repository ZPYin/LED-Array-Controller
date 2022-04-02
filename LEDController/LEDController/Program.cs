using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using LEDController.View;

namespace LEDController
{
    static class Program
    {
       /// <summary>
       /// 应用程序的主入口点。
       /// </summary>
       [STAThread]
       static void Main()
       {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var f1 = new LEDControllerViewer();
            var pr = new Presenter.LEDControllerPresenter(f1);

            Application.Run(f1);
       }
    }

}
