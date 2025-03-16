using System;

namespace ChatApp.Services.Models;

public record QueryResult(string Response, string Context, TimeSpan TotalDuration); 