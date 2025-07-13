#!/bin/bash

# Script to copy Windows executable to Windows filesystem
# Run this from WSL

WINDOWS_USER=$(cmd.exe /c "echo %USERNAME%" 2>/dev/null | tr -d '\r\n')
WINDOWS_PATH="/mnt/c/Users/$WINDOWS_USER/simple-memory"

echo "Creating Windows directory: $WINDOWS_PATH"
mkdir -p "$WINDOWS_PATH"

echo "Copying Windows executable..."
cp "./bin/Release/net8.0/win-x64/publish/SimpleMemory.exe" "$WINDOWS_PATH/"

echo "Creating Windows MCP config..."
cat > "$WINDOWS_PATH/mcp-config.json" << 'EOF'
{
  "mcpServers": {
    "simple-memory": {
      "command": "C:\\Users\\%USERNAME%\\simple-memory\\SimpleMemory.exe",
      "args": [],
      "env": {}
    }
  }
}
EOF

echo "Done! Files copied to: $WINDOWS_PATH"
echo "Windows executable: $WINDOWS_PATH/SimpleMemory.exe"
echo "Windows config: $WINDOWS_PATH/mcp-config.json"
