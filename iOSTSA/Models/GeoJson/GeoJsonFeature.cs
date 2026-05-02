using System.Text.Json.Serialization;

namespace iOSTSA.Models.GeoJson;

internal sealed class GeoJsonFeature
{
    [JsonPropertyName("properties")]
    public IntersectionProperties Properties { get; set; } = new();

    [JsonPropertyName("geometry")]
    public GeoJsonGeometry? Geometry { get; set; }
}
