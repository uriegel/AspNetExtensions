using System.Reactive.Subjects;
using Microsoft.AspNetCore.Http;

using static AspNetExtensions.Core;

namespace AspNetExtensions;

public class Sse<TEvent>
{
    public static void SendEvent(TEvent evt) 
        => subject.OnNext(evt);

    public static async Task Start(HttpContext context)
    {
        context.Response.StatusCode = 200;
        context.Response.Headers.ContentType = "text/event-stream";
        context.Response.Headers.CacheControl = "no-cache";

        subject.Subscribe(async n =>
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(n, JsonWebDefaults);
            await context.Response.WriteAsync($"data:{payload}\n\n");
        });

        // Wait forever
        var tcs = new TaskCompletionSource();
        await tcs.Task;
    }

    static Subject<TEvent> subject = new();
}

