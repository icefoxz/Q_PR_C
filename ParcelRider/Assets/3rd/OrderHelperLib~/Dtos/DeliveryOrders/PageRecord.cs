namespace OrderHelperLib.Dtos.DeliveryOrders;

public record PageRecord<T>
{
    public PageRecord(int current_PageIndex, int page_Size)
    {
        this.current_PageIndex = current_PageIndex;
        this.page_Size = page_Size;
    }

    public PageRecord()
    {
        
    }

    public int current_PageIndex { get; set; }

    /// <summary>
    /// 当前页数据量
    /// </summary>
    public int page_Size { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int totalPages { get; set; }

    /// <summary>
    /// 总数量
    /// </summary>
    public int total { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public List<T> CurrentPageData { get; set; }
}