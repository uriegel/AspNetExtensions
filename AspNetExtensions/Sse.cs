using System.Reactive.Subjects;
using Microsoft.AspNetCore.Http;

using static AspNetExtensions.Core;

namespace AspNetExtensions;

public class Sse<TEvent>
{
    public Sse(IObservable<TEvent> onNext)
        => this.onNext = onNext;

    public async Task Start(HttpContext context)
    {
        context.Response.StatusCode = 200;
        context.Response.Headers.ContentType = "text/event-stream";
        context.Response.Headers.CacheControl = "no-cache";

        onNext.Subscribe(async n =>
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(n, JsonWebDefaults);
            await context.Response.WriteAsync($"data:{payload}\n\n");
        });

        // Wait forever
        var tcs = new TaskCompletionSource();
        await tcs.Task;
    }

    IObservable<TEvent> onNext;
}

 