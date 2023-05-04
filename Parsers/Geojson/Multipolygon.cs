namespace GeojsonParser;

using System.Text.Json.Serialization;

public class MultiPolygon : Geometry
{
    [JsonPropertyName("coordinates")]
    public double[][][][] Parts { get; set; }
}
