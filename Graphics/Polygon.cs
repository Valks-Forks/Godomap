using System.Collections.Generic;

namespace GodotMap.Graphics;

public partial class Polygon : MeshInstance3D
{
	public static Vector3[] Tesselate(GodotMap.Geometries.Polygon polygon) {
		var res = new List<Vector3>();
		var innerRings = polygon.InnerRings;
		var nbInnerRings = innerRings.Length;

		var vertices = new List<double>();
		var innerRingIndices = new int[nbInnerRings];
		var outerRingVertices = polygon.OuterRing.Vertices;
		var nbOuterRingVertices = outerRingVertices.Length;
		for (var j = 0; j < nbOuterRingVertices; j++) {
			var outerRingVertex = outerRingVertices[j];
			vertices.Add(outerRingVertex.X);
			vertices.Add(outerRingVertex.Y);
		}
		var innerRingCurrentIndex = nbOuterRingVertices;
		for (var j = 0; j < innerRings.Length; j++) {
			innerRingIndices[j] = innerRingCurrentIndex;
			var innerRingVertices = innerRings[j].Vertices;
			var nbInnerRingVertices = innerRingVertices.Length;
			for (var k = 0; k < nbInnerRingVertices; k++) {
				var innerRingVertex = innerRingVertices[k];
				vertices.Add(innerRingVertex.X);
				vertices.Add(innerRingVertex.Y);
			}
			innerRingCurrentIndex += nbInnerRingVertices;
		}

		var connectivity = EarcutNet.Earcut.Tessellate(vertices, innerRingIndices);
		for (var i = 0; i < connectivity.Count; i++) {
			res.Insert(0, new Vector3((float) vertices[(connectivity[i]*2+1)], 0, (float) vertices[connectivity[i]*2]));
		}
		return res.ToArray();
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var multipolygon = GeojsonParser.FeatureCollection<GeojsonParser.MultiPolygon>.CreateFrom("res://Data/france.geojson");

		var vertices = new List<Vector3>();
		var normals = new List<Vector3>();
		foreach (var feature in multipolygon.Features) {
			var geom = feature.Geometry;
			var tesselation = MultiPolygon.Tesselate(new GodotMap.Geometries.MultiPolygon(geom));
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
		// GD.Print(mesh.GetAabb());
			
		// Set the mesh of this MeshInstance to the triangle mesh
		this.Mesh = mesh;
	}
}
