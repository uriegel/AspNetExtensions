using AspNetExtensions;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public class WebApplicationWithHost(WebApplication app, string host)
{
    public WebApplication GetApp() => app;

    public WebApplicationWithHost WithMapGet(string pattern, RequestDelegate requestDelegate)
        => this.SideEffect(_ => app.MapGet(pattern, requestDelegate).RequireHost(host));

    public WebApplicationWithHost WithMapGet(string pattern, Delegate handler)
        => this.SideEffect(_ => app.MapGet(pattern, handler).RequireHost(host));

    public WebApplicationWithHost WithJsonGet<TResult>(string path, Func<string?, AsyncResult<TResult, RequestError>> onJson)
        where TResult : class
        => this.SideEffect(a => a.WithMapGet(path, async context =>
            await onJson(context.GetRouteValue("path") as string)
                .ToResult()
                .MatchAsync(r => context.Response.WriteAsJsonAsync(r),
                     e => context
                         .StatusError(e.Status, e.StatusText))));

    public WebApplicationWithHost WithMapPost(string pattern, RequestDelegate requestDelegate)
        => this.SideEffect(_ => app.MapPost(pattern, requestDelegate).RequireHost(host));

    public WebApplicationWithHost WithMapPut(string pattern, RequestDelegate requestDelegate)
        => this.SideEffect(_ => app.MapPut(pattern, requestDelegate).RequireHost(host));

    public WebApplicationWithHost WithMapDelete(string pattern, RequestDelegate requestDelegate)
        => this.SideEffect(_ => app.MapDelete(pattern, requestDelegate).RequireHost(host));

    public WebApplicationWithHost WithSse<TEvent>(string path, SseEventSource<TEvent> sseEventSource)
        => this.SideEffect(a => a.WithMapGet(path, context => new Sse<TEvent>(sseEventSource.Subject).Start(context)));

    public WebApplicationWithHost WithSse<TEvent>(string path, IObservable<TEvent> onNext)
        => this.SideEffect(a => a.WithMapGet(path, context => new Sse<TEvent>(onNext).Start(context)));

    public WebApplicationWithHost WithJsonPost<T, TResult>(string path, Func<T, Task<TResult>> onJson)
        => this.SideEffect(a => a.WithMapPost(path, async context => 
        {
            var param = await context.Request.ReadFromJsonAsync<T>();
            await context.Response.WriteAsJsonAsync(await onJson(param!));
        }));

    public WebApplicationWithHost WithReverseProxy(string pattern, string reverseUrl)
        => this.SideEffect(_ => app.WithReverseProxy(pattern, reverseUrl).RequireHost(host));

    readonly WebApplication app = app;
    readonly string host = host;
}