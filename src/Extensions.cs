using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace rabbitmq.log4net.gelf.appender
{
    public static class Extensions
    {
        public static string AsJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        
        public static byte[] AsByteArray(this string value)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);
        }

        public static string AsString(this byte[] byteArray)
        {
            return System.Text.Encoding.UTF8.GetString(byteArray);
        }

        public static string TruncateString(this string value, int len)
        {
            return value.Length < len ? value : value.Substring(0, len);
        }

        public static double ToUnixTimestamp(this DateTime d)
        {
            var duration = d.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return duration.TotalSeconds;
        }

        public static DateTime FromUnixTimestamp(this double dateTime)
        {
            var datetime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(dateTime).ToLocalTime();
            return datetime;
        }

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
    }
}