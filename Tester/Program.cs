using AspNetExtensions;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;
using static AspNetExtensions.Core;
using static CsTools.Core;

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
            using var imageFile = File.OpenRead(Path.Combine(Environment.CurrentDirectory, "webroot", "Bild188.JPG"));
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
            .WithOrigins("http://localhost:5173")
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()))
    .WithJsonPost<Cmd1Param, Cmd1Result>("json/cmd1", JsonRequest)
    .WithJsonPost("requests/req1", request1)
    .WithJsonPost<Request2, Cmd1Result, ErrorResult>("requests/req2", request2)
    .WithJsonPost("requests/req3", request3)
    .WithJsonPost("requests/req7", request7)
    .WithJsonPost("requests/req8", request8)
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
            using var imageFile = File.OpenRead(Path.Combine(Environment.CurrentDirectory, "webroot", "Bild188.JPG"));
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
    .WithJsonPost<Cmd1Param, Cmd1Result>("json/cmd1", JsonRequest)
    .WithHost("localhost")
        .WithReverseProxy("", "http://localhost:5173")
        .GetApp()
    .WithRouting()
    .WithFileServer("/web", "webroot")
    .SideEffect(_ => Console.WriteLine("Open http://localhost:2000/web/"))
    .Run();

Task<Cmd1Result> JsonRequest(Cmd1Param? param)
    => new Cmd1Result("Result", 3).ToAsync(); 

AsyncResult<Cmd1Result, ErrorResult> request1() 
    => Ok<Cmd1Result, ErrorResult>(new Cmd1Result("Result", 999))
        .ToAsyncResult();
   
AsyncResult<Cmd1Result, ErrorResult> request2(Request2 payload) 
    => Ok<Cmd1Result, ErrorResult>(new Cmd1Result("Result2", 999))
        .ToAsyncResult();

AsyncResult<Cmd1Result, ErrorResult> request3() 
    => Error<Cmd1Result, ErrorResult>(new ErrorResult("An error has occurred", 17, 44, "error"))
        .ToAsyncResult();

AsyncResult<Cmd1Result, ErrorResult> request7()
    => throw new DivideByZeroException();

AsyncResult<Nothing, ErrorResult> request8()
    => Ok<Nothing, ErrorResult>(nothing)
        .ToAsyncResult();

void StartEvents(Action<Event> onChanged)   
{
    var counter = 0;
    new Thread(_ =>
        {
            while (true)
            {
                Thread.Sleep(5000);
                Task.Run(() => onChanged(new($"Ein Event {counter++}")));
                Task.Run(() => onChanged(new($"Ein Event {counter++}")));
           }
        }).Start();   
}

record Event(string Content);

record Cmd1Param(string Text, int Id);
record Cmd1Result(string Result, int Id);
record ErrorResult(
    string Msg, 
    int Code, 
    int Status, 
    string StatusText) 
    : RequestError(Status, StatusText);

record Request2(string Name, int Id);

