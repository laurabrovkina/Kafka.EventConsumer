using Amazon.DynamoDBv2.Model;

namespace Kafka.EventConsumer;

public interface IDatabaseClient
{
    Task CreateTableAsync();
    Task SaveAsync(Book book);
    Task UpdateItemAsync(Book book);
    Task DeleteItemAsync(Book book);
    Task<GetItemResponse> GetItemAsync(Book book);
}
