using System;
using NUnit.Framework;
using rabbitmq.log4net.gelf.appender;
using rabbitmq.log4net.gelf.appender.MessageFormatters;

namespace tests.Formatters
{
    [TestFixture]
    public class StringGelfMessageFormatterTests
    {
        [Test]
        public void Can_Be_Applied_To_String_But_Not_Object()
        {
            var formatter = new StringGelfMessageFormatter();

            Assert.That(formatter.CanApply("some string"), Is.True);
            Assert.That(formatter.CanApply(new Object()), Is.False);
        }

        [Test]
        public void When_Message_Is_Short_Enough_Only_Short_Message_Is_Populated()
        {
            var formatter = new StringGelfMessageFormatter();
            
            var message = GelfMessage.EmptyGelfMessage;

            formatter.Format(message, "Something went wrong");

            Assert.That(message.ShortMessage, Is.EqualTo("Something went wrong"));
            Assert.That(message.FullMessage, Is.Null);
        }

        [Test]
        public void Takes_First_250_Characters_From_A_Long_String_As_Short_Message()
        {
            var formatter = new StringGelfMessageFormatter();

            var gelfMessage = GelfMessage.EmptyGelfMessage;

            var messageString = "10 letters".Repeat(30);
            formatter.Format(gelfMessage, messageString);

            Assert.That(gelfMessage.ShortMessage, Is.EqualTo("10 letters".Repeat(25)));
            Assert.That(gelfMessage.FullMessage, Is.EqualTo(messageString));
        }
    }
}
