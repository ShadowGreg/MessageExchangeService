using MessageExchangeService.Api.Model;
using MessageExchangeService.Domain;
using MessageExchangeService.Infrastructure.Repository;
using MessageExchangeService.infrastructure.Service;
using Microsoft.AspNetCore.Mvc;

namespace MessageExchangeService.Api.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly MessageRepository _repository;
    private readonly IWebSocketService _webSocketService;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(MessageRepository repository, IWebSocketService webSocketService,
        ILogger<MessagesController> logger)
    {
        _repository = repository;
        _webSocketService = webSocketService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PostMessage([FromBody] MessageDto messageDto)
    {
        _logger.LogInformation("Received a new message for processing.");

        var message = new Message
        {
            Content = messageDto.Content,
            Timestamp = DateTime.UtcNow,
            SequenceNumber = messageDto.SequenceNumber
        };

        _logger.LogInformation(
            "Message created: Content = {Content}, SequenceNumber = {SequenceNumber}, Timestamp = {Timestamp}",
            message.Content, message.SequenceNumber, message.Timestamp);

        try
        {
            var id = await _repository.AddMessageAsync(message);
            message.Id = id;

            _logger.LogInformation("Message saved to database with Id = {Id}", id);

            // Отправка сообщения второму клиенту через WebSocket
            await _webSocketService.BroadcastMessageAsync(message);

            _logger.LogInformation("Message broadcasted to WebSocket clients.");

            return Ok(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the message.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        _logger.LogInformation("Retrieving messages between {Start} and {End}", start, end);

        try
        {
            var messages = await _repository.GetMessagesAsync(start, end);
            _logger.LogInformation("Retrieved {Count} messages.", messages.Count());

            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving messages.");
            return StatusCode(500, "Internal server error");
        }
    }
}