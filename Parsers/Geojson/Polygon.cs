using System.Text.Json.Serialization;

namespace GeojsonParser;

public class Polygon: Geometry {

	[JsonPropertyName("coordinates")]
	public double[][][] Rings { get; set; }
}
