namespace rabbitmq.log4net.gelf.appender
{
    public static class StringExtensions
    {
        public static string ShortenMessage(this string message)
        {
            const int shortMessageLength = 250;
            return message.Length < shortMessageLength ? message : message.Substring(0, shortMessageLength);
        }
    }
}