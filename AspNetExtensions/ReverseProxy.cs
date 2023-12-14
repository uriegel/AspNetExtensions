using CsTools.Async;
using CsTools.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using static CsTools.Functional.Memoization;

namespace AspNetExtensions;

public static class ReverseProxy
{
    public static Task Delegate(HttpContext context, string reverseUrl)
        => new HttpRequestMessage(
            context.Request.Method.ToHttpMethod(), 
            $"{reverseUrl}/{context.GetRouteValue("path")}")
        {
            Version = new(2, 0)
        }
            .AddHeaders(context)
            .AddContent(context)
            .SideEffect(m => m.Headers.Host = m.RequestUri?.Host)
            .SendAsync(context)
            .Select(m => CopyFromTargetResponseHeaders(m, context))
            .Select(m => m.SideEffect(m => context.Response.StatusCode = (int)m.StatusCode))
            .SelectMany(m => m.Content.CopyToAsync(context.Response.Body).ToNothing());

    static Task<HttpResponseMessage> SendAsync(this HttpRequestMessage msg, HttpContext context)
        => GetClient().SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

    static HttpMethod ToHttpMethod(this string method)
        => method switch
        {
            var m when HttpMethods.IsGet(m) => HttpMethod.Get,
            var m when HttpMethods.IsPost(m) => HttpMethod.Post,
            var m when HttpMethods.IsPut(m) => HttpMethod.Put,
            var m when HttpMethods.IsHead(m) => HttpMethod.Head,
            var m when HttpMethods.IsOptions(m) => HttpMethod.Options,
            var m when HttpMethods.IsDelete(m) => HttpMethod.Delete,
            var m when HttpMethods.IsTrace(m) => HttpMethod.Trace,
            _                                 => HttpMethod.Get
        };

    static HttpRequestMessage AddContent(this HttpRequestMessage msg, HttpContext context)
    {
        if (msg.Method == HttpMethod.Post
            || msg.Method == HttpMethod.Put
            || msg.Method == HttpMethod.Options)
        {
            msg.Content = new StreamContent(context.Request.Body);
            context
                .Request
                .Headers
                .ForEach(h => msg.Content.Headers.TryAddWithoutValidation(h.Key, h.Value.ToArray()));
        }
        return msg;
    }

    static HttpRequestMessage AddHeaders(this HttpRequestMessage msg, HttpContext context)
        => msg.SideEffect(_ => 
            context
                .Request
                .Headers
                .ForEach(h => msg.Headers.TryAddWithoutValidation(h.Key, h.Value.ToArray())));

    static readonly Func<HttpClient> GetClient = Memoize(InitGetClient);

    static HttpResponseMessage CopyFromTargetResponseHeaders(this HttpResponseMessage msg, HttpContext context)
        => msg
            .SideEffect(_ => 
                msg
                    .Headers
                    .ForEach(h => context.Response.Headers[h.Key] = h.Value.ToArray()))
            .SideEffect(_ => 
                msg
                    .Content
                    .Headers
                    .ForEach(h => context.Response.Headers[h.Key] = h.Value.ToArray()))
            .SideEffect(_ => context.Response.Headers.Remove("transfer-encoding"));

   static HttpClient InitGetClient()
        => new(new HttpClientHandler()
        {
            MaxConnectionsPerServer = 16,
        });
}