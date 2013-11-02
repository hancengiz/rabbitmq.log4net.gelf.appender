using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using rabbitmq.log4net.gelf.appender;
using rabbitmq.log4net.gelf.appender.MessageFormatters;

namespace tests.Formatters
{
    [TestFixture]
    public class DictionaryGelfMessageFormatterTests
    {

        [Test]
        public void This_Formatter_Should_Work_For_Any_IDictionary()
        {
            var formatter = new DictionaryGelfMessageFormatter();
            
            Assert.That(formatter.CanApply(new Dictionary<string, string>()), Is.True);
            Assert.That(formatter.CanApply(new SortedDictionary<string, string>()), Is.True);
            Assert.That(formatter.CanApply(new Hashtable()), Is.True);
            Assert.That(formatter.CanApply(new object()), Is.False);
        }

        [Test]
        public void Takes_Key_Value_Pairs_And_Puts_Them_Into_Gelf_Message_With_Underscores()
        {
            var formatter = new DictionaryGelfMessageFormatter();

            var dictionary = new Dictionary<string, string>
            {
                {"animal", "cat"},
                {"sleeps", "during the day"}
            };

            var gelfMessage = GelfMessage.EmptyGelfMessage;

            formatter.Format(gelfMessage, dictionary);
            
            Assert.That(gelfMessage["_animal"], Is.EqualTo("cat"));
            Assert.That(gelfMessage["_sleeps"], Is.EqualTo("during the day"));
        }

        [TestCase("fullmessage")]
        [TestCase("FULLMESSAGE")]
        [TestCase("FULL_MESSAGE")]
        [TestCase("MESSAGE")]
        [TestCase("message")]
        [Test]
        public void Uses_Known_Key_Names_To_Populate_Full_Message(string knownKeyName)
        {
            var formatter = new DictionaryGelfMessageFormatter();
            
            var dictionary = new Dictionary<string, string>
            {
                {knownKeyName, "This is a full message"},
            };

            var gelfMessage = GelfMessage.EmptyGelfMessage;
            formatter.Format(gelfMessage, dictionary);

            Assert.That(gelfMessage.FullMessage, Is.EqualTo("This is a full message"));
        }

        [TestCase("shortmessage")]
        [TestCase("SHORTMESSAGE")]
        [TestCase("SHORT_MESSAGE")]
        [Test]
        public void Uses_Known_Key_Names_To_Populate_Short_Message(string knownKeyName)
        {
            var formatter = new DictionaryGelfMessageFormatter();
            
            var dictionary = new Dictionary<string, string>
            {
                {knownKeyName, "Short message"},
            };

            var gelfMessage = GelfMessage.EmptyGelfMessage;
            formatter.Format(gelfMessage, dictionary);

            Assert.That(gelfMessage.ShortMessage, Is.EqualTo("Short message"));
        }
    }
}