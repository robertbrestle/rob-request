using System.Text;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Root endpoint with a short description of the available test endpoints.
app.MapGet("/", () => Results.Json(new
{
    name = "RobRequest.MockApi",
    description = "Minimal API for testing and development purposes.",
    endpoints = new[]
    {
        "/api/method           - responds to any HTTP method and echoes request details",
        "/api/status/{code}     - returns the given HTTP status code",
        "/api/timeout/{seconds} - returns a response after the given delay in seconds",
        "/api/json/{count?}     - returns automatically generated JSON (optionally {count} items)",
        "/api/json/{size}       - returns generated JSON of approx. the given size (eg. 5mb, 200kb)"
    }
}));

// ---------------------------------------------------------------------------
// 1. HTTP method endpoint - configurable through the HTTP method used.
//    GET /api/method performs a GET, POST /api/method performs a POST, etc.
// ---------------------------------------------------------------------------
var methodVerbs = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" };
app.MapMethods("/api/method", methodVerbs, async (HttpContext ctx) =>
{
    string body = string.Empty;
    if (ctx.Request.ContentLength is > 0 || ctx.Request.Headers.ContainsKey("Transfer-Encoding"))
    {
        using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8);
        body = await reader.ReadToEndAsync();
    }

    return Results.Json(new
    {
        method = ctx.Request.Method,
        path = ctx.Request.Path.Value,
        query = ctx.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString()),
        headers = ctx.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
        body
    });
});

// ---------------------------------------------------------------------------
// 2. Status code endpoint - returns the requested HTTP status code.
//    GET /api/status/200 -> 200, GET /api/status/404 -> 404, etc.
// ---------------------------------------------------------------------------
app.MapMethods("/api/status/{code:int}", methodVerbs, (int code) =>
{
    if (code is < 100 or > 599)
    {
        return Results.BadRequest(new { error = "Status code must be between 100 and 599." });
    }

    return Results.Json(new { status = code }, statusCode: code);
});

// ---------------------------------------------------------------------------
// 3. Timeout endpoint - returns a response after the given number of seconds.
//    GET /api/timeout/30 -> responds after 30 seconds.
// ---------------------------------------------------------------------------
app.MapGet("/api/timeout/{seconds:int}", async (int seconds, CancellationToken ct) =>
{
    if (seconds is < 0 or > 300)
    {
        return Results.BadRequest(new { error = "Seconds must be between 0 and 300." });
    }

    await Task.Delay(TimeSpan.FromSeconds(seconds), ct);
    return Results.Json(new { delayedSeconds = seconds });
});

// ---------------------------------------------------------------------------
// 4. JSON generation endpoint - returns automatically generated JSON.
//    GET /api/json       -> a single generated object.
//    GET /api/json/{n}   -> an array of n generated objects.
// ---------------------------------------------------------------------------
app.MapGet("/api/json", () => Results.Json(GenerateItem(1)));

app.MapGet("/api/json/{count:int}", (int count) =>
{
    switch (count)
    {
        case <= 0:
            return Results.Json("{}");
        case > 1_000_000:
            return Results.BadRequest(new
            {
                error = "Count must be less than 1,000,000." +
                        "Use /api/json/{size} for large response body tests."
            });
        default:
        {
            var items = Enumerable.Range(1, count).Select(GenerateItem).ToArray();
            return Results.Json(items);
        }
    }
});

// ---------------------------------------------------------------------------
// 5. JSON size endpoint - returns generated JSON of approximately the given size.
//    GET /api/json/5mb   -> ~5 MB of generated JSON.
//    GET /api/json/200kb -> ~200 KB of generated JSON.
// ---------------------------------------------------------------------------
app.MapGet("/api/json/{size}", (string size) =>
{
    var targetBytes = ParseSize(size);
    if (targetBytes is null)
    {
        return Results.BadRequest(new
        {
            error = "Invalid size format. Use a number followed by a unit: b, kb, mb, or gb (eg. 600b, 200kb, 5mb)."
        });
    }

    const long maxBytes = 100L * 1024 * 1024; // 100 MB safety cap
    if (targetBytes < 1 || targetBytes > maxBytes)
    {
        return Results.BadRequest(new { error = "Size must be between 1 byte and 100 MB." });
    }

    // Estimate how many items fill the target size using a sample serialisation.
    var sampleJson = System.Text.Json.JsonSerializer.Serialize(GenerateItem(1));
    var itemSize = System.Text.Encoding.UTF8.GetByteCount(sampleJson) + 1; // +1 for comma

    var count = (int)Math.Max(1, targetBytes.Value / itemSize);
    var items = Enumerable.Range(1, count).Select(GenerateItem).ToArray();
    return Results.Json(items);
});

app.Run();

return;

static long? ParseSize(string size)
{
    size = size.Trim().ToLowerInvariant();
    long multiplier;
    string numberPart;

    // if (size.EndsWith("gb"))
    // {
    //     multiplier = 1024L * 1024 * 1024;
    //     numberPart = size[..^2];
    // }
    // else
    if (size.EndsWith("mb"))
    {
        multiplier = 1024L * 1024;
        numberPart = size[..^2];
    }
    else if (size.EndsWith("kb"))
    {
        multiplier = 1024L;
        numberPart = size[..^2];
    }
    else if (size.EndsWith("b"))
    {
        multiplier = 1L;
        numberPart = size[..^1];
    }
    else if (long.TryParse(size, out var rawBytes)) return rawBytes;
    else return null;

    if (double.TryParse(numberPart,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out var number))
        return (long)(number * multiplier);

    return null;
}

static object GenerateItem(int id)
{
    var random = Random.Shared;
    var firstNames = new[] { "Alice", "Bob", "Charlie", "Diana", "Evan", "Fiona", "George", "Hannah" };
    var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };
    var first = firstNames[random.Next(firstNames.Length)];
    var last = lastNames[random.Next(lastNames.Length)];

    return new
    {
        id,
        guid = Guid.NewGuid(),
        firstName = first,
        lastName = last,
        email = $"{first.ToLowerInvariant()}.{last.ToLowerInvariant()}@example.com",
        age = random.Next(18, 80),
        isActive = random.Next(2) == 1,
        balance = Math.Round(random.NextDouble() * 10000, 2),
        createdAt = DateTime.UtcNow.AddDays(-random.Next(0, 365))
    };
}