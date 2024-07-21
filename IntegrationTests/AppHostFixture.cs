using Dapper;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Respawn;
using System.Data.SqlClient;
using Testcontainers.MsSql;

namespace IntegrationTests;

public class AppHostFixture : IAsyncLifetime
{
    private MsSqlContainer _container = default!;
    private const string Username = "sa";
    private const string Password = "S3nh@V@lid4";
    private const string Database = "testdb";
    private const int MsSqlPort = 1433;

    private Respawner _respawner = default!;
    private WebApplicationFactory<Program> _factory = default!;
    private HttpClient _client = default!;

    private static readonly string SetupPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "init_script.sql");


    public async Task InitializeAsync()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPortBinding(MsSqlPort, true)
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithEnvironment("SQLCMDUSER", Username)
            .WithEnvironment("SQLCMDPASSWORD", Password)
            .WithEnvironment("MSSQL_SA_PASSWORD", Password)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(MsSqlPort))
            .Build();

        // Start Container
        await _container.StartAsync();

        var sqlFilePath = SetupPath;

        // Verifique se o arquivo existe
        if (!File.Exists(sqlFilePath))
        {
            throw new FileNotFoundException("SQL initialization script not found.", sqlFilePath);
        }

        // Replace connection string in DbContext
        var connectionString = GetConnectionString();
        // Ensure the database exists
        await CreateDatabaseIfNotExistsAsync(connectionString);
        await InitializeDatabaseAsync(connectionString, SetupPath);

        _respawner = Respawner.CreateAsync(connectionString, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer
        }).GetAwaiter().GetResult();

        _factory = new WebApplicationFactory<Program>()
           .WithWebHostBuilder(builder =>
           {
               builder.ConfigureAppConfiguration((context, config) =>
               {
                   var builtConfig = config.Build();
                   var connectionString = GetConnectionString();

                   if (string.IsNullOrWhiteSpace(connectionString))
                   {
                       throw new InvalidOperationException("A conexão com o banco de dados não pode ser vazia.");
                   }

                   config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                   {
                        new KeyValuePair<string, string?>(
                            "ConnectionStrings:DefaultConnection",
                            connectionString)
                   });
               });
           });

        _client = _factory.CreateClient();
    }

    public HttpClient CreateClient() => _client;

    public string GetConnectionString()
    {
        var host = _container.Hostname;
        var port = _container.GetMappedPublicPort(MsSqlPort);
        return $"Server={host},{port};Database={Database};User Id={Username};Password={Password};TrustServerCertificate=True";
    }

    private async Task CreateDatabaseIfNotExistsAsync(string connectionString)
    {
        // Conecte-se ao banco de dados master para criar o banco de dados
        var masterConnectionString = connectionString.Replace($"Database={Database};", "Database=master;");

        using var masterConnection = new SqlConnection(masterConnectionString);
        await masterConnection.OpenAsync();

        var databaseExistsQuery = $@"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{Database}')
            BEGIN
                CREATE DATABASE [{Database}];
            END";

        using var command = new SqlCommand(databaseExistsQuery, masterConnection);
        await command.ExecuteNonQueryAsync();
    }

    private async Task InitializeDatabaseAsync(string connectionString, string sqlFilePath)
    {
        var sql = await File.ReadAllTextAsync(sqlFilePath);

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql);
    }

    public async Task ResetAsync()
    {
        var connectionString = GetConnectionString();
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        var connectionString = GetConnectionString();
        var masterConnectionString = connectionString.Replace($"Database={Database};", "Database=master;");

        using var masterConnection = new SqlConnection(masterConnectionString);
        await masterConnection.OpenAsync();

        var deleteDatabaseCommand = @$"IF EXISTS (SELECT [name] FROM sys.databases WHERE [name] = N'{Database}')
        BEGIN
            ALTER DATABASE [{Database}]
            SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE [{Database}];
        END;";

        using var command = new SqlCommand(deleteDatabaseCommand, masterConnection);
        await command.ExecuteNonQueryAsync();
        await _container.StopAsync();
    }
}
