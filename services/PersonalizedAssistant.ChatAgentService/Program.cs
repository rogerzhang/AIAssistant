using PersonalizedAssistant.Infrastructure.Extensions;
using PersonalizedAssistant.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddGrpc();

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add logging
builder.Services.AddLogging(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapGrpcService<PersonalizedAssistant.Infrastructure.Services.ChatAgentService>();

app.Run();
