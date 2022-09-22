# Kafka.EventConsumer
### AWS example for reading data from Kafka stream and put it into Dynamo DB table

The worker is running on a background and listening for an events that appears in specified Kafka Stream.
It depends on a type of event how it will be processed by worker:
- Saved to database
- Updated in database
- or deleted from database

### Useful links
- [Subscribing to a topic](https://docs.cloudera.com/runtime/7.2.10/kafka-developing-applications/topics/kafka-develop-subscribe.html)
- [How To Use Apache Kafka In .NET Application](https://www.c-sharpcorner.com/article/apache-kafka-net-application/)
- [Kafka Connect](https://docs.confluent.io/platform/current/connect/index.html#kafka-connect)
- [How to run DynamoDB Local and Offline - Complete Guide](https://dynobase.dev/run-dynamodb-locally/)

### Amazon DynamoDB client

Amazon Dynamo DB client developed with method that are self-explanatory:
````
Task CreateTableAsync();
Task SaveAsync(Book book);
Task UpdateItemAsync(Book book);
Task DeleteItemAsync(Book book);
Task<GetItemResponse> GetItemAsync(Book book);
````
The local Dynamo Db instance was used for development which was running on `http://localhost:8000` and fake or any AWS Credentials are required for this instance to work.

