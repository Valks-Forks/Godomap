namespace GodotMap.Geometries;

public partial class MultiPolygon : MeshInstance3D
{
    public Polygon[] Parts { get; set; }

    public MultiPolygon(GeojsonParser.MultiPolygon geometry)
    {
        this.Parts = geometry.Parts.Select(el => new GodotMap.Geometries.Polygon(el)).ToArray();
    }
}
