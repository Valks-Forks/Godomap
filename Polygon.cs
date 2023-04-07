using Godot;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace GeojsonParser {
	public class Geojson {
		[JsonPropertyName("type")]
		public string Type { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }
	}

	public class PolygonCoordinates {
		public List<List<List<double>>> Rings;
	}

	public class PolygonRing {

		public Vector2[] Vertices { get; }

		public PolygonRing(double[][] coordinates) {
			this.Vertices = coordinates.Select(el => new Vector2((float) el[0], (float) el[1])).ToArray();
		}
	}

	public class Polygon {

		public PolygonRing OuterRing;
		public PolygonRing[] InnerRings;

		public Polygon(double[][][] el) {
			this.OuterRing = new PolygonRing(el[0]);
			this.InnerRings = el.Skip(1).Select(innerRing => new PolygonRing(innerRing)).ToArray();
		}
	}

	public class MultiPolygon: Geometry {
		[JsonPropertyName("coordinates")]
		public double[][][][] JsonParts { get; set; }

		public Polygon[] Parts {
			get {
				return this.JsonParts.Select(el => new Polygon(el)).ToArray();
			}
		}
	}

	public class Geometry {
		[JsonPropertyName("type")]
		public string Type { get; set; }
	}

	public class Feature: Geojson {
		[JsonPropertyName("geometry")]
		public Geometry Geometry { get; set; }
	}

	public class Feature<TGeometry>: Geojson where TGeometry: Geometry {
		[JsonPropertyName("geometry")]
		public TGeometry Geometry { get; set; }
	}

	public class FeatureCollection<TGeometry>: Geojson where TGeometry: Geometry {
		[JsonPropertyName("features")]
		public Feature<TGeometry>[] Features { get; set; }
	}

	public class FeatureCollection: Geojson {
		[JsonPropertyName("features")]
		public List<Feature> Features { get; set; }
	}

	public class GeojsonConverter: JsonConverter<Geojson> {
		public override Geojson Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var jsonObject = JsonSerializer.Deserialize<JsonElement>(ref reader);
			var geojsonType = jsonObject.GetProperty("type").GetString();
			switch (geojsonType) {
				case "FeatureCollection":
					return JsonSerializer.Deserialize<FeatureCollection>(jsonObject.GetRawText(), options);
				default:
					throw new NotImplementedException(String.Format("Geojson with type {0} is not implemented.", geojsonType));
			}
		}

		public override void Write(Utf8JsonWriter writer, Geojson value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}

	public class GeometryConverter: JsonConverter<Geometry> {
		public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var jsonObject = JsonSerializer.Deserialize<JsonElement>(ref reader);
			var geometryType = jsonObject.GetProperty("type").GetString();
			switch (geometryType) {
				case "MultiPolygon":
					return JsonSerializer.Deserialize<MultiPolygon>(jsonObject.GetRawText(), options);
				default:
					throw new NotImplementedException(String.Format("Geometry with type {0} is not implemented.", geometryType));
			}
		}

		public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options)
		{
			throw new NotImplementedException();
		}
	}
}

public partial class Polygon : MeshInstance3D
{

	public Vector3[] Tesselate(GeojsonParser.Polygon polygon) {
		var res = new List<Vector3>();
		var innerRings = polygon.InnerRings;
		var nbInnerRings = innerRings.Count();

		var vertices = new List<double>();
		var innerRingIndices = new int[nbInnerRings];
		var outerRingVertices = polygon.OuterRing.Vertices;
		var nbOuterRingVertices = outerRingVertices.Count();
		for (var j = 0; j < nbOuterRingVertices; j++) {
			var outerRingVertex = outerRingVertices[j];
			vertices.Add(outerRingVertex.X);
			vertices.Add(outerRingVertex.Y);
		}
		var innerRingCurrentIndex = nbOuterRingVertices;
		for (var j = 0; j < innerRings.Count(); j++) {
			innerRingIndices[j] = innerRingCurrentIndex;
			var innerRingVertices = innerRings[j].Vertices;
			var nbInnerRingVertices = innerRingVertices.Count();
			for (var k = 0; k < nbInnerRingVertices; k++) {
				var innerRingVertex = innerRingVertices[k];
				vertices.Add(innerRingVertex.X);
				vertices.Add(innerRingVertex.Y);
			}
			innerRingCurrentIndex += nbInnerRingVertices;
		}

		var connectivity = EarcutNet.Earcut.Tessellate(vertices, innerRingIndices);
		for (var i = 0; i < connectivity.Count(); i++) {
			res.Add(new Vector3((float) vertices[(connectivity[i]*2)+1], 0, (float) vertices[connectivity[i]*2]));
		}
		return res.ToArray();
	}

	public Vector3[] Tesselate(GeojsonParser.MultiPolygon multipolygon) {
		return multipolygon.Parts.SelectMany(el => Tesselate(el)).ToArray();
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		using var file = FileAccess.Open("res://Data/simple.geojson", FileAccess.ModeFlags.Read);
		string text = file.GetAsText();
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			Converters = { new GeojsonParser.GeojsonConverter(), new GeojsonParser.GeometryConverter() }
		};
		var deserializedMultipolygon = JsonSerializer.Deserialize<GeojsonParser.FeatureCollection<GeojsonParser.MultiPolygon>>(text, options);

		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		foreach (var feature in deserializedMultipolygon.Features) {
			var geom = feature.Geometry;
			var tesselation = Tesselate(geom);
			vertices.AddRange(tesselation);
			normals.AddRange(tesselation.Select(el => new Vector3(0, 1, 0)));
		}

		// Create a new ArrayMesh and add the triangle surface to it
		var mesh = new ArrayMesh();

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) ArrayMesh.ArrayType.Max);
		arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices.ToArray();
		arrays[(int)ArrayMesh.ArrayType.Normal] = normals.ToArray();

		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		GD.Print(mesh.GetAabb());
		
		// Set the mesh of this MeshInstance to the triangle mesh
		this.Mesh = mesh;
	}

	public void Deserialize() {

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
