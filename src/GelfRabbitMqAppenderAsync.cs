using System.Threading;
using RabbitMQ.Client;
using log4net.Core;

namespace rabbitmq.log4net.gelf.appender
{
    public class GelfRabbitMqAppenderAsync : GelfRabbitMqAppender
    {
        public GelfRabbitMqAppenderAsync() : this(new GelfAdapter()) { }

        public GelfRabbitMqAppenderAsync(GelfAdapter gelfAdapter) : base(gelfAdapter) { }

        protected override void Append(LoggingEvent loggingEvent)
        {
            ThreadPool.QueueUserWorkItem(AsyncAppend, loggingEvent);
        }

        protected override void SafeShutDownForModel()
        {
            lock (model)
            {
                base.SafeShutDownForModel();
            }
        }

        private void AsyncAppend(object state)
        {
            var loggingEvent = state as LoggingEvent;

            if (loggingEvent == null) return;

            lock (model)
            {
                EnsureConnectionIsOpen();
                var messageBody = gelfAdapter.Adapt(loggingEvent).AsJson();
                model.BasicPublish(Exchange, "log4net.gelf.appender", true, null, messageBody.AsByteArray());
            }
        }
    }
}
