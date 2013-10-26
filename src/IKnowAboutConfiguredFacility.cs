using System;

namespace rabbitmq.log4net.gelf.appender
{
    public interface IKnowAboutConfiguredFacility
    {
        void UseTo(Action<string> facilitySettingAction);
    }
}
