using System.Text.Json;
using MessageExchangeService.Api.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MessageExchangeService.Api.Pages;

public class Client2Model : PageModel
{
    private readonly ILogger<Client2Model> _logger;
    private readonly HttpClient _httpClient;

    public Client2Model(ILogger<Client2Model> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public List<MessageDto> Messages { get; private set; } = new List<MessageDto>();

    public async Task OnGetAsync()
    {
        _logger.LogInformation("OnGetAsync started for Client2Model");

        try
        {
            var end = DateTime.UtcNow;
            var start = end.AddHours(-1); // Получаем сообщения за последний час

            var response = await _httpClient.GetAsync($"http://localhost:5252/api/messages?start={start:o}&end={end:o}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Messages = JsonSerializer.Deserialize<List<MessageDto>>(json) ?? new List<MessageDto>();
                _logger.LogInformation("Successfully retrieved {Count} messages.", Messages.Count);
            }
            else
            {
                _logger.LogWarning("Failed to retrieve messages. Status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while retrieving messages.");
        }
    }
}