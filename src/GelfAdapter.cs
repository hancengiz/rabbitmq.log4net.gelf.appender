using System;
using System.Collections.Generic;
using System.Linq;
using log4net.Core;
using rabbitmq.log4net.gelf.appender.MessageFormatters;

namespace rabbitmq.log4net.gelf.appender
{
    public class GelfAdapter
    {
        private readonly GelfLogLevelMapper gelfLogLevelMapper;
        private readonly IList<IGelfMessageFormatter> messageObjectFormatters;

        public GelfAdapter() : this(new GelfLogLevelMapper()) { }

        public GelfAdapter(GelfLogLevelMapper gelfLogLevelMapper)
            : this(gelfLogLevelMapper, new List<IGelfMessageFormatter>
                                           {
                                               new StringGelfMessageFormatter(),
                                               new DictionaryGelfMessageFormatter(),
                                               new GenericObjectGelfMessageFormatter(),
                                           }) { }

        public GelfAdapter(GelfLogLevelMapper gelfLogLevelMapper, IList<IGelfMessageFormatter> messageObjectFormatters)
        {
            this.gelfLogLevelMapper = gelfLogLevelMapper;
            this.messageObjectFormatters = messageObjectFormatters;
        }

        public GelfMessage Adapt(LoggingEvent loggingEvent)
        {
            var gelfMessage = GelfMessage.EmptyGelfMessage();
            gelfMessage.Level = gelfLogLevelMapper.Map(loggingEvent.Level);
            gelfMessage.Timestamp = loggingEvent.TimeStamp;
            gelfMessage.File = loggingEvent.LocationInformation.FileName;
            gelfMessage.Line = loggingEvent.LocationInformation.LineNumber;
            FormatGelfMessage(gelfMessage, loggingEvent);
            return gelfMessage;
        }

        private void FormatGelfMessage(GelfMessage gelfMessage, LoggingEvent loggingEvent)
        {
            messageObjectFormatters.First(x => x.CanApply(loggingEvent.MessageObject))
                .Format(gelfMessage, loggingEvent.MessageObject);

            AppendExceptionStringIfExists(gelfMessage, loggingEvent);
        }

        private void AppendExceptionStringIfExists(GelfMessage gelfMessage, LoggingEvent loggingEvent)
        {
            if (loggingEvent.ExceptionObject != null)
                gelfMessage.FullMessage = string.Format("{0}\n{1}", gelfMessage.FullMessage,
                                                        loggingEvent.GetExceptionString());
        }
    }
}