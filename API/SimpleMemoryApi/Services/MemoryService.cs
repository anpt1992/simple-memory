using System.Collections.Concurrent;

namespace SimpleMemoryApi.Services;

public class MemoryService
{
    private readonly ConcurrentDictionary<string, object> _memory = new();

    public bool Store(string key, object value)
    {
        if (string.IsNullOrEmpty(key)) return false;
        _memory[key] = value;
        return true;
    }

    public (bool found, object? value) Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return (false, null);
        return _memory.TryGetValue(key, out var value) ? (true, value) : (false, null);
    }

    public IEnumerable<string> ListKeys() => _memory.Keys;

    public bool Delete(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;
        return _memory.Remove(key, out _);
    }

    public int Clear()
    {
        var count = _memory.Count;
        _memory.Clear();
        return count;
    }
}
