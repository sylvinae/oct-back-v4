namespace API.Models;

public class BulkFailure<T>
{
    public T Input { get; set; } = default!;
    public Dictionary<string, string> Errors { get; set; } = new();
}