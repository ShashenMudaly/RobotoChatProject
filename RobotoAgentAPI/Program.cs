using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Azure.AI.OpenAI;
using Azure;
using RobotoAgentAPI.Agents;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .WithMethods("GET", "POST")
              .WithHeaders("Content-Type", "Authorization");
    });
});

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "TooManyRequests",
            message = "Rate limit exceeded",
            details = "Please try again later"
        }, token);
    };
});

// Configure Semantic Kernel
builder.Services.AddSingleton<Kernel>(sp =>
{
    var openAiEndpoint = builder.Configuration["AzureOpenAI:Endpoint"] 
        ?? throw new InvalidOperationException("Azure OpenAI endpoint is not configured");
    var openAiKey = builder.Configuration["AzureOpenAI:ApiKey"] 
        ?? throw new InvalidOperationException("Azure OpenAI API key is not configured");
    var bonoEndpoint = builder.Configuration["BonoSearch:Endpoint"] 
        ?? throw new InvalidOperationException("Bono Search endpoint is not configured");
    var bonoApiKey = builder.Configuration["BonoSearch:ApiKey"] ?? string.Empty;

    var kernel = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            deploymentName: "o3-mini",
            endpoint: openAiEndpoint,
            apiKey: openAiKey)
        .Build();

    // Get the chat completion service for both agents
    var chatCompletion = kernel.GetRequiredService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>();

    // Register plugins after kernel is built
    var chatAgentPlugin = new ChatAgentPlugin(chatCompletion);
    var searchAgentPlugin = new SearchAgentPlugin(bonoApiKey, bonoEndpoint, chatCompletion);
    
    kernel.ImportPluginFromObject(chatAgentPlugin, "ChatAgent");
    kernel.ImportPluginFromObject(searchAgentPlugin, "SearchAgent");

    return kernel;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();

app.MapControllers();

app.Run(); 