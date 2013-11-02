using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using log4net.Core;
using rabbitmq.log4net.gelf.appender.MessageFormatters;

namespace rabbitmq.log4net.gelf.appender
{
    public class GelfAdapter
    {
        private readonly GelfLogLevelMapper gelfLogLevelMapper;
        private readonly IList<IGelfMessageFormatter> messageObjectFormatters;

        private static readonly ExceptionMessageFormatter ExceptionMessageFormatter = new ExceptionMessageFormatter();

        public GelfAdapter() : this(new GelfLogLevelMapper()) { }

        public GelfAdapter(GelfLogLevelMapper gelfLogLevelMapper)
            : this(gelfLogLevelMapper, new List<IGelfMessageFormatter>
                                           {
                                               new StringGelfMessageFormatter(),
                                               ExceptionMessageFormatter,
                                               new DictionaryGelfMessageFormatter(),
                                               new GenericObjectGelfMessageFormatter(),
                                           }) { }

        public GelfAdapter(GelfLogLevelMapper gelfLogLevelMapper, IList<IGelfMessageFormatter> messageObjectFormatters)
        {
            this.gelfLogLevelMapper = gelfLogLevelMapper;
            this.messageObjectFormatters = messageObjectFormatters;
        }

        public string Facility { private get; set; }

        public GelfMessage Adapt(LoggingEvent loggingEvent)
        {
            var gelfMessage = GelfMessage.EmptyGelfMessage;
            gelfMessage.Level = gelfLogLevelMapper.Map(loggingEvent.Level);
            gelfMessage.Timestamp = loggingEvent.TimeStamp;
            if (!string.IsNullOrWhiteSpace(Facility))
            {
                gelfMessage.Facility = Facility;
            }
            gelfMessage["_LoggerName"] = loggingEvent.LoggerName;
            gelfMessage["_LoggerLevel"] = loggingEvent.Level.ToString();
            gelfMessage["_ProcessName"] = Process.GetCurrentProcess().ProcessName;

            AddLocationInfo(loggingEvent, gelfMessage);
            FormatGelfMessage(gelfMessage, loggingEvent);
            return gelfMessage;
        }

        private void AddLocationInfo(LoggingEvent loggingEvent, GelfMessage gelfMessage)
        {
            if (loggingEvent.LocationInformation == null) return;
            gelfMessage.File = loggingEvent.LocationInformation.FileName;
            gelfMessage.Line = loggingEvent.LocationInformation.LineNumber;
        }

        private void FormatGelfMessage(GelfMessage gelfMessage, LoggingEvent loggingEvent)
        {
            var messageFormatter = messageObjectFormatters.First(x => x.CanApply(loggingEvent.MessageObject));
            messageFormatter.Format(gelfMessage, loggingEvent.MessageObject);
            AppendExceptionInformationIfExists(gelfMessage, loggingEvent.ExceptionObject);
        }

        private void AppendExceptionInformationIfExists(GelfMessage gelfMessage, Exception exceptionObject)
        {
            if (exceptionObject != null)
            {
                ExceptionMessageFormatter.Format(gelfMessage, exceptionObject);
            }
        }
    }
}