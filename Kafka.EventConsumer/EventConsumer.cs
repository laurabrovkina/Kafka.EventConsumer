using Confluent.Kafka;

namespace Kafka.EventConsumer;

public class EventConsumer : BackgroundService
{
    private readonly ILogger<EventConsumer> _logger;
    private readonly string _topic = "simpletalk_topic";
    private readonly ConsumerConfig _config;
    private readonly IDatabaseClient _databaseClient;

    public EventConsumer(ILogger<EventConsumer> logger, IDatabaseClient databaseClient)
    {
        _logger = logger;
        _databaseClient = databaseClient;
        _config = new ConsumerConfig
        {
            GroupId = "st_consumer_group",
            BootstrapServers = "localhost:9092",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var builder = new ConsumerBuilder<Ignore, string>(_config)
            .Build();
        builder.Subscribe(_topic);

        await _databaseClient.CreateTableAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumerResult = builder.Consume(stoppingToken);
                var result = consumerResult.Message?.Value;

                if (result == null)
                {
                    _logger.LogWarning("The message is empty!");
                    throw new ApplicationException();
                }

                var message = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(result);

                if (message?.Type == Type.CreatedMessage)
                {
                    if (message.Book is null)
                    {
                        continue;
                    }

                    await _databaseClient.SaveAsync(message.Book);

                    var response = await _databaseClient.GetItemAsync(message.Book);
                    foreach (var item in response.Item)
                    {
                        _logger.LogInformation("The {ItemKey} created with value {@ItemValue}", item.Key, item.Value);
                    }
                }
                else if (message?.Type == Type.UpdatedMessage)
                {
                    if (message.Book is null)
                    {
                        continue;
                    }

                    await _databaseClient.UpdateItemAsync(message.Book);

                    var response = await _databaseClient.GetItemAsync(message.Book);
                    foreach (var item in response.Item)
                    {
                        _logger.LogInformation("The {ItemKey} created with value {@ItemValue}", item.Key, item.Value);
                    }
                }
                else if (message?.Type == Type.DeletedMessage)
                {
                    if (message.Book is null)
                    {
                        continue;
                    }

                    await _databaseClient.DeleteItemAsync(message.Book);

                    var response = await _databaseClient.GetItemAsync(message.Book);
                    foreach (var item in response.Item)
                    {
                        _logger.LogInformation("The {ItemKey} created with value {@ItemValue}", item.Key, item.Value);
                    }
                }
                else
                {
                    break;
                }
                
                _logger.LogInformation("{MessageType} with {MessageId} for Book Id: {BookBookId} has been processed", message.Type, message.Id, message.Book.BookId);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ApplicationException)
            {
                break;
            }
            catch (Exception e)
            {
                builder.Close();
                _logger.LogError("Builder has stopped because of exception: {EMessage}", e.Message);
                throw;
            }
        }
    }
}
