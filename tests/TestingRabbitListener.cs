using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace tests
{
    public class TestingRabbitListener : IDisposable
    {
        private IConnection _connection;
        private IModel _model;
        private const string QueueName = "test.consumer";
        private const string ExchangeName = "tests.log4net.gelf.appender";
        public List<string> ReceivedMessages { get; private set; }

        public TestingRabbitListener()
        {
            ReceivedMessages = new List<string>();
            SubscribeToExchange();
        }

        private void SubscribeToExchange()
        {
            var factory = new ConnectionFactory
                              {
                                  HostName = "localhost",
                                  UserName = "guest",
                                  Password = "guest",
                                  Protocol = Protocols.DefaultProtocol
                              };
            _connection = factory.CreateConnection();
            _model = _connection.CreateModel();
            var consumerQueue = _model.QueueDeclare(QueueName, false, true, true, null);
            _model.QueueBind(consumerQueue, ExchangeName, "#");

            var consumer = new EventingBasicConsumer(_model);
            consumer.Received += (o, e) =>
                                     {
                                         var data = Encoding.ASCII.GetString(e.Body);
                                         ReceivedMessages.Add(data);
                                         Console.WriteLine(data);
                                     };
            _model.BasicConsume(consumerQueue, true, consumer);
        }

        public void Dispose()
        {
            _model.QueueUnbind(QueueName, ExchangeName, "#", null);
            _model.Dispose();
            _connection.Dispose();
        }
    }
}