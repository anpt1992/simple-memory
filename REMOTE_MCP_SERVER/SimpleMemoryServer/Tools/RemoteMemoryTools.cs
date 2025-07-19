using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace Tools
{
    [McpServerToolType]
    public sealed class RemoteMemoryTools
    {
        private static readonly ConcurrentDictionary<string, string> _memory = new();

        [McpServerTool, Description("Store a value under a key")]
        public static string Store(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return "Key cannot be empty";
            _memory[key] = value;
            return $"Stored value '{value}' under key '{key}'";
        }

        [McpServerTool, Description("Get a value by key")]
        public static string Get(string key)
        {
            if (_memory.TryGetValue(key, out var value))
                return $"Value for key '{key}': {value}";
            return $"No value found for key '{key}'";
        }

        [McpServerTool, Description("List all keys")]
        public static string List()
        {
            if (_memory.IsEmpty)
                return "No keys stored in memory";
            return $"Keys in memory: {string.Join(", ", _memory.Keys)}";
        }

        [McpServerTool, Description("Delete a key")]
        public static string Delete(string key)
        {
            if (_memory.TryRemove(key, out _))
                return $"Deleted key '{key}' from memory";
            return $"Key '{key}' not found in memory";
        }

        [McpServerTool, Description("Clear all memory")]
        public static string Clear()
        {
            int count = _memory.Count;
            _memory.Clear();
            return $"Cleared {count} items from memory";
        }
    }
}