using System.Text.Json.Serialization;
using Toolbox.Tools;

namespace Toolbox.Types;

[JsonRegister(typeof(DataETag))]
[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    Converters = new[] { typeof(ImmutableByteArrayConverter) })
    ]
[JsonSerializable(typeof(DataETag))]
internal partial class DataETagJsonContext : JsonSerializerContext
{
}
