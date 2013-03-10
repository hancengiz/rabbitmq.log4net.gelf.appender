using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;
using log4net.Appender;
using log4net.Core;

namespace rabbitmq.log4net.gelf.appender
{
    public class GelfRabbitMQAdapter : AppenderSkeleton
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
        public string Exchange { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        private readonly GelfAdapter gelfAdapter;
        private IConnection connection;
        private IModel model;

        public GelfRabbitMQAdapter() : this(new GelfAdapter()) { }

        public GelfRabbitMQAdapter(GelfAdapter gelfAdapter)
        {
            this.gelfAdapter = gelfAdapter;
            SetDefaultConfig();
        }

        private void SetDefaultConfig()
        {
            HostName = "localhost";
            Port = 5672;
            VirtualHost = "/";
            Exchange = "log4net.gelf.appender";
            Username = "guest";
            Password = "guest";
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            OpenConnection();
        }

        public void EnsureConnectionIsOpen()
        {
            if (model != null) return;
            OpenConnection();
        }

        private void OpenConnection()
        {
            connection = CreateConnectionFactory().CreateConnection();
            connection.ConnectionShutdown += ConnectionShutdown;
            model = connection.CreateModel();
            model.ExchangeDeclare(Exchange, ExchangeType.Topic);
        }

        void ConnectionShutdown(IConnection shutingDownConnection, ShutdownEventArgs reason)
        {
            SafeShutdownForConnection();
            SafeShutDownForModel();
        }

        private void SafeShutDownForModel()
        {
            if (model == null) return;
            model.Close(Constants.ReplySuccess, "gelf rabbit appender shutting down!");
            model.Dispose();
            model = null;
        }

        private void SafeShutdownForConnection()
        {
            if (connection == null) return;
            connection.ConnectionShutdown -= ConnectionShutdown;
            connection.AutoClose = true;
            connection = null;
        }

        protected virtual ConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory()
                                    {
                                        Protocol = Protocols.FromEnvironment(),
                                        HostName = HostName,
                                        Port = Port,
                                        VirtualHost = VirtualHost,
                                        UserName = Username,
                                        Password = Password
                                    };
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            EnsureConnectionIsOpen();
            model.ExchangeDeclare(Exchange, ExchangeType.Topic);
            var messageBody = gelfAdapter.Adapt(loggingEvent).AsJson().AsByteArray();
            model.BasicPublish(Exchange, "log4net.gelf.appender", true, null, messageBody);
        }

        protected override void OnClose()
        {
            base.OnClose();
            SafeShutdownForConnection();
            SafeShutDownForModel();
        }
    }
}