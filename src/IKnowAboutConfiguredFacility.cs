using System;

namespace rabbitmq.log4net.gelf.appender
{
    public interface IKnowAboutConfiguredFacility
    {
        void UseToCall(Action<string> facilitySettingAction);
    }
}
