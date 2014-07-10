using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;
using log4net.Appender;
using log4net.Core;

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

        private readonly GelfAdapter _gelfAdapter;
        private IConnection _connection;
        private IModel _model;
        private IKnowAboutConfiguredFacility _facilityInformation = new UnknownFacility();

        public GelfRabbitMqAppender() : this(new GelfAdapter()) { }

        public GelfRabbitMqAppender(GelfAdapter gelfAdapter)
        {
            _gelfAdapter = gelfAdapter;
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
                _facilityInformation = new KnownFacility(Facility);
                _gelfAdapter.Facility = Facility;
            }

            OpenConnection();
        }

        public void EnsureConnectionIsOpen()
        {
            if (_model != null) return;
            OpenConnection();
        }

        private void OpenConnection()
        {
            _connection = CreateConnectionFactory().CreateConnection();
            _connection.ConnectionShutdown += ConnectionShutdown;
            _model = _connection.CreateModel();
            _model.ExchangeDeclare(Exchange, ExchangeType.Topic);
        }

        void ConnectionShutdown(IConnection shutingDownConnection, ShutdownEventArgs reason)
        {
            SafeShutdownForConnection();
            SafeShutDownForModel();
        }

        private void SafeShutDownForModel()
        {
            if (_model == null) return;
            _model.Close(Constants.ReplySuccess, "gelf rabbit appender shutting down!");
            _model.Dispose();
            _model = null;
        }

        private void SafeShutdownForConnection()
        {
            if (_connection == null) return;
            _connection.ConnectionShutdown -= ConnectionShutdown;
            _connection.AutoClose = true;
            _connection = null;
        }

        protected virtual ConnectionFactory CreateConnectionFactory()
        {
            return new ConnectionFactory
                                    {
                                        Protocol = Protocols.FromEnvironment(),
                                        HostName = HostName,
                                        Port = Port,
                                        VirtualHost = VirtualHost,
                                        UserName = Username,
                                        Password = Password,
                                        ClientProperties = AmqpClientProperties.WithFacility(_facilityInformation) 
                                    };
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            EnsureConnectionIsOpen();
            _model.ExchangeDeclare(Exchange, ExchangeType.Topic);
            var messageBody = _gelfAdapter.Adapt(loggingEvent).AsJson();
            _model.BasicPublish(Exchange, "log4net.gelf.appender", true, null, messageBody.AsByteArray());
        }

        protected override void OnClose()
        {
            base.OnClose();
            SafeShutdownForConnection();
            SafeShutDownForModel();
        }
    }
}