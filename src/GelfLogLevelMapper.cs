using System.Collections.Generic;
using log4net.Appender;
using log4net.Core;

namespace rabbitmq.log4net.gelf.appender
{
    public class GelfLogLevelMapper
    {
        private readonly Dictionary<string, long> _levelMappings;

        public GelfLogLevelMapper()
        {
            _levelMappings = new Dictionary<string, long>
                                {
                                    {Level.Alert.Name, (long)LocalSyslogAppender.SyslogSeverity.Alert},
                                    {Level.Critical.Name, (long)LocalSyslogAppender.SyslogSeverity.Critical},
                                    {Level.Fatal.Name, (long)LocalSyslogAppender.SyslogSeverity.Critical},
                                    {Level.Debug.Name, (long)LocalSyslogAppender.SyslogSeverity.Debug},
                                    {Level.Emergency.Name, (long)LocalSyslogAppender.SyslogSeverity.Emergency},
                                    {Level.Error.Name, (long)LocalSyslogAppender.SyslogSeverity.Error},
                                    {Level.Fine.Name, (long)LocalSyslogAppender.SyslogSeverity.Informational},
                                    {Level.Finer.Name, (long)LocalSyslogAppender.SyslogSeverity.Informational},
                                    {Level.Finest.Name, (long)LocalSyslogAppender.SyslogSeverity.Informational},
                                    {Level.Info.Name, (long)LocalSyslogAppender.SyslogSeverity.Informational},
                                    {Level.Off.Name, (long)LocalSyslogAppender.SyslogSeverity.Informational},
                                    {Level.Notice.Name, (long)LocalSyslogAppender.SyslogSeverity.Notice},
                                    {Level.Verbose.Name, (long)LocalSyslogAppender.SyslogSeverity.Notice},
                                    {Level.Trace.Name, (long)LocalSyslogAppender.SyslogSeverity.Notice},
                                    {Level.Severe.Name, (long)LocalSyslogAppender.SyslogSeverity.Emergency},
                                    {Level.Warn.Name, (long)LocalSyslogAppender.SyslogSeverity.Warning}
                                };
        }

        public virtual long Map(Level log4NetLevel)
        {
            if (log4NetLevel == null) return (long)LocalSyslogAppender.SyslogSeverity.Debug;
            long mappedValue;
            _levelMappings.TryGetValue(log4NetLevel.Name, out mappedValue);
            return mappedValue;
        }
    }
}