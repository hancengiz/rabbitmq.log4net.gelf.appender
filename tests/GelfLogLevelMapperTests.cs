using NUnit.Framework;
using log4net.Appender;
using log4net.Core;
using rabbitmq.log4net.gelf.appender;

namespace tests
{
    [TestFixture]
    public class GelfLogLevelMapperTests
    {
        [Test]
        public void ShouldMapLog4NetLevelToCorrectLogLevel()
        {
            var logLevelMapper = new GelfLogLevelMapper();

            Assert.That(logLevelMapper.Map(Level.Alert), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Alert));

            Assert.That(logLevelMapper.Map(Level.Fatal), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Critical));
            Assert.That(logLevelMapper.Map(Level.Critical), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Critical));

            Assert.That(logLevelMapper.Map(Level.Debug), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Debug));
            Assert.That(logLevelMapper.Map(null), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Debug));

            Assert.That(logLevelMapper.Map(Level.Emergency), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Emergency));
            Assert.That(logLevelMapper.Map(Level.Severe), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Emergency));

            Assert.That(logLevelMapper.Map(Level.Error), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Error));

            Assert.That(logLevelMapper.Map(Level.Fine), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Informational));
            Assert.That(logLevelMapper.Map(Level.Finer), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Informational));
            Assert.That(logLevelMapper.Map(Level.Finest), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Informational));
            Assert.That(logLevelMapper.Map(Level.Info), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Informational));
            Assert.That(logLevelMapper.Map(Level.Off), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Informational));

            Assert.That(logLevelMapper.Map(Level.Notice), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Notice));
            Assert.That(logLevelMapper.Map(Level.Verbose), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Notice));
            Assert.That(logLevelMapper.Map(Level.Trace), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Notice));

            Assert.That(logLevelMapper.Map(Level.Warn), Is.EqualTo((long)LocalSyslogAppender.SyslogSeverity.Warning));
        }
    }
}