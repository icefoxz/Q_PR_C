namespace OrderHelperLib.Dtos;

/// <summary>
/// 数据模型
/// </summary>
/// <typeparam name="T"></typeparam>
public record Dto<T> where T : IConvertible
{
    public T Id { get; set; }
    public int Version { get; set; }

    public Dto()
    {
        
    }
}

public record LongDto : Dto<long>
{
}

public record IntDto : Dto<int>
{
}

public record StringDto : Dto<string>
{
}
