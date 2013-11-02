using System;

namespace rabbitmq.log4net.gelf.appender.MessageFormatters
{
    public class ExceptionMessageFormatter : IGelfMessageFormatter
    {
        public bool CanApply(object messageObject)
        {
            return messageObject is Exception;
        }

        public void Format(GelfMessage gelfMessage, object messageObject)
        {
            var exception = (Exception) messageObject;

            if (string.IsNullOrEmpty(gelfMessage.ShortMessage))
            {
                gelfMessage.ShortMessage = exception.Message;
            }
            else
            {
                gelfMessage["_ExceptionMessage"] = exception.Message;
            }

            if (string.IsNullOrEmpty(gelfMessage.FullMessage))
            {
                gelfMessage.FullMessage = exception.ToString();
            }
            else
            {
                gelfMessage["_Exception"] = exception.ToString();
            }
            
            gelfMessage["_ExceptionType"] = messageObject.GetType().FullName;
            gelfMessage["_ExceptionStackTrace"] = exception.StackTrace;

            if (exception.InnerException == null) return;

            gelfMessage["_InnerExceptionType"] = exception.InnerException.GetType().FullName;
            gelfMessage["_InnerExceptionMessage"] = exception.InnerException.Message;
        }
    }
}
