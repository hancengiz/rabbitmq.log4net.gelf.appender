namespace rabbitmq.log4net.gelf.appender.MessageFormatters
{
    public interface IGelfMessageFormatter
    {
        bool CanApply(object messageObject);
        void Format(GelfMessage gelfMessage, object messageObject);
    }
}