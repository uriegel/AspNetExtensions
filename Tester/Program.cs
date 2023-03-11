
using AspNetExtensions;

using static AspNetExtensions.Core;

var sseEventSource = SseEventSource<Event>.Create();
StartEvents(sseEventSource.Send);

WebApplication
    .CreateBuilder(args)
    .ConfigureWebHost(webHostBuilder =>
        webHostBuilder
            .ConfigureKestrel(options => options.ListenAnyIP(19999))
            .ConfigureServices(services =>
                services
                    .When(true, s => s.AddCors())
                    .AddResponseCompression())
            .ConfigureLogging(builder =>
                builder
                    .AddFilter(a => a == LogLevel.Warning)
                    .AddConsole()
                    .AddDebug()))
    .Build()
    .WithResponseCompression()
    .With(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            await context.Response.WriteAsync("Terminal Middleware.");
            return;
        }
        await next(context);
    })
    .WithMapGet("/test", () => "Das ist der Test")
    .WithMapGet("/filmseite", context =>
        {
            context.Response.Headers.ContentType = "text/html";
            return context.Response.WriteAsync(VideoPage.Value);
        })
    .WithMapGet("/film", context => context.StreamRangeFile("/home/uwe/Videos/Buster Keaton - Der Navigator.mp4"))
    .WithMapGet("/json/{name:alpha}", async context =>
        {
            var name = context.Request.RouteValues["name"];
            await context.Response.WriteAsJsonAsync(new { Message = $"Hello {name}", Mist = (string?)null }, JsonWebDefaults);
        })
    .WithSse("/commander/sse", sseEventSource)
    .When(true, app => app.WithCors(builder =>
        builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()))
    .WithRouting()
    .Run();

void StartEvents(Action<Event> onChanged)   
{
    var counter = 0;
    new Thread(_ =>
        {
            while (true)
            {
                Thread.Sleep(5000);
                onChanged(new($"Ein Event {counter++}"));
           }
        }).Start();   
}

record Event(string Theme);

// TODO index.html
// TODO script.css
// TODO sse events to console
// TODO Rest interface with Post (JSON -> JSON)
// TODO buttons to test rest interface
// TODO SendFiles (ContentType, lastModified)
// TODO GetMimeType
// TODO Delete Kestrel project