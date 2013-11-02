using System;
using NUnit.Framework;
using rabbitmq.log4net.gelf.appender;
using rabbitmq.log4net.gelf.appender.MessageFormatters;

namespace tests.Formatters
{
    public class ExceptionMessageFormatterTests
    {
        private ExceptionMessageFormatter formatter;

        [SetUp]
        public void Setup()
        {
            formatter = new ExceptionMessageFormatter();
        }

        [Test]
        public void Can_Format_An_Exception()
        {
            Assert.That(formatter.CanApply(new Exception()), Is.True);
            Assert.That(formatter.CanApply(new object()), Is.False);
        }

        [Test]
        public void Uses_Exception_Message_For_GelfMessage_ShortMessage()
        {
            var gelfMessage = GelfMessage.EmptyGelfMessage;

            formatter.Format(gelfMessage, new Exception("Something bad happend"));

            Assert.That(gelfMessage.ShortMessage, Is.EqualTo("Something bad happend"));
        }

        [Test]
        public void Uses_Exception_ToString_For_GelfMessage_FullMessage()
        {
            var gelfMessage = GelfMessage.EmptyGelfMessage;

            formatter.Format(gelfMessage, new Exception("Something bad happend"));

            Assert.That(gelfMessage.FullMessage, Is.EqualTo("System.Exception: Something bad happend"));
        }

        [Test]
        public void Adds_Exception_Type_Into_Additional_Attribute()
        {
            var gelfMessage = GelfMessage.EmptyGelfMessage;

            formatter.Format(gelfMessage, new InvalidOperationException("Something bad happend"));

            Assert.That(gelfMessage["_ExceptionType"], Is.EqualTo("System.InvalidOperationException"));
        }

        [Test]
        public void Adds_Stacktrace_Into_Additional_Attribute()
        {
            var gelfMessage = GelfMessage.EmptyGelfMessage;

            try
            {
                throw new Exception("Bananas");
            }
            catch (Exception ex)
            {
                formatter.Format(gelfMessage, ex);
            }

            Assert.That(gelfMessage["_ExceptionStackTrace"], Is.Not.Empty);
        }

        [Test]
        public void Adds_InnerExceptionInformation()
        {
            var gelfMessage = GelfMessage.EmptyGelfMessage;

            try
            {
                throw new InvalidOperationException("House of Commons", new Exception("MP"));
            }
            catch (Exception ex)
            {
                formatter.Format(gelfMessage, ex);
            }

            Assert.That(gelfMessage["_ExceptionType"], Contains.Substring("InvalidOperationException"));
            Assert.That(gelfMessage["_InnerExceptionType"], Is.EqualTo("System.Exception"));
            Assert.That(gelfMessage["_InnerExceptionMessage"], Is.EqualTo("MP"));
        }

        [Test]
        public void Does_Not_Override_Short_or_Full_Message_If_Already_Exist()
        {
            var gelfMessage = GelfMessage.EmptyGelfMessage;
            gelfMessage.ShortMessage = "I'm telling you";
            gelfMessage.FullMessage = "I'm telling you a long story";

            formatter.Format(gelfMessage, new Exception("Something bad happend"));

            Assert.That(gelfMessage.ShortMessage, Is.EqualTo("I'm telling you")); 
            Assert.That(gelfMessage.FullMessage, Is.EqualTo("I'm telling you a long story")); 
        }

        [Test]
        public void When_Short_or_Full_Message_Already_Exist_Exception_Details_Added_As_Extras()
        {
            var gelfMessage = GelfMessage.EmptyGelfMessage;
            gelfMessage.ShortMessage = "I'm telling you";
            gelfMessage.FullMessage = "I'm telling you a long story";

            formatter.Format(gelfMessage, new Exception("Something bad happend"));

            Assert.That(gelfMessage["_ExceptionMessage"], Is.EqualTo("Something bad happend"));
            Assert.That(gelfMessage["_Exception"], Is.EqualTo("System.Exception: Something bad happend"));
        }
    }
}