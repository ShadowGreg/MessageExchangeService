using Dapper;
using MessageExchangeService.Domain;
using Npgsql;
using System.Data;

namespace MessageExchangeService.Infrastructure.Repository;

public class MessageRepository
{
    private readonly string _connectionString;
    private readonly string _databaseName;

    public MessageRepository(string connectionString)
    {
        _connectionString = connectionString;
        _databaseName = new NpgsqlConnectionStringBuilder(connectionString).Database;
    }

    // Метод для удаления и пересоздания базы данных с нужными таблицами
    public async Task RecreateDatabaseAsync()
    {
        using var connection = new NpgsqlConnection(_connectionString);

        // Подключаемся к postgres, чтобы иметь возможность управлять базой данных
        var connectionStringWithoutDb = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Database = "postgres"
        }.ToString();

        using var masterConnection = new NpgsqlConnection(connectionStringWithoutDb);
        await masterConnection.OpenAsync();

        // Закрываем все активные соединения с базой данных
        await masterConnection.ExecuteAsync($@"
            SELECT pg_terminate_backend(pg_stat_activity.pid)
            FROM pg_stat_activity
            WHERE pg_stat_activity.datname = '{_databaseName}' AND pid <> pg_backend_pid();");

        // Удаляем существующую базу данных, если она есть
        await masterConnection.ExecuteAsync($"DROP DATABASE IF EXISTS \"{_databaseName}\"");

        // Создаем новую базу данных
        await masterConnection.ExecuteAsync($"CREATE DATABASE \"{_databaseName}\"");

        // Подключаемся к новой базе данных и создаем таблицы
        var newConnectionString = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Database = _databaseName
        }.ToString();

        using var newConnection = new NpgsqlConnection(newConnectionString);
        await newConnection.OpenAsync();
        await CreateTablesAsync(newConnection);
    }

    // Метод для создания необходимых таблиц
    private async Task CreateTablesAsync(IDbConnection connection)
    {
        const string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Messages (
                Id SERIAL PRIMARY KEY,
                Content TEXT NOT NULL,
                Timestamp TIMESTAMPTZ NOT NULL,
                SequenceNumber INT NOT NULL
            );
        ";

        await connection.ExecuteAsync(createTableQuery);
    }

    // Метод для добавления сообщения в базу данных
    public async Task<int> AddMessageAsync(Message message)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = @"
            INSERT INTO Messages (Content, Timestamp, SequenceNumber) 
            VALUES (@Content, @Timestamp, @SequenceNumber) 
            RETURNING Id";
        return await connection.ExecuteScalarAsync<int>(query, message);
    }

    // Метод для получения сообщений в заданном диапазоне дат
    public async Task<IEnumerable<Message>> GetMessagesAsync(DateTime start, DateTime end)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        var query = "SELECT * FROM Messages WHERE Timestamp BETWEEN @Start AND @End";
        return await connection.QueryAsync<Message>(query, new { Start = start, End = end });
    }
}