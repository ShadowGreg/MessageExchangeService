namespace MessageExchangeService.Domain;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int SequenceNumber { get; set; }
}