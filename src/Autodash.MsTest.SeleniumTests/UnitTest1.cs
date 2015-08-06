using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Autodash.MsTest.SeleniumTests
{
    //http://localhost:4444/grid/api/proxy?id=http://10.240.240.74:5555
    [TestClass]
    public class UnitTest1
    {
        private const string GridHubUrl = "http://alexappvm.cloudapp.net:4444/wd/hub";
        
        [TestMethod]
        public void TestMethod1()
        {
            IWebDriver driver = GetDriver();
            driver.Navigate().GoToUrl("https://www.etsy.com/");
            var txt = driver.FindElement(By.Id("search-query"));
            txt.SendKeys("jewelry");
            txt.SendKeys(OpenQA.Selenium.Keys.Enter);
            Thread.Sleep(TimeSpan.FromSeconds(2));
            bool b = driver.Url.Contains("jewelry");

            var screenshooter = ((ITakesScreenshot) driver);
            Screenshot screenshot = screenshooter.GetScreenshot();
            screenshot.SaveAsFile(Path.Combine(Environment.CurrentDirectory, "screenshot.png"), ImageFormat.Png);

            Assert.IsTrue(b);
            driver.Quit();
            driver.Dispose();
        }


        private static IWebDriver GetDriver()
        {
            string hubUrl = Environment.GetEnvironmentVariable("hubUrl") ?? GridHubUrl;
            string browserName = Environment.GetEnvironmentVariable("browserName") ?? "internet explorer";
            string browserVersion = Environment.GetEnvironmentVariable("browserVersion");

            Console.WriteLine("Hub Url: " + hubUrl);
            Console.WriteLine("Browser Name: " + browserName);
            DesiredCapabilities capabilities;
            switch (browserName)
            {
                case "chrome":
                    capabilities = DesiredCapabilities.Chrome();
                    break;
                case "internet explorer":
                    capabilities = DesiredCapabilities.InternetExplorer();
                    break;
                case "firefox":
                    capabilities = DesiredCapabilities.Firefox();
                    break;
                default:
                    throw new InvalidOperationException("Unknown browser");
            }

            capabilities.SetCapability(CapabilityType.Version, "9");
            IWebDriver driver = new RemoteWebDriver(new Uri(hubUrl), capabilities);
            return driver;
        }
    }
}
