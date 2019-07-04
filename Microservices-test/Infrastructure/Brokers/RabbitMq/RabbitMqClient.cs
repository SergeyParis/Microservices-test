using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Infrastructure.Brokers.RabbitMq
{
    public class RabbitMqClient : IDisposable
    {
        private const string DefaultChannelName = "Default";

        private readonly IConnection _connection;
        private readonly List<IModel> _channels;

        public bool AutoAck { get; set; } = true;

        public RabbitMqClient(string host = "localhost")
        {
            _channels = new List<IModel>();

            var factory = new ConnectionFactory {HostName = host};
            _connection = factory.CreateConnection();

            CreateDefaultChannel();
        }

        public void PublishMessage(byte[] body, string queue = DefaultChannelName)
        {
            var channel = GetChannel(queue);
            channel.BasicPublish("", queue, null, body);
        }

        public void SubscribeOnQueue(Action<object, BasicDeliverEventArgs> handler, string queue = DefaultChannelName)
        {
            var channel = GetChannel(queue);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, args) => handler(model, args);

            channel.BasicConsume(queue, AutoAck, consumer);
        }

        public void Ack(BasicDeliverEventArgs args, string queue = DefaultChannelName)
        {
            var channel = GetChannel(queue);
            channel.BasicAck(args.DeliveryTag, false);
        }

        public byte[] WaitMessageFromQueue(string queue = DefaultChannelName)
        {
            var channel = GetChannel(queue);
            
            BasicGetResult result = null;
            while (result == null)
            {
                result = channel.BasicGet(queue, AutoAck);
                Thread.Sleep(50);
            }

            return result.Body;
        }

        public void Dispose()
        {
            _connection?.Dispose();
            foreach (var channel in _channels)
                channel.Dispose();
        }

        private void CreateDefaultChannel() => CreateChannelWithQueue(DefaultChannelName);

        private void CreateChannelWithQueue(string queueName)
        {
            var channel = _connection.CreateModel();
            _channels.Add(channel);

            CreateQueue(queueName, channel);
        }

        private IModel GetChannel(string queueName)
        {
            if (queueName != DefaultChannelName)
                throw new NotImplementedException();

            return _channels.First();
        }

        private void CreateQueue(string name, IModel channel, bool isDurable = false)
        {
            channel.QueueDeclare(name, isDurable, false, false, null);
        }
    }
}