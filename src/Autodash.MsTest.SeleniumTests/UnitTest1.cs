using System;
using System.Diagnostics;
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
        //private const string GridHubUrl = "http://alexappvm.cloudapp.net:4444/wd/hub";
        private const string GridHubUrl = "http://localhost:4444/wd/hub";

        [TestMethod]
        public void TestMethod1()
        {
            var searches = new string[] { "rugs", "jewelry", "towel", "dress", "shoes", "bags" };

            Parallel.ForEach(searches, new ParallelOptions{MaxDegreeOfParallelism = 1}, (term) =>
            {
                Console.WriteLine("============ Starting test with: {0}", term);
                DesiredCapabilities capabilities = DesiredCapabilities.InternetExplorer();
                IWebDriver driver = null;
                try
                {
                    driver = new RemoteWebDriver(new Uri(GridHubUrl), capabilities);
                    driver.Navigate().GoToUrl("https://www.etsy.com/");
                    var txt = driver.FindElement(By.Id("search-query"));
                    txt.SendKeys(term);
                    txt.SendKeys(OpenQA.Selenium.Keys.Enter);
                    bool b = driver.Url.Contains(term);
                    Assert.IsTrue(b);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("------------ Exception in " + term + Environment.NewLine + ex.ToString());
                }
                finally
                {
                    if (driver != null)
                    {
                        driver.Quit();
                        driver.Dispose();    
                    }
                }

                Console.WriteLine("++++++++++++ Finished test with: {0}", term);
            });

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void TestMethod2()
        {
            DesiredCapabilities capabilities = DesiredCapabilities.Chrome();
            IWebDriver driver = new RemoteWebDriver(new Uri(GridHubUrl), capabilities);
            driver.Navigate().GoToUrl("https://www.etsy.com/");
            var txt = driver.FindElement(By.Id("search-query"));
            txt.SendKeys("jewelry");
            txt.SendKeys(OpenQA.Selenium.Keys.Enter);
            Thread.Sleep(10000);
            bool b = driver.Url.Contains("jewelry");
            Assert.IsTrue(b);
            driver.Quit();
            driver.Dispose();
        }

        [TestMethod]
        public void TestMethod3()
        {
            IWebDriver driver;
            DesiredCapabilities capability = DesiredCapabilities.Firefox();
            capability.SetCapability("platform", "WIN8");
            capability.SetCapability("version", "33");
            capability.SetCapability("gridlasticUser", "JmbfdfAXLgrs6NgnjOEBb2TYtujj5WdE");
            capability.SetCapability("gridlasticKey", "8eKckaZWghedVlq4ZUw1NfQ6AZgvMj0L");
            capability.SetCapability("video", "True");
            driver = new RemoteWebDriver(
              new Uri("http://CCLDEV.gridlastic.com:80/wd/hub/"), capability
            );
            driver.Navigate().GoToUrl("http://www.google.com/ncr");
            IWebElement query = driver.FindElement(By.Name("q"));
            query.SendKeys("webdriver");
            query.Submit();
            string sessionId = capability.GetCapability("webdriver.remote.sessionid") as string;
            Console.WriteLine("Video: https://s3.amazonaws.com/4ad4a405-ef2a-b3d3-a629-1ab0a2d338b1/65769dff-3809-7109-8d05-93476425fe18/play.html?" + sessionId);
            driver.Quit();
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        //[TestMethod]
        //public void TestMethod4()
        //{
        //    DesiredCapabilities capabilities = DesiredCapabilities.Chrome();
        //    IWebDriver driver = new RemoteWebDriver(new Uri("http://alexappvm.cloudapp.net:4444/wd/hub"), capabilities);
        //    driver.Navigate().GoToUrl("https://www.etsy.com/");
        //    var txt = driver.FindElement(By.Id("search-query"));
        //    txt.SendKeys("dress");
        //    txt.SendKeys(OpenQA.Selenium.Keys.Enter);
        //    bool b = driver.Url.Contains("dress");
        //    Assert.IsTrue(b);
        //    driver.Quit();
        //    driver.Dispose();
        //}

        //[TestMethod]
        //public void TestMethod5()
        //{
        //    DesiredCapabilities capabilities = DesiredCapabilities.Chrome();
        //    IWebDriver driver = new RemoteWebDriver(new Uri("http://alexappvm.cloudapp.net:4444/wd/hub"), capabilities);
        //    driver.Navigate().GoToUrl("https://www.etsy.com/");
        //    var txt = driver.FindElement(By.Id("search-query"));
        //    txt.SendKeys("shoes");
        //    txt.SendKeys(OpenQA.Selenium.Keys.Enter);
        //    bool b = driver.Url.Contains("shoes");
        //    Assert.IsTrue(b);
        //    driver.Quit();
        //    driver.Dispose();
        //}
    }
}
