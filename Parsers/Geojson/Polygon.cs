namespace GeojsonParser;

using System.Text.Json.Serialization;

public class Polygon : Geometry
{
    [JsonPropertyName("coordinates")]
    public double[][][] Rings { get; set; }
}
