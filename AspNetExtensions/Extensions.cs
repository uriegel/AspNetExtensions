using Microsoft.AspNetCore.Cors.Infrastructure;

using static Giraffe.Streaming.StreamingExtensions;
using LinqTools;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

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

    public static Task StreamRangeFile(this HttpContext context, string filePath)
        => context.WriteFileStreamAsync(true, filePath, Microsoft.FSharp.Core.FSharpOption<Microsoft.Net.Http.Headers.EntityTagHeaderValue>.None,
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

    public static WebApplication With(this WebApplication webApp, IEnumerable<Func<WebApplication, WebApplication>> handlers)
    {
        var result = webApp;
        foreach (var handler in handlers)
            result = handler(result);
        return result;
    }
}


