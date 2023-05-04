namespace GeojsonParser;

using System.Text.Json;
using System.Text.Json.Serialization;

public class Geojson
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class Feature : Geojson
{
    [JsonPropertyName("geometry")]
    public Geometry Geometry { get; set; }
}

public class Geometry
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class Feature<TGeometry> : Geojson where TGeometry : Geometry
{
    [JsonPropertyName("geometry")]
    public TGeometry Geometry { get; set; }
}

public class FeatureCollection<TGeometry> : Geojson where TGeometry : Geometry
{
    [JsonPropertyName("features")]
    public Feature<TGeometry>[] Features { get; set; }

    public static FeatureCollection<TGeometry> CreateFrom(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string text = file.GetAsText();
        return JsonSerializer.Deserialize<GeojsonParser.FeatureCollection<TGeometry>>(text);
    }
}

public class FeatureCollection : Geojson
{
    [JsonPropertyName("features")]
    public Feature[] Features { get; set; }
}
