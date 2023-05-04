namespace GodotMap.Geometries;

public class Polygon: IGeometry {

    public PolygonRing OuterRing;
    public PolygonRing[] InnerRings;

    public Polygon(double[][][] el) {
        this.OuterRing = new PolygonRing(el[0]);
        this.InnerRings = el.Skip(1).Select(innerRing => new PolygonRing(innerRing)).ToArray();
    }

    public void InitFromGeojson(string path) => throw new NotImplementedException();

}

public class PolygonRing {
    public Vector2[] Vertices { get; }

    public PolygonRing(double[][] coordinates) {
        this.Vertices = coordinates.Select(el => new Vector2((float) el[0], (float) el[1])).ToArray();
    }
}
