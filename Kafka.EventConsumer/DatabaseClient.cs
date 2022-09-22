using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Kafka.EventConsumer;

public class DatabaseClient : IDatabaseClient
{
    private const string StatusUnknown = "UNKNOWN";
    private const string StatusActive = "ACTIVE";
    private readonly AmazonDynamoDBClient _amazonDynamoDbClient;
    
    public DatabaseClient(AmazonDynamoDBClient amazonDynamoDbClient)
    {
        _amazonDynamoDbClient = amazonDynamoDbClient;
    }
    
    public async Task CreateTableAsync()
    {
        var createTableRequest = new CreateTableRequest
        {
            ProvisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = 10,
                WriteCapacityUnits = 5
            },
            StreamSpecification = new StreamSpecification
            {
                StreamEnabled = true,
                StreamViewType = StreamViewType.NEW_AND_OLD_IMAGES
            },
            AttributeDefinitions = new List<AttributeDefinition>()
            {
                new AttributeDefinition
                {
                    AttributeName = "BookId",
                    AttributeType = "S"
                },
                new AttributeDefinition
                {
                    AttributeName = "AuthorId",
                    AttributeType = "S"
                }
            },
            KeySchema = new List<KeySchemaElement>()
            {
                new KeySchemaElement
                {
                    AttributeName = "BookId",
                    KeyType = "HASH"
                },
                new KeySchemaElement
                {
                    AttributeName = "AuthorId",
                    KeyType = "RANGE"
                }
            },
            TableName = "book_shelf"
        };
        
        var status = await GetTableStatusAsync(createTableRequest.TableName);
        if (status != StatusUnknown)
        {
            return;
        }

        await _amazonDynamoDbClient.CreateTableAsync(createTableRequest);

        await WaitUntilTableReady(createTableRequest.TableName);
    }
    
    public async Task SaveAsync(Book book)
    {
        var putItemRequest = new PutItemRequest
        {
            TableName = "book_shelf",
            Item = new Dictionary<string, AttributeValue>
            {
                { "BookId", new AttributeValue { S = book.BookId.ToString() } },
                { "AuthorId", new AttributeValue { S = book.AuthorId.ToString() } },
                { "Title", new AttributeValue { S = book.Title } },
                { "Publisher", new AttributeValue { S = book.Publisher } },
                { "NumberOfPages", new AttributeValue { S = book.NumberOfPages.ToString() } },
                { "Published", new AttributeValue { S = book.Published.ToString() } },
                { "Timestamp", new AttributeValue { S = DateTime.UtcNow.ToString() } }
            }
        };
        
        await _amazonDynamoDbClient.PutItemAsync(putItemRequest);
    }

    public async Task UpdateItemAsync(Book book)
    {
        var request = new UpdateItemRequest
        {
            Key = new Dictionary<string, AttributeValue>()
            {
                { "BookId", new AttributeValue { S = book.BookId.ToString() } },
                { "AuthorId", new AttributeValue { S = book.AuthorId.ToString() } }
            },
            AttributeUpdates = new Dictionary<string, AttributeValueUpdate>
            {
                { "Title", new AttributeValueUpdate
                {
                    Value = new AttributeValue {S = book.Title },
                    Action = AttributeAction.PUT
                }},
                { "Publisher", new AttributeValueUpdate
                {
                    Value = new AttributeValue {S = book.Publisher },
                    Action = AttributeAction.PUT
                }},
                { "NumberOfPages", new AttributeValueUpdate
                {
                    Value = new AttributeValue {S = book.NumberOfPages.ToString() },
                    Action = AttributeAction.PUT
                }},
                { "Published", new AttributeValueUpdate
                {
                    Value = new AttributeValue {S = book.Published.ToString() },
                    Action = AttributeAction.PUT
                }},
                { "Timestamp", new AttributeValueUpdate
                {
                    Value = new AttributeValue {S = DateTime.UtcNow.ToString() },
                    Action = AttributeAction.PUT
                }}
            },
            TableName = "book_shelf"
        };
        
        await _amazonDynamoDbClient.UpdateItemAsync(request);
    }

    public async Task DeleteItemAsync(Book book)
    {
        var request = new DeleteItemRequest
        {
            Key = new Dictionary<string, AttributeValue>()
            {
                { "BookId", new AttributeValue { S = book.BookId.ToString() } },
                { "AuthorId", new AttributeValue { S = book.AuthorId.ToString() } }
            },
            TableName = "book_shelf"
        };
        
        await _amazonDynamoDbClient.DeleteItemAsync(request);
    }

    public async Task<GetItemResponse> GetItemAsync(Book book)
    {
        var getRequest = new GetItemRequest()
        {
            Key = new Dictionary<string, AttributeValue>()
            {
                { "BookId", new AttributeValue { S = book.BookId.ToString() } },
                { "AuthorId", new AttributeValue { S = book.AuthorId.ToString() } }
            },
            TableName = "book_shelf"
        };
        var response = await _amazonDynamoDbClient.GetItemAsync(getRequest);

        return response;
    }

    private async Task<string> GetTableStatusAsync(string tableName)
    {
        try
        {
            var response = await _amazonDynamoDbClient.DescribeTableAsync(new DescribeTableRequest
            {
                TableName = tableName
            });
            return response?.Table.TableStatus;
        }
        catch (ResourceNotFoundException)
        {
            return StatusUnknown;
        }
    }

    private async Task WaitUntilTableReady(string tableName)
    {
        var status = await GetTableStatusAsync(tableName);
        for (var i = 0; i < 10 && status != StatusActive; ++i)
        {
            await Task.Delay(500);
            status = await GetTableStatusAsync(tableName);
        }
    }
}
