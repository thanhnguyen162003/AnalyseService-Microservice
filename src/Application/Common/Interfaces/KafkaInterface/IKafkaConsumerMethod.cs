namespace Application.Common.Interfaces.KafkaInterface;

public interface IKafkaConsumerMethod
{
    public Task<string> ConsumeByKeyAsync(string topicName, string key, CancellationToken stoppingToken);
}
