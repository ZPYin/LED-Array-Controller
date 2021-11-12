using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LEDController.View;
using LEDController.Model;

namespace LEDController.Presenter
{
    class LEDControllerPresenter
    {
        private ILEDControllerForm view;
        private LEDBoardCom connector;
        private FileSysIOClass fileIOer;

        public LEDControllerPresenter(ILEDControllerForm newView)
        {
            view = newView;

            // connector = new LEDBoardCom();
        }
    }
}
