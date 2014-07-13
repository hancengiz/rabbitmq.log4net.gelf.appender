using System;
using System.Reflection;
using NUnit.Framework;
using rabbitmq.log4net.gelf.appender;

namespace tests
{
    [TestFixture]
    public class AmqpClientPropertiesTests
    {
        [Test]
        public void CreatesDefaultClientPropertiesAccordingToStandard()
        {
            IKnowAboutConfiguredFacility noFacilityInformation = new StubFacilityInformation();
            var clientProperties = AmqpClientProperties.WithFacility(noFacilityInformation);

            var expectedVersion = Assembly.GetAssembly(typeof (AmqpClientProperties)).GetName().Version;

            Assert.That(clientProperties["product"], Is.EqualTo("rabbitmq.log4net.gelf.appender"));
            Assert.That(clientProperties["version"], Is.EqualTo(expectedVersion.ToString()));
            Assert.That(clientProperties["platform"], Is.EqualTo(Environment.OSVersion.Platform.ToString()));
        }

        [Test]
        public void SetsFacilityIfInformationIsAvailable()
        {
            IKnowAboutConfiguredFacility knownFacility = new KnownFacility("test");
            var clientProperties = AmqpClientProperties.WithFacility(knownFacility);

            Assert.That(clientProperties["facility"], Is.EqualTo("test"));
        }

        [Test]
        public void ProvidesHostNameInClientProperties()
        {
            var clientProperties = AmqpClientProperties.WithFacility(new StubFacilityInformation());

            Assert.That(clientProperties["host"], Is.EqualTo(Environment.MachineName));
        }

        public class KnownFacility : IKnowAboutConfiguredFacility
        {
            private readonly string facilityName;

            public KnownFacility(string facilityName)
            {
                this.facilityName = facilityName;
            }

            public void UseToCall(Action<string> facilitySettingAction)
            {
                facilitySettingAction(facilityName);
            }
        }

        public class StubFacilityInformation : IKnowAboutConfiguredFacility
        {
            public void UseToCall(Action<string> facilitySettingAction)
            {
            }
        }
    }
}
