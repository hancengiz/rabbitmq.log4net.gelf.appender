using System.Collections.Generic;

namespace rabbitmq.log4net.gelf.appender.MessageFormatters
{
    public class StringGelfMessageFormatter : IGelfMessageFormatter
    {
        private const int MaximumShortMessageLength = 250;

        readonly List<string> stringTypeList = new List<string>
        {
            "System.String","log4net.Util.SystemStringFormat","Common.Logging.Factory.AbstractLogger+StringFormatFormattedMessage"
        };

        public bool CanApply(object messageObject)
        {
            return stringTypeList.Contains(messageObject.GetType().FullName);
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