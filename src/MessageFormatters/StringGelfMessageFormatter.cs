using log4net.Util;

namespace rabbitmq.log4net.gelf.appender.MessageFormatters
{
    public class StringGelfMessageFormatter : IGelfMessageFormatter
    {
        public bool CanApply(object messageObject)
        {
            return (messageObject is string || messageObject is SystemStringFormat);
        }

        public void Format(GelfMessage gelfMessage, object messageObject)
        {
            string message = messageObject.ToString();
            gelfMessage.FullMessage = message;
            gelfMessage.ShortMessage = message.ShortenMessage();
        }
    }
}