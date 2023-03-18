using AspNetExtensions;
using LinqTools;
using static AspNetExtensions.Core;

var startTime = DateTime.Now;

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
    .WithMapGet("/requests/icon", async context => 
        {
            
            var qs = context.Request.Query;
            var test = qs["path"].ToString();
            using var imageFile = File.OpenRead(Path.Combine(System.Environment.CurrentDirectory, "webroot", "Bild188.JPG"));
            var mime = imageFile.Name.GetMimeType();
            //TODO Extension function to serve a file
            bool isModified = context.CheckIsModified(startTime);
            // TODO 304 or serve
            context.Response.Headers.ContentType = mime;
            context.Response.Headers.LastModified = startTime.ToUnixTimestring();
            await imageFile.CopyToAsync(context.Response.Body, 8192);
        })
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
