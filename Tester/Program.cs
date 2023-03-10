
using AspNetExtensions;

using static AspNetExtensions.Core;

var sse = new Sse<RendererEvent>();
// Theme.StartThemeDetection(sse.SetTheme);

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
    .WithMapGet("/commander/sse", h => sse.Start(h))
    .WithMapGet("/json/{name:alpha}", async context =>
        {
            var name = context.Request.RouteValues["name"];
            await context.Response.WriteAsJsonAsync(new { Message = $"Hello {name}", Mist = (string?)null }, JsonWebDefaults);
        })
    .When(true, app => app.WithCors(builder =>
        builder
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()))
    .WithRouting()
    .Run();

record RendererEvent(string Theme);
// TODO Inject new Sse

