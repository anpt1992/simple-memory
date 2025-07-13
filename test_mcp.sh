#!/bin/bash

cd SimpleMemory

echo "Testing MCP Server..."
echo

# Test initialize
echo "1. Testing initialize:"
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{}}' | timeout 5s dotnet run
echo

# Test tools/list
echo "2. Testing tools/list:"
echo '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}' | timeout 5s dotnet run
echo

echo "Tests completed."
