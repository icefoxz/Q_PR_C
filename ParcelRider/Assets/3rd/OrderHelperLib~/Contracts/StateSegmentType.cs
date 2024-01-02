namespace OrderHelperLib.Contracts;


public static class StateSegments
{
    public const string None = "None";
    public const string Remark = "Remark";
    public const string Images = "Images";

    public enum Type
    {
        None,
        Remark,
        Images,
    }

}

public static class StateSegmentTypeExtension
{
    public static string Text(this StateSegments.Type type) => type switch
    {
        StateSegments.Type.Remark => StateSegments.Remark,
        StateSegments.Type.Images => StateSegments.Images,
        _ => StateSegments.None,
    };

    public static StateSegments.Type ToStateSegment(this string type) => type switch
    {
        StateSegments.Remark => StateSegments.Type.Remark,
        StateSegments.Images => StateSegments.Type.Images,
        _ => StateSegments.Type.None,
    };
}