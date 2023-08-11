using LinqTools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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

    WebApplication app;
    string host;
}