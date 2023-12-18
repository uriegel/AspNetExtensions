namespace AspNetExtensions;

public record RequestError(
    int Status,
    string StatusText
);