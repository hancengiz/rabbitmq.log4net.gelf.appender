using System;
using System.Collections.Generic;

namespace rabbitmq.log4net.gelf.appender
{
    public static class Extensions
    {
        public static string TruncateString(this string value, int len)
        {
            return value.Length < len ? value : value.Substring(0, len);
        }

        public static double ToUnixTimestamp(this DateTime d)
        {
            var duration = d.ToLocalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return duration.TotalMilliseconds;
        }

        public static DateTime FromUnixTimestamp(this double dateTime)
        {
            var datetime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(dateTime).ToLocalTime();
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