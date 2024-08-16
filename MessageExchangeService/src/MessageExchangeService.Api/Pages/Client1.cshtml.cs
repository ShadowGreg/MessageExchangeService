using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MessageExchangeService.Api.Pages;

public class Client1Model : PageModel
{
    private readonly ILogger<Client1Model> _logger;

    public Client1Model(ILogger<Client1Model> logger)
    {
        _logger = logger;
    }

    [BindProperty] public string MessageContent { get; set; }

    [BindProperty] public int SequenceNumber { get; set; }

    public bool Success { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("OnPostAsync started for Client1Model");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state.");
            return Page();
        }

        using var client = new HttpClient();
        var content = new StringContent(
            JsonSerializer.Serialize(new { Content = MessageContent, SequenceNumber = SequenceNumber }),
            Encoding.UTF8,
            "application/json"
        );

        var response = await client.PostAsync("http://localhost:5252/api/messages", content);
        Success = response.IsSuccessStatusCode;

        if (Success)
        {
            _logger.LogInformation("Message sent successfully.");
        }
        else
        {
            _logger.LogError("Failed to send message. Status code: {StatusCode}", response.StatusCode);
        }

        return Page();
    }
}