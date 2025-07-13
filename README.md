# Simple Memory MCP Server

A Model Context Protocol (MCP) server implemented in C#/.NET that provides persistent memory capabilities for AI assistants.

## Setup Options for Windows VS Code + WSL

### Option 1: Run via WSL from Windows (Recommended)

Use this configuration in your Windows VS Code MCP settings:

```json
{
  "mcpServers": {
    "simple-memory": {
      "command": "wsl",
      "args": [
        "-e", 
        "/root/projects/simple-memory/SimpleMemory/bin/Release/net8.0/linux-x64/publish/SimpleMemory"
      ],
      "env": {}
    }
  }
}
```

### Option 2: Copy Windows Executable to Windows Filesystem

1. From WSL, run the copy script:
```bash
cd /root/projects/simple-memory
./copy-to-windows.sh
```

2. Use this configuration in Windows VS Code:
```json
{
  "mcpServers": {
    "simple-memory": {
      "command": "C:\\Users\\%USERNAME%\\simple-memory\\SimpleMemory.exe",
      "args": [],
      "env": {}
    }
  }
}
```

### Option 3: Development Mode (via WSL with dotnet run)

For development/debugging:

```json
{
  "mcpServers": {
    "simple-memory": {
      "command": "wsl",
      "args": [
        "-e", 
        "dotnet",
        "run",
        "--project",
        "/root/projects/simple-memory/SimpleMemory"
      ],
      "env": {}
    }
  }
}
```

## Features

This MCP server provides the following tools:

- **store_memory**: Store a value in memory with a given key
- **get_memory**: Retrieve a value from memory by key
- **list_memory**: List all keys stored in memory
- **delete_memory**: Delete a value from memory by key
- **clear_memory**: Clear all values from memory

## Prerequisites

- .NET 8.0 or later

## Building and Running

1. Build the project:
```bash
cd SimpleMemory
dotnet build
```

2. Run the server:
```bash
dotnet run
```

## Usage with MCP Inspector

You can test the server using the MCP Inspector:

1. Build the project first
2. Use the inspector with the built executable:
```bash
npx @modelcontextprotocol/inspector dotnet run --project SimpleMemory
```

## Tool Examples

### Store Memory
```json
{
  "name": "store_memory",
  "arguments": {
    "key": "user_name",
    "value": "John Doe"
  }
}
```

### Get Memory
```json
{
  "name": "get_memory",
  "arguments": {
    "key": "user_name"
  }
}
```

### List Memory
```json
{
  "name": "list_memory",
  "arguments": {}
}
```

### Delete Memory
```json
{
  "name": "delete_memory",
  "arguments": {
    "key": "user_name"
  }
}
```

### Clear Memory
```json
{
  "name": "clear_memory",
  "arguments": {}
}
```

## Architecture

The server implements the MCP protocol by:

1. Reading JSON-RPC messages from stdin
2. Processing MCP method calls (initialize, tools/list, tools/call)
3. Maintaining an in-memory dictionary for storage
4. Responding with appropriate JSON-RPC responses to stdout

The memory storage is ephemeral and will be lost when the server stops.
