using RabbitMQ.Client;
using log4net.Appender;
using log4net.Core;
using RabbitMQ.Client.Framing;

namespace rabbitmq.log4net.gelf.appender
{
    public class GelfRabbitMqAppender : AppenderSkeleton
    {
        public string HostName { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
        public string Exchange { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Facility { get; set; }
        public bool Durable { get; set; }
        public string ExchangeType { get; set; }
        public string RoutingKey { get; set; }

        protected readonly GelfAdapter gelfAdapter;
        private IConnection connection;
        protected IModel model;
        private IKnowAboutConfiguredFacility facilityInformation = new UnknownFacility();

        public GelfRabbitMqAppender() : this(new GelfAdapter()) { SetDefaultConfig(); }

        public GelfRabbitMqAppender(GelfAdapter gelfAdapter)
        {
            this.gelfAdapter = gelfAdapter;
            SetDefaultConfig();
        }

        private void SetDefaultConfig()
        {
            HostName = string.IsNullOrEmpty(HostName) ? "localhost" : HostName;
            Port = Port == 0 ? 5672 : Port;
            VirtualHost =string.IsNullOrEmpty(VirtualHost) ? "/" : VirtualHost;
            Exchange = string.IsNullOrEmpty(Exchange) ? "log4net.gelf.appender" : Exchange;
            Username = string.IsNullOrEmpty(Username) ? "guest" : Username;
            Password = string.IsNullOrEmpty(Password) ? "guest" : Password;
            ExchangeType = string.IsNullOrEmpty(ExchangeType) ? "topic" : ExchangeType;
            RoutingKey =string.IsNullOrEmpty(RoutingKey) ? "log4net.gelf.appender" : RoutingKey;
        }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            if (!string.IsNullOrWhiteSpace(Facility))
            {
                facilityInformation = new KnownFacility(Facility);
                gelfAdapter.Facility = Facility;
            }

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
            model.ExchangeDeclare(Exchange, ExchangeType, Durable);
        }

        private void ConnectionShutdown(IConnection shutingDownConnection, ShutdownEventArgs reason)
        {
            SafeShutdownForConnection();
            SafeShutDownForModel();
        }

        protected virtual void SafeShutDownForModel()
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
            return new ConnectionFactory
                                    {
                                        Protocol = Protocols.DefaultProtocol,
                                        HostName = HostName,
                                        Port = Port,
                                        VirtualHost = VirtualHost,
                                        UserName = Username,
                                        Password = Password,
                                        ClientProperties = AmqpClientProperties.WithFacility(facilityInformation)
                                    };
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            EnsureConnectionIsOpen();
            model.ExchangeDeclare(Exchange, ExchangeType, Durable);
            var messageBody = gelfAdapter.Adapt(loggingEvent).AsJson();
            model.BasicPublish(Exchange, RoutingKey, true, null, messageBody.AsByteArray());
        }

        protected override void OnClose()
        {
            base.OnClose();
            SafeShutdownForConnection();
            SafeShutDownForModel();
        }
    }
}