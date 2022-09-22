using System.Reflection;
using Amazon;
using Amazon.DynamoDBv2;

namespace Kafka.EventConsumer;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(options => options.AddJsonFile("appsettings.json"))
            .ConfigureAppConfiguration(config => config.AddUserSecrets(Assembly.GetExecutingAssembly()))
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<EventConsumer>();
                services.AddSingleton<IAmazonDynamoDB>(x => CreateAmazonDynamoDb("http://localhost:8000"));
                services.AddScoped<IDatabaseClient, DatabaseClient>();
            });

    private static IAmazonDynamoDB CreateAmazonDynamoDb(string serviceUrl)
    {
        var clientConfig = new AmazonDynamoDBConfig {RegionEndpoint = RegionEndpoint.EUWest1};

        if (!string.IsNullOrEmpty(serviceUrl))
        {
            clientConfig.ServiceURL = serviceUrl;
        }

        var dynamoClient = new AmazonDynamoDBClient("keyId","key", clientConfig);
        return dynamoClient;
    }
}