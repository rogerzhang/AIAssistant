namespace PersonalizedAssistant.Infrastructure.Configuration;

public class AppSettings
{
    public MongoDbSettings MongoDb { get; set; } = new();
    public JwtSettings Jwt { get; set; } = new();
    public GoogleApiSettings GoogleApi { get; set; } = new();
    public GrpcSettings Grpc { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

public class GoogleApiSettings
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();
}

public class GrpcSettings
{
    public string AuthServiceUrl { get; set; } = string.Empty;
    public string DataCollectionServiceUrl { get; set; } = string.Empty;
    public string DataProcessingServiceUrl { get; set; } = string.Empty;
    public string ChatAgentServiceUrl { get; set; } = string.Empty;
}

public class LoggingSettings
{
    public string LogLevel { get; set; } = "Information";
    public string LogPath { get; set; } = "logs";
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = true;
}
