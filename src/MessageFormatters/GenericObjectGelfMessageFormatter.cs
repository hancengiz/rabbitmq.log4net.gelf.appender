using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace rabbitmq.log4net.gelf.appender.MessageFormatters
{
    public class GenericObjectGelfMessageFormatter : DictionaryGelfMessageFormatter
    {
        public override bool CanApply(object messageObject)
        {
            return messageObject != null;
        }

        public override void Format(GelfMessage gelfMessage, object messageObject)
        {
            base.Format(gelfMessage, CreateDictionaryFromObject(messageObject));
        }

        private IDictionary CreateDictionaryFromObject(object messageObject)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(messageObject))
            {
                var propertyValue = propertyDescriptor.GetValue(messageObject);
                dict.Add(propertyDescriptor.Name, propertyValue);
            }
            return dict;
        }
    }
}