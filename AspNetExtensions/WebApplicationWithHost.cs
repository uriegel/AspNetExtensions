using AspNetExtensions;
using LinqTools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

public class WebApplicationWithHost
{
    public WebApplicationWithHost(WebApplication app, string host)
    {
        this.app = app;
        this.host = host;
    }

    public WebApplication GetApp() => app;

    public WebApplicationWithHost WithMapGet(string pattern, RequestDelegate requestDelegate)
        => this.SideEffect(_ => app.MapGet(pattern, requestDelegate).RequireHost(host));

    public WebApplicationWithHost WithMapGet(string pattern, Delegate handler)
        => this.SideEffect(_ => app.MapGet(pattern, handler).RequireHost(host));

    public WebApplicationWithHost WithMapPost(string pattern, RequestDelegate requestDelegate)
        => this.SideEffect(_ => app.MapPost(pattern, requestDelegate).RequireHost(host));

    public WebApplicationWithHost WithStaticFiles(string requestUrl, string path)
        => this.SideEffect(_ => app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), path)),
            RequestPath = requestUrl
        }));

    public WebApplicationWithHost WithFileServer(string requestUrl, string path)
        => this.SideEffect(_ => app.UseFileServer(new FileServerOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), path)),
            RequestPath = requestUrl,
        }));

    public WebApplicationWithHost When(bool when, Func<WebApplication, WebApplication> handler)
    {
        app = when
            ? handler(app)
            : app;
        return this;
    }

    public WebApplicationWithHost WithMapSubPath(string path, Func<HttpContext, string, Task> handler)
        => this.SideEffect(_ => app.With(async (context, next) =>
        {
            if (context.Request.Path.ToString().StartsWith(path))
            {
                await handler(context, context.Request.Path.ToString()[(path.Length + 1)..]);
                return;
            }
            await next(context);
        }));

    public WebApplicationWithHost WithSse<TEvent>(string path, SseEventSource<TEvent> sseEventSource)
        => this.SideEffect(_ => app.WithMapGet(path, context => new Sse<TEvent>(sseEventSource.Subject).Start(context)));

    public WebApplicationWithHost WithSse<TEvent>(string path, IObservable<TEvent> onNext)
        => this.SideEffect(_ => app.WithMapGet(path, context => new Sse<TEvent>(onNext).Start(context)));

    public WebApplicationWithHost WithJsonPost<T, TResult>(string path, Func<T, Task<TResult>> onJson)
        => this.SideEffect(_ => app.WithMapPost(path, async context => 
        {
            var param = await context.Request.ReadFromJsonAsync<T>();
            await context.Response.WriteAsJsonAsync<TResult>(await onJson(param!));
        }));

    WebApplication app;
    string host;
}