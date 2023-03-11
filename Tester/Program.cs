using AspNetExtensions;
using LinqTools;
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
    // .With(async (context, next) =>
    // {
    //     if (context.Request.Path == "/")
    //     {
    //         await context.Response.WriteAsync("Terminal Middleware.");
    //         return;
    //     }
    //     await next(context);
    // })
    .WithMapGet("/test", () => "Das ist der Test")
    .WithMapGet("/cinema", context =>
        {
            context.Response.Headers.ContentType = "text/html";
            return context.Response.WriteAsync(VideoPage.Value);
        })
    .WithMapGet("/video", context => context.StreamRangeFile("/home/uwe/Videos/Buster Keaton - Der Navigator.mp4"))
    .WithMapGet("/json/{name:alpha}", async context =>
        {
            var name = context.Request.RouteValues["name"];
            await context.Response.WriteAsJsonAsync(new { Message = $"Hello {name}", Mist = (string?)null }, JsonWebDefaults);
        })
    .WithSse("/sse/test", sseEventSource)
    .When(true, app => app.WithCors(builder =>
        builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()))
    .WithJsonPost<Cmd1Param, Cmd1Result>("json/cmd1", JsonRequest1)
    .WithRouting()
    .WithFileServer("", "webroot")
    .Run();

Task<Cmd1Result> JsonRequest1(Cmd1Param param)
    => new Cmd1Result("Result", 3).ToAsync(); 

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

record Event(string Content);

record Cmd1Param(string Text, int Id);
record Cmd1Result(string Result, int Id);

// TODO GetMimeType
// string GetMimeTypeForFileExtension(string filePath)
// {
//     const string DefaultContentType = "application/octet-stream";

//     var provider = new FileExtensionContentTypeProvider();

//     if (!provider.TryGetContentType(filePath, out var contentType))
//     {
//         contentType = DefaultContentType;
//     }

//     return contentType;
// }
