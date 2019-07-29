using RabbitMQ.Client.Events;

namespace Infrastructure.Brokers.RabbitMq.Facades.Interfaces
{
    public interface IMessageFacade
    {
        void PublishMessage(byte[] body, string routingKey);
        
        void Ack(BasicDeliverEventArgs args);
        
        byte[] WaitMessageFromQueue(string queueName);
    }
}