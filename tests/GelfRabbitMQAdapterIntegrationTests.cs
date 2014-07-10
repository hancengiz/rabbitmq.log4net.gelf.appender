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
        private GelfRabbitMqAppender _appender;
        private TestingRabbitListener _testingRabbitListener;
        private ILog _logger;

        [SetUp]
        public void SetUp()
        {
            _appender = new GelfRabbitMqAppender
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
            _appender.ActivateOptions();

            var root = ((Hierarchy)LogManager.GetRepository()).Root;
            root.AddAppender(_appender);
            root.Repository.Configured = true;
            _logger = LogManager.GetLogger(GetType());

            _testingRabbitListener = new TestingRabbitListener();
        }

        [TearDown]
        public void TearDown()
        {
            _testingRabbitListener.Dispose();
            LogManager.Shutdown();
        }

        [Test]
        public void ShouldPublishGelfMessage_WhenLog4NetLogsEvent()
        {
            const string message = "should be published to rabbit";

            _logger.Error(message);
            Thread.Sleep(200);

            Assert.That(_testingRabbitListener.ReceivedMessages.Count, Is.EqualTo(1));
            var receivedMessage = _testingRabbitListener.ReceivedMessages[0];
            var gelfMessage = JsonConvert.DeserializeObject<GelfMessage>(receivedMessage);
            Assert.That(gelfMessage.ShortMessage, Is.EqualTo(message));
            Assert.That(gelfMessage.Facility, Is.EqualTo("test-system"));
        } 
        
        [Test]
        public void ShouldNotPublishInfoLevelsWhenAppenderLogLevelIsError()
        {
            const string message = "should not be published to rabbit";

            _logger.Info(message);
            Thread.Sleep(200);

            Assert.That(_testingRabbitListener.ReceivedMessages.Count, Is.EqualTo(0));
        }
    }
}