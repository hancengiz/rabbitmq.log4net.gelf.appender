using System;

namespace rabbitmq.log4net.gelf.appender
{
    public static class Extensions
    {
        public static string ShortenMessage(this string message)
        {
            const int shortMessageLength = 250;
            return message.Length < shortMessageLength ? message : message.Substring(0, shortMessageLength);
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
    }
}