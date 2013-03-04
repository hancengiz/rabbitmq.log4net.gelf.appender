using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Appender;
using log4net.Core;
using log4net.Util;

namespace rabbitmq.log4net.gelf.appender
{
    public class GelfAdapter
    {
        private readonly GelfLogLevelMapper gelfLogLevelMapper;
        private readonly IList<IGelfMessageFormatter> messageObjectFormatters;

        public GelfAdapter() : this(new GelfLogLevelMapper()) { }
        public GelfAdapter(GelfLogLevelMapper gelfLogLevelMapper) : this(gelfLogLevelMapper, new List<IGelfMessageFormatter>
                                                                                                 {
                                                                                                     new StringGelfMessageFormatter(),
                                                                                                     new DictionaryGelfMessageFormatter()
                                                                                                 }) { }
        public GelfAdapter(GelfLogLevelMapper gelfLogLevelMapper, IList<IGelfMessageFormatter> messageObjectFormatters)
        {
            this.gelfLogLevelMapper = gelfLogLevelMapper;
            this.messageObjectFormatters = messageObjectFormatters;
        }

        public GelfMessage Adapt(LoggingEvent loggingEvent)
        {
            var gelfMessage = new GelfMessage()
                       {
                           Facility = "GELF",
                           Version = "1.0",
                           Host = Environment.MachineName,
                           Level = gelfLogLevelMapper.Map(loggingEvent.Level),
                           Timestamp = loggingEvent.TimeStamp,
                           File = "",
                           Line = ""
                       };
            FormatGelfMessage(gelfMessage, loggingEvent);
            return gelfMessage;
        }

        private void FormatGelfMessage(GelfMessage gelfMessage, LoggingEvent loggingEvent)
        {
            messageObjectFormatters.First(x => x.CanApply(loggingEvent.MessageObject)).Format(gelfMessage, loggingEvent.MessageObject);

            if (loggingEvent.ExceptionObject != null)
                gelfMessage.FullMessage = string.Format("{0}\n{1}", gelfMessage.FullMessage, loggingEvent.GetExceptionString());
        }


        public interface IGelfMessageFormatter
        {
            bool CanApply(object messageObject);
            void Format(GelfMessage gelfMessage, object messageObject);
        }

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

        public class DictionaryGelfMessageFormatter : IGelfMessageFormatter
        {
            private static readonly IEnumerable<string> FullMessageKeyValues = new[] { "FULLMESSAGE", "FULL_MESSAGE", "MESSAGE" };
            private static readonly IEnumerable<string> ShortMessageKeyValues = new[] { "SHORTMESSAGE", "SHORT_MESSAGE", "MESSAGE" };

            public bool CanApply(object messageObject)
            {
                return messageObject is IDictionary;
            }

            public void Format(GelfMessage gelfMessage, object messageObject)
            {
                foreach (DictionaryEntry entry in (IDictionary)messageObject)
                {
                    var key = (entry.Key ?? string.Empty).ToString();
                    var value = (entry.Value ?? string.Empty).ToString();
                    if (FullMessageKeyValues.Contains(key, StringComparer.OrdinalIgnoreCase))
                        gelfMessage.FullMessage = value;
                    else if (ShortMessageKeyValues.Contains(key, StringComparer.OrdinalIgnoreCase))
                        gelfMessage.ShortMessage = value.ShortenMessage();
                    else
                    {
                        key = key.StartsWith("_") ? key : "_" + key;
                        gelfMessage[key] = value;
                    }
                }
            }
        }
    }
}