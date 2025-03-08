using StackExchange.Redis;
using ChatApp.Services;
using ChatApp.Services.Interfaces;
using ChatApp.Options;
using Microsoft.Extensions.Options;
using Azure.AI.TextAnalytics;
using Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Redis configuration
var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(redisConnectionString));

// Add OpenAI configuration
builder.Services.Configure<ChatClientOptions>(options =>
{
    options.DeploymentName = builder.Configuration["AzureOpenAI:DeploymentName"] ?? "";
});

builder.Services.AddSingleton(sp => 
{
    var endpoint = builder.Configuration["AzureOpenAI:Endpoint"] ?? "";
    var key = builder.Configuration["AzureOpenAI:Key"] ?? "";
    return new Azure.AI.OpenAI.OpenAIClient(
        new Uri(endpoint), 
        new Azure.AzureKeyCredential(key));
});

builder.Services.AddSingleton<IChatClient, ChatClient>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IMovieSearchService, MovieSearchService>();
builder.Services.AddSingleton<IChatCacheRepository, ChatCacheRepository>();
builder.Services.AddSingleton<IMovieConversationOrchestrator, MovieConversationOrchestrator>();

// Add Text Analytics configuration
builder.Services.Configure<TextAnalyticsOptions>(
    builder.Configuration.GetSection("AzureLanguage"));

builder.Services.AddSingleton(sp => 
{
    var options = sp.GetRequiredService<IOptions<TextAnalyticsOptions>>();
    return new TextAnalyticsClient(
        new Uri(options.Value.Endpoint), 
        new Azure.AzureKeyCredential(options.Value.Key));
});

builder.Services.AddSingleton<ITextSummarizationService, TextSummarizationService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Use CORS before other middleware
app.UseCors("AllowReactApp");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
