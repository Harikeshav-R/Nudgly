using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nudgly.Shared.Models;

public sealed record OpenRouterRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] List<OpenRouterMessage> Messages
);

public sealed record OpenRouterMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] List<OpenRouterContentPart> Content
);

public sealed record OpenRouterContentPart(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Text = null,
    [property: JsonPropertyName("image_url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] OpenRouterImageUrl? ImageUrl = null
);

public sealed record OpenRouterImageUrl(
    [property: JsonPropertyName("url")] string Url
);

public sealed record OpenRouterResponse(
    [property: JsonPropertyName("choices")] List<OpenRouterChoice>? Choices
);

public sealed record OpenRouterChoice(
    [property: JsonPropertyName("message")] OpenRouterResponseMessage? Message
);

public sealed record OpenRouterResponseMessage(
    [property: JsonPropertyName("content")] string? Content
);

[JsonSerializable(typeof(OpenRouterRequest))]
[JsonSerializable(typeof(OpenRouterResponse))]
public partial class OpenRouterJsonContext : JsonSerializerContext
{
}
