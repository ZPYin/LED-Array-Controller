using Microsoft.VisualStudio.TestTools.UnitTesting;
using LEDController.Model;
using System.IO;
using System;

namespace LEDControllerTest
{
    [TestClass]
    public class LEDBoardComTests
    {
        [TestMethod]
        public void TestReadINICfg()
        {
            string workingDir = Environment.CurrentDirectory;
            string projectDir = Directory.GetParent(workingDir).Parent.Parent.FullName;

            string iniTestFile = Path.Combine(projectDir, "testCfg.ini");

            Config cfgReader = new Config(iniTestFile);

            Assert.AreEqual(cfgReader.configData["name"], "zhenping");
        }
    }
}
