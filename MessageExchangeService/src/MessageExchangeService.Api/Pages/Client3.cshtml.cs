using System.Text.Json;
using MessageExchangeService.Api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MessageExchangeService.Api.Pages;

public class Client3Model : PageModel
{
    public List<MessageDto> RecentMessages { get; private set; } = new List<MessageDto>();

    public async Task<IActionResult> OnGetLoadMessagesAsync(int minutesAgo = 1)
    {
        var end = DateTime.UtcNow;
        var start = end.AddMinutes(-minutesAgo);

        using var client = new HttpClient();
        var response = await client.GetAsync($"http://localhost:5252/api/messages?start={start:o}&end={end:o}");
        var json = await response.Content.ReadAsStringAsync();

        RecentMessages = JsonSerializer.Deserialize<List<MessageDto>>(json) ?? new List<MessageDto>();

        return new JsonResult(RecentMessages);
    }
}