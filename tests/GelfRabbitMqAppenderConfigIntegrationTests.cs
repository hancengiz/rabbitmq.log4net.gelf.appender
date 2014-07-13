using System.Linq;
using System.Xml;
using log4net;
using NUnit.Framework;
using rabbitmq.log4net.gelf.appender;

namespace tests
{
    [TestFixture]
    public class GelfRabbitMqAppenderConfigIntegrationTests
    {

        [Test]
        public void SetsAppenderPropertiesFromConfig()
        {
            var doc = CreateLog4NetXmlConfigurationDocument(facility: "test-system");

            log4net.Config.XmlConfigurator.Configure(doc.DocumentElement);

            var appender = LogManager.GetRepository().GetAppenders().First();
            var gelfAppender = (GelfRabbitMqAppender) appender;

            Assert.That(gelfAppender, Is.Not.Null);
            Assert.That(gelfAppender.VirtualHost, Is.EqualTo("/"));
            Assert.That(gelfAppender.Facility, Is.EqualTo("test-system"));

            LogManager.Shutdown();
        }

        private static XmlDocument CreateLog4NetXmlConfigurationDocument(string facility)
        {
            const string appenderName = "rabbitmq.gelf.appender";

            var doc = new XmlDocument();
            var log4netElement = doc.AppendChild(doc.CreateElement("log4net"));
  
            var appenderElement = AddAppender(log4netElement, doc, appenderName);

            AddAppenderProprty(appenderElement, doc, "VirtualHost", "/");
            AddAppenderProprty(appenderElement, doc, "Facility", facility);

            AddRootLoggingElement(doc, appenderName, log4netElement);
            return doc;
        }

        private static XmlElement AddAppender(XmlNode log4netElement, XmlDocument doc, string appenderName)
        {
            var appenderElement = (XmlElement) log4netElement.AppendChild(doc.CreateElement("appender"));
            appenderElement.SetAttribute("name", appenderName);
            appenderElement.SetAttribute("type",
                "rabbitmq.log4net.gelf.appender.GelfRabbitMqAppender, rabbitmq.log4net.gelf.appender");
            return appenderElement;
        }

        private static void AddRootLoggingElement(XmlDocument doc, string appenderName, XmlNode log4netElement)
        {
            var appenderRefElement = doc.CreateElement("appender-ref");
            appenderRefElement.SetAttribute("ref", appenderName);

            log4netElement.AppendChild(doc.CreateElement("root")).AppendChild(appenderRefElement);
        }

        private static void AddAppenderProprty(XmlElement appenderElement, XmlDocument doc, string propertyName, string propertyValue)
        {
            var vhostElement = (XmlElement) appenderElement.AppendChild(doc.CreateElement(propertyName));
            vhostElement.SetAttribute("value", propertyValue);
        }
    }
}