using System.Linq;
using System.Text.Json.Serialization;

namespace GeojsonParser {
	public class MultiPolygon: Geometry {

		[JsonPropertyName("coordinates")]
		public double[][][][] Parts { get; set; }
	}
}
