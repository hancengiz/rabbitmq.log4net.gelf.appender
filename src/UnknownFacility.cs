using System;

namespace rabbitmq.log4net.gelf.appender
{
    internal class UnknownFacility : IKnowAboutConfiguredFacility
    {
        public void UseToCall(Action<string> facilitySettingAction)
        {
        }
    }
}