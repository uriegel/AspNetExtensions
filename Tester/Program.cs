using AspNetExtensions;
using CsTools.Extensions;
using static AspNetExtensions.Core;

var startTime = DateTime.Now;

var sseEventSource = SseEventSource<Event>.Create();
StartEvents(sseEventSource.Send);

WebApplication
    .CreateBuilder(args)
    .ConfigureWebHost(webHostBuilder =>
        webHostBuilder
            .ConfigureKestrel(options => 
                options
                    .UseListenAnyIP(2000)
                    .UseLimits(limits => 
                        limits.SetMaxRequestBodySize(null))
            )
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
    .WithHost("illmatic")
        .WithMapGet("/test1i", () => "Test 1 illmatic")
        .WithMapGet("/test2i", () => "Test 2 illmatic")
        .WithReverseProxy("", "http://fritz.box")
        .GetApp()
    .WithHost("localhost")
        .WithMapGet("/test1l", () => "Test 1 localhost")
        .WithMapGet("/test2l", () => "Test 2 localhost")
        .GetApp()

    .WithMapGet("/test1", () => "Test 1")
    .WithMapGet("/test2", () => "Test 2")
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
            await context.SendStream(imageFile, startTime, imageFile.Name);
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
    .WithFileServer("/web", "webroot")
    .Start();

WebApplication
    .CreateBuilder(args)
    .ConfigureWebHost(webHostBuilder =>
        webHostBuilder
            .ConfigureKestrel(options => options.ListenAnyIP(2001))
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
    .WithHost("illmatic")
        .WithMapGet("/test1i", () => "Test 1 illmatic")
        .WithMapGet("/test2i", () => "Test 2 illmatic")
        .GetApp()
    .WithHost("localhost")
        .WithMapGet("/test1l", () => "Test 1 localhost")
        .WithMapGet("/test2l", () => "Test 2 localhost")
        .GetApp()

    .WithMapGet("/test1", () => "Test 1")
    .WithMapGet("/test2", () => "Test 2")
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
            await context.SendStream(imageFile, startTime, imageFile.Name);
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
    .WithFileServer("/web", "webroot")
    .SideEffect(_ => Console.WriteLine("Open http://localhost:2000/web/"))
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

// TODO React web page because of typescript Result
// TODO typescript fetch request with Result returning client side error (connection refused)
// TODO Json-Request server side returning AsyncResult ( or Result.ToAsyncResult()), catching all Exceptions like runtime error or JSON deserialize exception
// TODO Result must be extendable