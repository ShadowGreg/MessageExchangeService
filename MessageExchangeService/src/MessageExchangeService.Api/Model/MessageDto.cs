namespace MessageExchangeService.Api.Model;

public class MessageDto
{
    public string Content { get; set; } = string.Empty;
    public int SequenceNumber { get; set; }
    public DateTime Timestamp { get; set; } 
}