using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetExtensions;

public static partial class Core
{
    public static JsonSerializerOptions JsonWebDefaults {get; }

    static Core()
        => JsonWebDefaults = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
}