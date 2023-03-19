using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Cors.Infrastructure;

using CsTools.Extensions;
using LinqTools;

using static Giraffe.Streaming.StreamingExtensions;


namespace AspNetExtensions;

public static class Extensions
{
    public static WebApplicationBuilder ConfigureWebHost(this WebApplicationBuilder appBuilder, Func<IWebHostBuilder, IWebHostBuilder> webHost)
        => appBuilder.SideEffect(n => webHost(n.WebHost));

    public static WebApplication With(this WebApplication app, Func<HttpContext, RequestDelegate, Task> middleware)
        => app.SideEffect(a => a.Use(middleware));

    public static WebApplication WithMapGet(this WebApplication app, string pattern, RequestDelegate requestDelegate)
        => app.SideEffect(a => a.MapGet(pattern, requestDelegate));

    public static WebApplication WithMapGet(this WebApplication app, string pattern, Delegate handler)
        => app.SideEffect(a => a.MapGet(pattern, handler));

    public static WebApplication WithMapPost(this WebApplication app, string pattern, RequestDelegate requestDelegate)
        => app.SideEffect(a => a.MapPost(pattern, requestDelegate));
    public static WebApplication WithResponseCompression(this WebApplication app)
        => app.SideEffect(a => a.UseResponseCompression());

    public static WebApplication WithCors(this WebApplication app, Action<CorsPolicyBuilder> builder)
        => app.SideEffect(a => a.UseCors(builder));

    public static WebApplication WithRouting(this WebApplication app)
        => app.SideEffect(a => a.UseRouting());

    public static WebApplication WithEndpoints(this WebApplication app, Action<IEndpointRouteBuilder> configure)
        => app.SideEffect(a => a.UseEndpoints(configure));

    public static WebApplication WithStaticFiles(this WebApplication app, string requestUrl, string path)
        => app.SideEffect(a => a.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), path)),
            RequestPath = requestUrl
        }));

    public static WebApplication WithFileServer(this WebApplication app, string requestUrl, string path)
        => app.SideEffect(a => a.UseFileServer(new FileServerOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), path)),
            RequestPath = requestUrl,
        }));

    public static WebApplication When(this WebApplication webApp, bool when, Func<WebApplication, WebApplication> handler)
        => when
            ? handler(webApp)
            : webApp;

    public static WebApplication WithMapSubPath(this WebApplication app, string path, Func<HttpContext, string, Task> handler)
        => app.With(async (context, next) =>
        {
            if (context.Request.Path.ToString().StartsWith(path))
            {
                await handler(context, context.Request.Path.ToString()[(path.Length + 1)..]);
                return;
            }
            await next(context);
        });

    public static WebApplication WithSse<TEvent>(this WebApplication webApp, string path, SseEventSource<TEvent> sseEventSource)
        => webApp.WithMapGet(path, context => new Sse<TEvent>(sseEventSource.Subject).Start(context));

    public static WebApplication WithSse<TEvent>(this WebApplication webApp, string path, IObservable<TEvent> onNext)
        => webApp.WithMapGet(path, context => new Sse<TEvent>(onNext).Start(context));

    public static async Task StreamRangeFile(this HttpContext context, string filePath)
        => await context.WriteFileStreamAsync(true, filePath, Microsoft.FSharp.Core.FSharpOption<Microsoft.Net.Http.Headers.EntityTagHeaderValue>.None,
        Microsoft.FSharp.Core.FSharpOption<DateTimeOffset>.None);

    public static IServiceCollection When(this IServiceCollection services, bool when, Func<IServiceCollection, IServiceCollection> handler)
        => when
            ? handler(services)
            : services;

    public static WebApplication WithJsonPost<T, TResult>(this WebApplication webApp, string path, Func<T, Task<TResult>> onJson)
        => webApp.WithMapPost(path, async context => 
        {
            var param = await context.Request.ReadFromJsonAsync<T>();
            await context.Response.WriteAsJsonAsync<TResult>(await onJson(param!));
        });

    public static string GetMimeType(this string file)
        => new FileExtensionContentTypeProvider().TryGetContentType(file, out var contentType)
            ? contentType
            : "application/octet-stream";

    public static string ToUnixTimestring(this DateTime localTime)
        => localTime.ToUniversalTime().ToString("r");

    public static bool CheckIsModified(this HttpContext context, DateTime? lastWriteTime)
    => lastWriteTime.HasValue
        ? (from n in context
                        .Request
                        .Headers
                        .IfModifiedSince
                        .ToString()
                        .SubstringUntil(';')
                        .WhiteSpaceToNull()
                        ?.FromString()
                        .ToRef()
           let r = lastWriteTime.Value.TruncateMilliseconds() > n
           select r.ToRef())
                .GetOrDefault(true)
        : true;

    public static async Task SendStream(this HttpContext context, Stream stream, DateTime? lastWriteTime, string? fileName = null)
    {
        var mime = fileName?.GetMimeType();
        bool isModified = context.CheckIsModified(lastWriteTime);
        if (isModified)
        {
            context.Response.Headers.ContentType = mime;
            if (lastWriteTime.HasValue)
            context.Response.Headers.LastModified = lastWriteTime.Value.ToUnixTimestring();
            await stream.CopyToAsync(context.Response.Body, 8192);
        }
        else
            context.Response.StatusCode = 304;
    }
    
    public static DateTime FromString(this string timeString)
        => Convert.ToDateTime(timeString);

    public static DateTime TruncateMilliseconds(this DateTime dt)
        =>  dt.AddTicks( - (dt.Ticks % TimeSpan.TicksPerSecond));        

    public static WebApplication With(this WebApplication webApp, IEnumerable<Func<WebApplication, WebApplication>> handlers)
    {
        var result = webApp;
        foreach (var handler in handlers)
            result = handler(result);
        return result;
    }
}


