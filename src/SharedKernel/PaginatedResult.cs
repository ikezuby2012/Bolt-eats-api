namespace SharedKernel;

public sealed class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; }
    public int? TotalItems { get; set; }
    public int? PageSize { get; set; }
    public int? PageNumber { get; set; }
}
