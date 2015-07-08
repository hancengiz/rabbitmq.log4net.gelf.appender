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

        protected readonly GelfAdapter gelfAdapter;
        private IConnection connection;
        protected IModel model;
        private IKnowAboutConfiguredFacility facilityInformation = new UnknownFacility();

        public GelfRabbitMqAppender() : this(new GelfAdapter()) { }

        public GelfRabbitMqAppender(GelfAdapter gelfAdapter)
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
            model.ExchangeDeclare(Exchange, ExchangeType.Topic, Durable);
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
            model.ExchangeDeclare(Exchange, ExchangeType.Topic, Durable);
            var messageBody = gelfAdapter.Adapt(loggingEvent).AsJson();
            model.BasicPublish(Exchange, "log4net.gelf.appender", true, null, messageBody.AsByteArray());
        }

        protected override void OnClose()
        {
            base.OnClose();
            SafeShutdownForConnection();
            SafeShutDownForModel();
        }
    }
}