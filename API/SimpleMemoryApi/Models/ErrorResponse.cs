namespace SimpleMemoryApi.Models;

public class ErrorResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = "";
    public object? Data { get; set; }
}
