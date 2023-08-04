namespace OrderHelperLib.Dtos;

/// <summary>
/// 数据模型
/// </summary>
/// <typeparam name="T"></typeparam>
public record Dto<T> where T : IConvertible
{
    public T Id { get; set; }
}

public record IntDto : Dto<int>
{
}