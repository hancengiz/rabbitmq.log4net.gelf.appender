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
    public class GelfRabbitMQAdapterIntegrationTests
    {
        private GelfRabbitMQAdapter appender;
        private TestingSubscriber testingSubscriber;

        [SetUp]
        public void SetUp()
        {
            appender = new GelfRabbitMQAdapter
                           {
                               Threshold = Level.All,
                               HostName = "localhost",
                               VirtualHost = "/",
                               Name = "GelfRabbitMQAdapter",
                               Port = 5672,
                               Exchange = "tests.log4net.gelf.appender",
                               Username = "guest",
                               Password = "guest"
                           };
            appender.ActivateOptions();

            var root = ((Hierarchy)LogManager.GetRepository()).Root;
            root.AddAppender(appender);
            root.Repository.Configured = true;
            testingSubscriber = new TestingSubscriber();
        }

        [TearDown]
        public void TearDown()
        {
            var root = ((Hierarchy)LogManager.GetRepository()).Root;
            root.RemoveAppender(appender);
            root.Repository.Configured = true;
            testingSubscriber.Dispose();
            appender.Close();
        }

        [Test]
        public void ShouldPublishGelfMessage_WhenLog4NetLogsEvent()
        {
            const string message = "should be published to rabbit";

            var logger = LogManager.GetLogger(GetType());
            logger.Info(message);
            Thread.Sleep(200);

            Assert.That(testingSubscriber.ReceivedMessages.Count, Is.EqualTo(1));
            var receivedMessage = testingSubscriber.ReceivedMessages[0];
            var gelfMessage = JsonConvert.DeserializeObject<GelfMessage>(receivedMessage);
            Assert.That(gelfMessage.FullMessage, Is.EqualTo(message));
        }
    }
}