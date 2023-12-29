namespace OrderHelperLib.Dtos;

public static class PageList
{
    public static PageList<T> Instance<T>(int pageIndex, int pageSize, int itemCount, List<T> list) where T : class =>
        new(pageIndex, pageSize, itemCount, list);
}
public record PageList<T> where T : class
{
    /// <summary>
    /// 当前页
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 当前页数据量
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPage { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public List<T> List { get; set; }
    public PageList() { }
    public PageList(int pageIndex, int pageSize, int itemCount, List<T> list)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        ItemCount = itemCount;
        TotalPage = Math.Max(1, (int)Math.Ceiling(itemCount / (double)pageSize));
        List = list;
    }
}