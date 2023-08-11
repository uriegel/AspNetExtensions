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

    public WebApplicationWithHost WithSse<TEvent>(string path, SseEventSource<TEvent> sseEventSource)
        => this.SideEffect(a => a.WithMapGet(path, context => new Sse<TEvent>(sseEventSource.Subject).Start(context)));

    public WebApplicationWithHost WithSse<TEvent>(string path, IObservable<TEvent> onNext)
        => this.SideEffect(a => a.WithMapGet(path, context => new Sse<TEvent>(onNext).Start(context)));

    public WebApplicationWithHost WithJsonPost<T, TResult>(string path, Func<T, Task<TResult>> onJson)
        => this.SideEffect(a => a.WithMapPost(path, async context => 
        {
            var param = await context.Request.ReadFromJsonAsync<T>();
            await context.Response.WriteAsJsonAsync<TResult>(await onJson(param!));
        }));

    WebApplication app;
    string host;
}