namespace Kafka.EventConsumer;

public class Message
{
    public Guid Id { get; set; }
    public Type Type { get; set; }
    public Book? Book { get; set; }
}

public class Book
{
    public Guid BookId { get; set; }
    public Guid AuthorId { get; set; }
    public string Title { get; set; }
    public string Publisher { get; set; }
    public int NumberOfPages { get; set; }
    public DateTime Published { get; set; }
}

public enum Type
{
    CreatedMessage = 0,
    UpdatedMessage = 1,
    DeletedMessage = 2
}