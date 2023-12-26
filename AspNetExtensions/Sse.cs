using System.Reactive.Subjects;
using Microsoft.AspNetCore.Http;

using static AspNetExtensions.Core;

namespace AspNetExtensions;

public class Sse<TEvent>(IObservable<TEvent> onNext)
{
    public async Task Start(HttpContext context)
    {
        context.Response.StatusCode = 200;
        context.Response.Headers.ContentType = "text/event-stream";
        context.Response.Headers.CacheControl = "no-cache";

        onNext.Subscribe(n => {
            lock (locker)
            {
                context
                    .Response
                    .WriteAsync($"data:{System.Text.Json.JsonSerializer.Serialize(n, JsonWebDefaults)}\n\n")
                    .Wait();
            }
        });

        // Wait forever
        var tcs = new TaskCompletionSource();
        await tcs.Task;
    }

    readonly object locker = new();
    readonly IObservable<TEvent> onNext = onNext;
}

 