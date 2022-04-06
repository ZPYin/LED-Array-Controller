using Microsoft.VisualStudio.TestTools.UnitTesting;
using LEDController.Model;
using System.Drawing;

namespace LEDControllerTest
{
    [TestClass]
    public class ValueConversionTests
    {
        [TestMethod]
        public void TestHSL2RGB()
        {
            double hue = 0.5;
            ColorConv.HSL2RGB(hue, 0.5, 0.5);
        }
    }
}
