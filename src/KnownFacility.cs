using System;

namespace rabbitmq.log4net.gelf.appender
{
    public class KnownFacility : IKnowAboutConfiguredFacility
    {
        private readonly string facility;

        public KnownFacility(string facility)
        {
            this.facility = facility;
        }

        public void UseToCall(Action<string> facilitySettingAction)
        {
            facilitySettingAction(facility);
        }
    }
}