using System;

namespace rabbitmq.log4net.gelf.appender
{
    public class KnownFacility : IKnowAboutConfiguredFacility
    {
        private readonly string _facility;

        public KnownFacility(string facility)
        {
            _facility = facility;
        }

        public void UseToCall(Action<string> facilitySettingAction)
        {
            facilitySettingAction(_facility);
        }
    }
}