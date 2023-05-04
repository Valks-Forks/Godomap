namespace GodotMap.Graphics;

public class MultiPolygon {
    public static Vector3[] Tesselate(GodotMap.Geometries.MultiPolygon multipolygon) {
        return multipolygon.Parts.SelectMany(el => GodotMap.Graphics.Polygon.Tesselate(el)).ToArray();
    }
}
