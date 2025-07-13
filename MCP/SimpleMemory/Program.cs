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
    private readonly Dictionary<string, object> _memory = new();
    
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
        
        return Task.FromResult(toolCall.Name switch
        {
            "store_memory" => HandleStoreMemory(request, toolCall),
            "get_memory" => HandleGetMemory(request, toolCall),
            "list_memory" => HandleListMemory(request),
            "delete_memory" => HandleDeleteMemory(request, toolCall),
            "clear_memory" => HandleClearMemory(request),
            _ => new McpResponse
            {
                Id = request.Id,
                Error = new McpError { Code = -32602, Message = "Unknown tool" }
            }
        });
    }
    
    private McpResponse HandleStoreMemory(McpRequest request, ToolCallParams toolCall)
    {
        try
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, object>>(toolCall.Arguments?.ToString() ?? "{}");
            if (args == null || !args.ContainsKey("key") || !args.ContainsKey("value"))
            {
                return new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError { Code = -32602, Message = "Missing required parameters: key and value" }
                };
            }
            
            var key = args["key"]?.ToString() ?? "";
            var value = args["value"]?.ToString() ?? "";
            
            if (string.IsNullOrEmpty(key))
            {
                return new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError { Code = -32602, Message = "Key cannot be empty" }
                };
            }
            
            _memory[key] = value;
            
            return new McpResponse
            {
                Id = request.Id,
                Result = new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = $"Stored value '{value}' under key '{key}'"
                        }
                    }
                }
            };
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
    
    private McpResponse HandleGetMemory(McpRequest request, ToolCallParams toolCall)
    {
        try
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, object>>(toolCall.Arguments?.ToString() ?? "{}");
            if (args == null || !args.ContainsKey("key"))
            {
                return new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError { Code = -32602, Message = "Missing required parameter: key" }
                };
            }
            
            var key = args["key"]?.ToString() ?? "";
            
            if (string.IsNullOrEmpty(key))
            {
                return new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError { Code = -32602, Message = "Key cannot be empty" }
                };
            }
            
            if (_memory.TryGetValue(key, out var value))
            {
                return new McpResponse
                {
                    Id = request.Id,
                    Result = new
                    {
                        content = new[]
                        {
                            new
                            {
                                type = "text",
                                text = $"Value for key '{key}': {value}"
                            }
                        }
                    }
                };
            }
            else
            {
                return new McpResponse
                {
                    Id = request.Id,
                    Result = new
                    {
                        content = new[]
                        {
                            new
                            {
                                type = "text",
                                text = $"No value found for key '{key}'"
                            }
                        }
                    }
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
    
    private McpResponse HandleListMemory(McpRequest request)
    {
        var keys = string.Join(", ", _memory.Keys);
        
        return new McpResponse
        {
            Id = request.Id,
            Result = new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = _memory.Count == 0 ? "No keys stored in memory" : $"Keys in memory: {keys}"
                    }
                }
            }
        };
    }
    
    private McpResponse HandleDeleteMemory(McpRequest request, ToolCallParams toolCall)
    {
        try
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, object>>(toolCall.Arguments?.ToString() ?? "{}");
            if (args == null || !args.ContainsKey("key"))
            {
                return new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError { Code = -32602, Message = "Missing required parameter: key" }
                };
            }
            
            var key = args["key"]?.ToString() ?? "";
            
            if (string.IsNullOrEmpty(key))
            {
                return new McpResponse
                {
                    Id = request.Id,
                    Error = new McpError { Code = -32602, Message = "Key cannot be empty" }
                };
            }
            
            if (_memory.Remove(key))
            {
                return new McpResponse
                {
                    Id = request.Id,
                    Result = new
                    {
                        content = new[]
                        {
                            new
                            {
                                type = "text",
                                text = $"Deleted key '{key}' from memory"
                            }
                        }
                    }
                };
            }
            else
            {
                return new McpResponse
                {
                    Id = request.Id,
                    Result = new
                    {
                        content = new[]
                        {
                            new
                            {
                                type = "text",
                                text = $"Key '{key}' not found in memory"
                            }
                        }
                    }
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
    
    private McpResponse HandleClearMemory(McpRequest request)
    {
        var count = _memory.Count;
        _memory.Clear();
        
        return new McpResponse
        {
            Id = request.Id,
            Result = new
            {
                content = new[]
                {
                    new
                    {
                        type = "text",
                        text = $"Cleared {count} items from memory"
                    }
                }
            }
        };
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
