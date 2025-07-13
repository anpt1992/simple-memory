#!/bin/bash

# Interactive test script for the Simple Memory MCP Server

echo "Testing Simple Memory MCP Server..."
echo "=================================="

cd SimpleMemory

# Create a test input file with multiple commands
cat > test_input.json << 'EOF'
{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0.0"}}}
{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}
{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"store_memory","arguments":{"key":"test_key","value":"test_value"}}}
{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"get_memory","arguments":{"key":"test_key"}}}
{"jsonrpc":"2.0","id":5,"method":"tools/call","params":{"name":"list_memory","arguments":{}}}
EOF

echo "Running MCP server with test commands..."
timeout 10s dotnet run < test_input.json

echo -e "\n\nTest completed!"

# Clean up
rm -f test_input.json
