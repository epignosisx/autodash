using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Autodash.Core
{
    public class ApplyTestSettingsPreProcessor : ITestRunnerPreProcessor
    {
        private static readonly Dictionary<string, string> BrowserMapper = new Dictionary<string, string>
        {
            { BrowserNames.SeleniumChrome, "Chrome" },
            { BrowserNames.SeleniumFirefox, "Firefox" },
            { BrowserNames.SeleniumIe, "IE" }
        };

        private readonly ILoggerWrapper _logger;

        public ApplyTestSettingsPreProcessor(ILoggerProvider loggerProvider)
        {
            _logger = loggerProvider.GetLogger(GetType().Name);
        }

        public void Process(TestRunnerPreProcessorContext context)
        {
            if (context == null) 
                throw new ArgumentNullException("context");

            var configFile = Path.Combine(context.TestDirectory, "config.xml");
            if (!File.Exists(configFile))
            {
                _logger.Error("config.xml file not found");
                return;
            }

            XDocument xDoc = null;
            using (var stream = File.OpenRead(configFile))
            {
                xDoc = XDocument.Load(stream);
            }
            
            //browser
            var browser = xDoc.XPathSelectElement("/framework/browser/name");
            browser.Value = BrowserMapper[context.NodeBrowser.BrowserName];

            //environment
            var environment = xDoc.XPathSelectElement("/framework/url/name");
            environment.Value = context.TestSuiteConfiguration.EnvironmentUrl;

            //hub
            var gridUrl = new Uri(context.GridConfiguration.HubUrl);
            var grid = xDoc.XPathSelectElement("/framework/grid");
            grid.Element("active").Value = "true";
            grid.Element("port").Value = gridUrl.Port.ToString(CultureInfo.InvariantCulture);
            grid.Element("ip").Value = gridUrl.Host.ToString(CultureInfo.InvariantCulture);

            xDoc.Save(configFile);
        }
    }
}