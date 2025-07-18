using Microsoft.AspNetCore.Mvc;
using SimpleMemoryApi.Services;

namespace SimpleMemoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MemoryController : ControllerBase
{
    private readonly MemoryService _memoryService;

    public MemoryController(MemoryService memoryService)
    {
        _memoryService = memoryService;
    }

    [HttpPost("store")]
    public IActionResult Store([FromBody] StoreRequest request)
    {
        if (!_memoryService.Store(request.Key, request.Value))
            return BadRequest("Key cannot be empty");
        return Ok(new { message = $"Stored value '{request.Value}' under key '{request.Key}'" });
    }

    [HttpGet("{key}")]
    public IActionResult Get(string key)
    {
        var (found, value) = _memoryService.Get(key);
        if (!found)
            return NotFound(new { message = $"No value found for key '{key}'" });
        return Ok(new { message = $"Value for key '{key}': {value}" });
    }

    [HttpGet("list")]
    public IActionResult List()
    {
        var keys = _memoryService.ListKeys();
        if (!keys.Any())
            return Ok(new { message = "No keys stored in memory" });
        return Ok(new { message = $"Keys in memory: {string.Join(", ", keys)}" });
    }

    [HttpDelete("{key}")]
    public IActionResult Delete(string key)
    {
        if (!_memoryService.Delete(key))
            return NotFound(new { message = $"Key '{key}' not found in memory" });
        return Ok(new { message = $"Deleted key '{key}' from memory" });
    }

    [HttpDelete("clear")]
    public IActionResult Clear()
    {
        var count = _memoryService.Clear();
        return Ok(new { message = $"Cleared {count} items from memory" });
    }
}

public class StoreRequest
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}
