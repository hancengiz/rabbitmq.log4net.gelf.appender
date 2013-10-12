using System.Threading;
using NUnit.Framework;
using Newtonsoft.Json;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using rabbitmq.log4net.gelf.appender;

namespace tests
{
    [TestFixture]
    public class GelfRabbitMqAdapterIntegrationTests
    {
        private GelfRabbitMqAppender appender;
        private TestingRabbitListener testingRabbitListener;
        private ILog logger;

        [SetUp]
        public void SetUp()
        {
            appender = new GelfRabbitMqAppender
                           {
                               Threshold = Level.Error,
                               HostName = "localhost",
                               VirtualHost = "/",
                               Name = "GelfRabbitMQAdapter",
                               Port = 5672,
                               Exchange = "tests.log4net.gelf.appender",
                               Username = "guest",
                               Password = "guest",
                               Facility = "test-system"
                           };
            appender.ActivateOptions();

            var root = ((Hierarchy)LogManager.GetRepository()).Root;
            root.AddAppender(appender);
            root.Repository.Configured = true;
            logger = LogManager.GetLogger(GetType());

            testingRabbitListener = new TestingRabbitListener();
        }

        [TearDown]
        public void TearDown()
        {
            testingRabbitListener.Dispose();
            LogManager.Shutdown();
        }

        [Test]
        public void ShouldPublishGelfMessage_WhenLog4NetLogsEvent()
        {
            const string message = "should be published to rabbit";

            logger.Error(message);
            Thread.Sleep(200);

            Assert.That(testingRabbitListener.ReceivedMessages.Count, Is.EqualTo(1));
            var receivedMessage = testingRabbitListener.ReceivedMessages[0];
            var gelfMessage = JsonConvert.DeserializeObject<GelfMessage>(receivedMessage);
            Assert.That(gelfMessage.FullMessage, Is.EqualTo(message));
            Assert.That(gelfMessage.Facility, Is.EqualTo("test-system"));
        } 
        
        [Test]
        public void ShouldNotPublishInfoLevelsWhenAppenderLogLevelIsError()
        {
            const string message = "should not be published to rabbit";

            logger.Info(message);
            Thread.Sleep(200);

            Assert.That(testingRabbitListener.ReceivedMessages.Count, Is.EqualTo(0));
        }
    }
}