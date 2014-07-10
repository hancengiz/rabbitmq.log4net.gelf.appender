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
        private readonly GelfLogLevelMapper _gelfLogLevelMapper;
        private readonly IList<IGelfMessageFormatter> _messageObjectFormatters;

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
            _gelfLogLevelMapper = gelfLogLevelMapper;
            _messageObjectFormatters = messageObjectFormatters;
        }

        public string Facility { private get; set; }

        public GelfMessage Adapt(LoggingEvent loggingEvent)
        {
            var gelfMessage = GelfMessage.EmptyGelfMessage;
            gelfMessage.Level = _gelfLogLevelMapper.Map(loggingEvent.Level);
            gelfMessage.Timestamp = loggingEvent.TimeStamp;
            if (!string.IsNullOrWhiteSpace(Facility))
            {
                gelfMessage.Facility = Facility;
            }
            gelfMessage["_LoggerName"] = loggingEvent.LoggerName;
            gelfMessage["_LoggerLevel"] = loggingEvent.Level.ToString();
            gelfMessage["_ProcessName"] = Process.GetCurrentProcess().ProcessName;
            gelfMessage["_ThreadName"] = loggingEvent.ThreadName;
            gelfMessage["_Domain"] = loggingEvent.Domain;

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
            var messageFormatter = _messageObjectFormatters.First(x => x.CanApply(loggingEvent.MessageObject));
            messageFormatter.Format(gelfMessage, loggingEvent.MessageObject);
            AppendExceptionInformationIfExists(gelfMessage, loggingEvent.ExceptionObject);

            if (string.IsNullOrWhiteSpace(gelfMessage.ShortMessage))
            {
                gelfMessage.ShortMessage = "Logged object of type: " + loggingEvent.MessageObject.GetType().FullName;
            }
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