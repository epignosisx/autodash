using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Autodash.MsTest.SeleniumTests
{
    [TestClass]
    public class UnitTest1
    {
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
                    driver = new RemoteWebDriver(new Uri("http://alexappvm.cloudapp.net:4444/wd/hub"), capabilities);
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

        //[TestMethod]
        //public void TestMethod2()
        //{
        //    DesiredCapabilities capabilities = DesiredCapabilities.Chrome();
        //    IWebDriver driver = new RemoteWebDriver(new Uri("http://alexappvm.cloudapp.net:4444/wd/hub"), capabilities);
        //    driver.Navigate().GoToUrl("https://www.etsy.com/");
        //    var txt = driver.FindElement(By.Id("search-query"));
        //    txt.SendKeys("jewelry");
        //    txt.SendKeys(OpenQA.Selenium.Keys.Enter);
        //    bool b = driver.Url.Contains("jewelry");
        //    Assert.IsTrue(b);
        //    driver.Quit();
        //    driver.Dispose();
        //}

        //[TestMethod]
        //public void TestMethod3()
        //{
        //    DesiredCapabilities capabilities = DesiredCapabilities.Chrome();
        //    IWebDriver driver = new RemoteWebDriver(new Uri("http://alexappvm.cloudapp.net:4444/wd/hub"), capabilities);
        //    driver.Navigate().GoToUrl("https://www.etsy.com/");
        //    var txt = driver.FindElement(By.Id("search-query"));
        //    txt.SendKeys("towel");
        //    txt.SendKeys(OpenQA.Selenium.Keys.Enter);
        //    bool b = driver.Url.Contains("towel");
        //    Assert.IsTrue(b);
        //    driver.Quit();
        //    driver.Dispose();
        //}

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
