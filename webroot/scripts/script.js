console.log("script loaded")

const source = new EventSource("http://localhost:19999/sse/test")

source.onmessage = (event) => console.log("SSE event", event.data)
