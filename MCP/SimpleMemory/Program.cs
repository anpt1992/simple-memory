using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleMemory;

class Program
{
    private static readonly Dictionary<string, object> _memory = new();
    
    static async Task Main(string[] args)
    {
        var server = new McpServer();
        await server.RunAsync();
    }
}

public class McpServer
{
    private readonly HttpClient _httpClient = new();
    private readonly string _apiBaseUrl = "http://localhost:5000/api/memory";
    
    public async Task RunAsync()
    {
        // Read from stdin and write to stdout for MCP protocol
        using var reader = new StreamReader(Console.OpenStandardInput());
        using var writer = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
        
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line == null) break;
            
            try
            {
                var request = JsonSerializer.Deserialize<McpRequest>(line);
                if (request != null)
                {
                    var response = await HandleRequestAsync(request);
                    var responseJson = JsonSerializer.Serialize(response);
                    await writer.WriteLineAsync(responseJson);
                }
            }
            catch (Exception ex)
            {
                var errorResponse = new McpResponse
                {
                    Id = null,
                    Error = new McpError { Code = -1, Message = ex.Message }
                };
                var errorJson = JsonSerializer.Serialize(errorResponse);
                await writer.WriteLineAsync(errorJson);
            }
        }
    }
    
    private async Task<McpResponse> HandleRequestAsync(McpRequest request)
    {
        return request.Method switch
        {
            "initialize" => HandleInitialize(request),
            "tools/list" => HandleToolsList(request),
            "tools/call" => await HandleToolCallAsync(request),
            _ => new McpResponse
            {
                Id = request.Id,
                Error = new McpError { Code = -32601, Message = "Method not found" }
            }
        };
    }
    
    private McpResponse HandleInitialize(McpRequest request)
    {
        return new McpResponse
        {
            Id = request.Id,
            Result = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = new { }
                },
                serverInfo = new
                {
                    name = "simple-memory",
                    version = "0.1.0"
                }
            }
        };
    }
    
    private McpResponse HandleToolsList(McpRequest request)
    {
        var tools = new object[]
        {
            new
            {
                name = "store_memory",
                description = "Store a value in memory with a given key",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        key = new { type = "string", description = "The key to store the value under" },
                        value = new { type = "string", description = "The value to store" }
                    },
                    required = new[] { "key", "value" }
                }
            },
            new
            {
                name = "get_memory",
                description = "Retrieve a value from memory by key",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        key = new { type = "string", description = "The key to retrieve the value for" }
                    },
                    required = new[] { "key" }
                }
            },
            new
            {
                name = "list_memory",
                description = "List all keys stored in memory",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            },
            new
            {
                name = "delete_memory",
                description = "Delete a value from memory by key",
                inputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        key = new { type = "string", description = "The key to delete from memory" }
                    },
                    required = new[] { "key" }
                }
            },
            new
            {
                name = "clear_memory",
                description = "Clear all values from memory",
                inputSchema = new
                {
                    type = "object",
                    properties = new { }
                }
            }
        };
        
        return new McpResponse
        {
            Id = request.Id,
            Result = new { tools }
        };
    }
    
    private Task<McpResponse> HandleToolCallAsync(McpRequest request)
    {
        var toolCall = JsonSerializer.Deserialize<ToolCallParams>(request.Params?.ToString() ?? "{}");
        if (toolCall == null)
        {
            return Task.FromResult(new McpResponse
            {
                Id = request.Id,
                Error = new McpError { Code = -32602, Message = "Invalid tool call parameters" }
            });
        }
        
        return ForwardToolCallAsync(request, toolCall);
    }
    
    private async Task<McpResponse> ForwardToolCallAsync(McpRequest request, ToolCallParams toolCall)
    {
        try
        {
            switch (toolCall.Name)
            {
                case "store_memory":
                    var storeArgs = JsonSerializer.Deserialize<Dictionary<string, object>>(toolCall.Arguments?.ToString() ?? "{}");
                    var storeResp = await _httpClient.PostAsync(
                        _apiBaseUrl + "/store",
                        new StringContent(JsonSerializer.Serialize(new { Key = storeArgs?["key"], Value = storeArgs?["value"] }), System.Text.Encoding.UTF8, "application/json")
                    );
                    var storeContent = await storeResp.Content.ReadAsStringAsync();
                    return new McpResponse { Id = request.Id, Result = JsonSerializer.Deserialize<object>(storeContent) };

                case "get_memory":
                    var getArgs = JsonSerializer.Deserialize<Dictionary<string, object>>(toolCall.Arguments?.ToString() ?? "{}");
                    var getResp = await _httpClient.GetAsync(_apiBaseUrl + $"/{getArgs?["key"]}");
                    var getContent = await getResp.Content.ReadAsStringAsync();
                    return new McpResponse { Id = request.Id, Result = JsonSerializer.Deserialize<object>(getContent) };

                case "list_memory":
                    var listResp = await _httpClient.GetAsync(_apiBaseUrl + "/list");
                    var listContent = await listResp.Content.ReadAsStringAsync();
                    return new McpResponse { Id = request.Id, Result = JsonSerializer.Deserialize<object>(listContent) };

                case "delete_memory":
                    var delArgs = JsonSerializer.Deserialize<Dictionary<string, object>>(toolCall.Arguments?.ToString() ?? "{}");
                    var delResp = await _httpClient.DeleteAsync(_apiBaseUrl + $"/{delArgs?["key"]}");
                    var delContent = await delResp.Content.ReadAsStringAsync();
                    return new McpResponse { Id = request.Id, Result = JsonSerializer.Deserialize<object>(delContent) };

                case "clear_memory":
                    var clearResp = await _httpClient.DeleteAsync(_apiBaseUrl + "/clear");
                    var clearContent = await clearResp.Content.ReadAsStringAsync();
                    return new McpResponse { Id = request.Id, Result = JsonSerializer.Deserialize<object>(clearContent) };

                default:
                    return new McpResponse
                    {
                        Id = request.Id,
                        Error = new McpError { Code = -32602, Message = "Unknown tool" }
                    };
            }
        }
        catch (Exception ex)
        {
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError { Code = -32603, Message = $"Internal error: {ex.Message}" }
            };
        }
    }
    }
    

public class McpRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
    
    [JsonPropertyName("id")]
    public object? Id { get; set; }
    
    [JsonPropertyName("method")]
    public string Method { get; set; } = "";
    
    [JsonPropertyName("params")]
    public object? Params { get; set; }
}

public class McpResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";
    
    [JsonPropertyName("id")]
    public object? Id { get; set; }
    
    [JsonPropertyName("result")]
    public object? Result { get; set; }
    
    [JsonPropertyName("error")]
    public McpError? Error { get; set; }
}

public class McpError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
    
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

public class ToolCallParams
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("arguments")]
    public object? Arguments { get; set; }
}
