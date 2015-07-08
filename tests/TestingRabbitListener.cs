using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using tests.TestingSupport.tests.TestingSupport;

namespace tests
{
	public class TestingRabbitListener : IDisposable
	{
		private IConnection connection;
		private IModel model;
		private const string queueName = "test.consumer";
		private const string exchangeName = "tests.log4net.gelf.appender";
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
			connection = factory.CreateConnection();
			model = connection.CreateModel();
			var consumerQueue = model.QueueDeclare(queueName, false, true, true, null);
			model.QueueBind(consumerQueue, exchangeName, "#");

			var consumer = new EventingBasicConsumer(model);
			consumer.Received += (o, e) =>
									 {
										 var data = Encoding.ASCII.GetString(e.Body);
										 ReceivedMessages.Add(data);
										 Console.WriteLine(data);
									 };
			model.BasicConsume(consumerQueue, true, consumer);
		}

		public void Dispose()
		{
			model.QueueUnbind(queueName, exchangeName, "#", null);
			model.Dispose();
			connection.Dispose();
		}

		public void WaitForMessage(int expectedMessageCount)
		{
			Wait.For(1).Until(() => ReceivedMessages.Count == 1);
		}

		public void WaitForAMessage()
		{
			WaitForMessage(1);
		}
	}
}