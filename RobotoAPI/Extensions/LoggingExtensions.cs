using System;
using Microsoft.Extensions.Logging;

namespace ChatApp.Extensions;

public static class LoggingExtensions
{
    public static void LogDuration(this ILogger logger, string operation, DateTime startTime)
    {
        var duration = DateTime.UtcNow - startTime;
        logger.LogInformation("{Operation} completed in {Duration}ms", 
            operation, duration.TotalMilliseconds);
    }
} 