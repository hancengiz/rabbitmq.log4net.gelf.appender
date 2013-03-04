using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace rabbitmq.log4net.gelf.appender.MessageFormatters
{
    public class DictionaryGelfMessageFormatter : IGelfMessageFormatter
    {
        private const string FullMessageKeyName = "full_message";
        private const string ShortMessageKeyName = "short_message";
        private static readonly IEnumerable<string> FullMessageKeyValues = new[] { "FULLMESSAGE", "FULL_MESSAGE", "MESSAGE" };
        private static readonly IEnumerable<string> ShortMessageKeyValues = new[] { "SHORTMESSAGE", "SHORT_MESSAGE", "MESSAGE" };

        public virtual bool CanApply(object messageObject)
        {
            return messageObject is IDictionary;
        }

        public virtual void Format(GelfMessage gelfMessage, object messageObject)
        {
            foreach (DictionaryEntry entry in (IDictionary)messageObject)
            {
                var key = FormatKey(entry.Key.ToString());
                gelfMessage[key] = entry.Value.ToString();
            }
        }

        private string FormatKey(string key)
        {
            if (IsForFullMessage(key)) return FullMessageKeyName;
            if (IsForShortMessage(key)) return ShortMessageKeyName;
            return key.StartsWith("_") ? key : "_" + key;
        }

        private static bool IsForShortMessage(string key)
        {
            return ShortMessageKeyValues.Contains(key, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsForFullMessage(string key)
        {
            return FullMessageKeyValues.Contains(key, StringComparer.OrdinalIgnoreCase);
        }
    }
}