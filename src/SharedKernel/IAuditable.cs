namespace SharedKernel;
public interface IAuditable<T> : IAuditable
{
    T Id { get; set; }
}

public interface IAuditable
{
    DateTime? CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
    bool IsSoftDeleted { get; set; }
}
