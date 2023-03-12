using System.Reactive.Subjects;

namespace AspNetExtensions;

public class SseEventSource<TEvent>
{
    public static SseEventSource<TEvent> Create()
        => new ();
    public void Send(TEvent evt) => Subject.OnNext(evt);

    public Subject<TEvent> Subject { get; private set; } = new();
}