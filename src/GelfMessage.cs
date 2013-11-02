using System;
using System.Collections.Generic;
using System.Globalization;

namespace rabbitmq.log4net.gelf.appender
{
    public class GelfMessage : Dictionary<string, object>
    {
        public string Facility
        {
            get { return ValueAs<string>("facility"); }
            set { SetValueAs(value, "facility"); }
        }

        public string File
        {
            get { return ValueAs<string>("file"); }
            set { SetValueAs(value, "file"); }
        }

        public string FullMessage
        {
            get { return ValueAs<string>("full_message"); }
            set { SetValueAs(value, "full_message"); }
        }

        public string Host
        {
            get { return ValueAs<string>("host"); }
            set { SetValueAs(value, "host"); }
        }

        public long Level
        {
            get { return ValueAs<long>("level"); }
            set { SetValueAs(value, "level"); }
        }

        public string Line
        {
            get { return ValueAs<string>("line"); }
            set { SetValueAs(value, "line"); }

        }

        public string ShortMessage
        {
            get { return ValueAs<string>("short_message"); }
            set { SetValueAs(value, "short_message"); }
        }

        public string Version
        {
            get { return ValueAs<string>("version"); }
            set { SetValueAs(value, "version"); }
        }

        public DateTime Timestamp
        {
            get
            {
                if (!ContainsKey("timestamp"))
                    return DateTime.MinValue;

                object val = this["timestamp"];
                double value;
                bool parsed = Double.TryParse(val as string, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                return parsed ? value.FromUnixTimestamp() : DateTime.MinValue;
            }
            set
            {
                if (!ContainsKey("timestamp"))
                    Add("timestamp", value.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture));
                else
                    this["timestamp"] = value.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture);
            }
        }

        private T ValueAs<T>(string key)
        {
            if (!ContainsKey(key))
                return default(T);

            return (T) this[key];
        }

        private void SetValueAs(object value, string key)
        {
            if (!ContainsKey(key))
                Add(key, value);
            else
                this[key] = value;
        }

        public static GelfMessage EmptyGelfMessage
        {
            get
            {
                return new GelfMessage
                {
                    Version = "1.0",
                    Host = Environment.MachineName,
                    File = "",
                    Line = ""
                };
            }
        }
    }
}