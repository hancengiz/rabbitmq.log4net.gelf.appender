using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using log4net.Core;
using rabbitmq.log4net.gelf.appender;

namespace tests
{
    [TestFixture]
    public class GelfAdapterTests
    {

        [Test]
        public void GelfMessageContainsFieldsRequiredByTheStandard()
        {
            const string message = "some log message";
            var loggingEvents = CreateLogginEvent(message, Level.Info);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(2));
            var gelfMessage = adapter.Adapt(loggingEvents);

            Assert.That(gelfMessage.Version, Is.EqualTo("1.0"));
            Assert.That(gelfMessage.Host, Is.EqualTo(Environment.MachineName));
            Assert.That(gelfMessage.ShortMessage, Is.Not.Empty);
            Assert.That(gelfMessage.Timestamp, Is.GreaterThan(DateTime.MinValue));
        }

        [Test]
        public void GelfMessageIsPopulatedWithExpectedValuesBasedOnTheLoggingEvent()
        {
            var message = "logging event data message which is longer than two hundred and fifty five characters".Repeat(3);
            var loggingEvent = CreateLogginEvent(message, Level.Debug);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage.FullMessage, Is.EqualTo(message));
            Assert.That(gelfMessage.ShortMessage, Is.EqualTo(message.Substring(0, 250)));
            Assert.That(gelfMessage.Level, Is.EqualTo((long)1));
            Assert.That(gelfMessage.Timestamp.ToString(), Is.EqualTo(loggingEvent.TimeStamp.ToString()));
            Assert.That(gelfMessage.File, Is.StringEnding(@"rabbitmq.log4net.gelf.appender\tests\GelfAdapterTests.cs"));
            Assert.That(string.IsNullOrEmpty(gelfMessage.Line), Is.False);
            Assert.That(gelfMessage["_LoggerName"], Is.EqualTo(typeof(GelfAdapter).FullName));
            Assert.That(gelfMessage["_LoggerLevel"], Is.EqualTo("DEBUG"));
            Assert.That(gelfMessage["_ProcessName"], Is.EqualTo(Process.GetCurrentProcess().ProcessName));
        }

        [Test]
        public void SetsFacilityInTheGelfMessageWhenItHasBeenConfigured()
        {
            const string message = "irrelevant message";
            var loggingEvent = CreateLogginEvent(message, Level.Debug);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1))
            {
                Facility = "test-system"
            };
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage.Facility, Is.EqualTo("test-system"));
        }

        [Test]
        public void ShouldBeAbleToSuccessfullyLogMessagesShorterThan255Characters()
        {
            const string message = "a short message";
            var loggingEvent = CreateLogginEvent(message, Level.Info);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage.FullMessage, Is.EqualTo(message));
            Assert.That(gelfMessage.ShortMessage, Is.EqualTo(message));
        }

        [Test]
        public void ShouldCreateAGelfMessageWithADictionaryAsMessageObject()
        {
            var messageObject = new Dictionary<string, long> { { "key1", 1 }, { "key2", 2 } };
            var loggingEvent = CreateLogginEvent(messageObject, Level.Debug);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage["_key1"], Is.EqualTo("1"));
            Assert.That(gelfMessage["_key2"], Is.EqualTo("2"));
        }


        [Test]
        public void ShouldCreateAGelfMessageWithADictionaryAsMessageObjectWithNullProperties()
        {
            var messageObject = new Dictionary<string, object> { { "key1", null } };
            var loggingEvent = CreateLogginEvent(messageObject, Level.Debug);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage.ContainsKey("_key1"), Is.False);
        }

        [Test]
        public void ShouldCreateAGelfMessageWithAnExceptionInLoggingEvent()
        {
            const string message = "some message for exception logging";
            var exception = CreateExceptionObjectWithStackTrace();
            var loggingEvent = CreateLogginEvent(message, Level.Debug, exception);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage.FullMessage, Is.EqualTo(string.Format("{0}\n{1}", message, exception)));
        }

        [Test]
        public void ShouldCreateAGelfMessageWithAnObjectInLoggingEvent()
        {
            var messageObject = new { A = "B", C = "D" };
            var loggingEvent = CreateLogginEvent(messageObject, Level.Debug);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage["_A"], Is.EqualTo("B"));
            Assert.That(gelfMessage["_C"], Is.EqualTo("D"));
        }

        [Test]
        public void ShouldSetShortMessageFromMessagePropertyOnAnMessageObject()
        {
            var messageObject = new { Message = "message", Foo = "Bar" };
            var loggingEvent = CreateLogginEvent(messageObject, Level.Debug);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage.ShortMessage, Is.EqualTo("message"));
        }

        [Test]
        public void ShouldSetFullMessageFromFullMessagePropertyOnAnMessageObject()
        {
            var messageObject = new { FullMessage = "fullmessage", Foo = "Bar" };
            var loggingEvent = CreateLogginEvent(messageObject, Level.Debug);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage.FullMessage, Is.EqualTo("fullmessage"));
            Assert.That(gelfMessage.ShortMessage, Is.Null);
        }

        [Test]
        public void ShouldSetFullMessageFromFull_MessagePropertyOnAnMessageObject()
        {
            var messageObject = new { Full_Message = "full_message", Foo = "Bar" };
            var loggingEvent = CreateLogginEvent(messageObject, Level.Debug);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage.FullMessage, Is.EqualTo("full_message"));
            Assert.That(gelfMessage.ShortMessage, Is.Null);
        }
        [Test]
        public void ShouldSetShortMessageFromShortMessagePropertyOnAnMessageObject()
        {
            var messageObject = new { ShortMessage = "shortmessage", Foo = "Bar" };
            var loggingEvent = CreateLogginEvent(messageObject, Level.Debug);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage.ShortMessage, Is.EqualTo("shortmessage"));
            Assert.That(gelfMessage.FullMessage, Is.Null);
        }

        [Test]
        public void ShouldSetShortMessageFromShort_MessagePropertyOnAnMessageObject()
        {
            var messageObject = new { short_message = "short_message", Foo = "Bar" };
            var loggingEvent = CreateLogginEvent(messageObject, Level.Debug);

            var adapter = new GelfAdapter(StubGelfLogLevelMapper.WithValueToReturn(1));
            var gelfMessage = adapter.Adapt(loggingEvent);

            Assert.That(gelfMessage.ShortMessage, Is.EqualTo("short_message"));
            Assert.That(gelfMessage.FullMessage, Is.Null);
        }

        private static Exception CreateExceptionObjectWithStackTrace()
        {
            Exception exception;
            try
            {
                throw new Exception("some exception message");
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            return exception;
        }

        private LoggingEvent CreateLogginEvent(object message, Level level, Exception exception = null)
        {
            return new LoggingEvent(typeof(GelfAdapter), null, typeof(GelfAdapter).FullName, level, message, exception);
        }

        private class StubGelfLogLevelMapper : GelfLogLevelMapper
        {
            private readonly long valueToReturn;

            private StubGelfLogLevelMapper(long valueToReturn)
            {
                this.valueToReturn = valueToReturn;
            }

            public override long Map(Level log4NetLevel)
            {
                return valueToReturn;
            }

            public static GelfLogLevelMapper WithValueToReturn(long valueToReturn)
            {
                return new StubGelfLogLevelMapper(valueToReturn);
            }
        }
    }
}
