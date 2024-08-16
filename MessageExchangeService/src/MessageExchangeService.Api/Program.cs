using MessageExchangeService.Infrastructure.Repository;
using MessageExchangeService.infrastructure.Service;
using MessageExchangeService.infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);


// Настройка логирования
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddHttpClient();

// Чтение строки подключения из конфигурации
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var urls = builder.Configuration.GetValue<string>("Urls") ?? "http://*:5252";
builder.WebHost.UseUrls(urls);

// Добавляем CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(urls ?? "http://*:5252") 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();


// Добавление кастомных сервисов с использованием строки подключения
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();

builder.Services.AddSingleton<MessageRepository>(
    provider =>
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new AggregateException("Connection string is empty or null");
        return new MessageRepository(connectionString);
    }
);

var messageRepository = new MessageRepository(connectionString);

var app = builder.Build();

// Получение логгера для использования в программе
var logger = app.Logger;

logger.LogInformation("Application starting...");

await messageRepository.RecreateDatabaseAsync();
logger.LogInformation("Application messageRepository starting...");


app.UseCors();
logger.LogInformation("Application addCors starting...");

// Конфигурация HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    logger.LogInformation("Swagger is enabled in development environment.");
}

app.UseHttpsRedirection();
logger.LogInformation("Application add UseHttpsRedirection.");
app.UseStaticFiles();
logger.LogInformation("Application add UseStaticFiles.");

app.UseWebSockets();
logger.LogInformation("Application add UseWebSockets.");

app.MapControllers();
logger.LogInformation("Application add MapControllers.");

app.MapRazorPages();
logger.LogInformation("Application add MapRazorPages.");

app.Run();

logger.LogInformation("Application has started successfully.");