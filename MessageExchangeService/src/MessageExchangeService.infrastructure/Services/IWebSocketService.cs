using MessageExchangeService.Domain;

namespace MessageExchangeService.infrastructure.Service;

public interface IWebSocketService
{
    Task BroadcastMessageAsync(Message message);
}