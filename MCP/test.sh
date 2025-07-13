#!/bin/bash

# Test script for the Simple Memory MCP Server

echo "Testing Simple Memory MCP Server..."
echo "=================================="

cd SimpleMemory

# Test 1: Initialize
echo "Test 1: Initialize"
echo '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test","version":"1.0.0"}}}' | dotnet run
echo ""

# Test 2: List tools
echo "Test 2: List tools"
echo '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}' | dotnet run
echo ""

# Test 3: Store memory
echo "Test 3: Store memory"
echo '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"store_memory","arguments":{"key":"test_key","value":"test_value"}}}' | dotnet run
echo ""

# Test 4: Get memory
echo "Test 4: Get memory"
echo '{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"get_memory","arguments":{"key":"test_key"}}}' | dotnet run
echo ""

# Test 5: List memory
echo "Test 5: List memory"
echo '{"jsonrpc":"2.0","id":5,"method":"tools/call","params":{"name":"list_memory","arguments":{}}}' | dotnet run
echo ""

echo "Testing complete!"
