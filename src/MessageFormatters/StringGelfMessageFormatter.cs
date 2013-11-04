using log4net.Util;

namespace rabbitmq.log4net.gelf.appender.MessageFormatters
{
    public class StringGelfMessageFormatter : IGelfMessageFormatter
    {
        private const int MaximumShortMessageLength = 250;

        public bool CanApply(object messageObject)
        {
            return (messageObject is string || messageObject is SystemStringFormat);
        }

        public void Format(GelfMessage gelfMessage, object messageObject)
        {
            var message = messageObject.ToString();
            if (message.Length > MaximumShortMessageLength)
            {
                gelfMessage.FullMessage = message;
                gelfMessage.ShortMessage = message.TruncateString(MaximumShortMessageLength);
            }
            else
            {
                gelfMessage.ShortMessage = message;
            }
        }
    }
}