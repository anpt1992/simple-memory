@echo off
echo Testing MCP server from Windows via WSL...
echo.

echo Testing initialize:
echo {"jsonrpc":"2.0","id":1,"method":"initialize","params":{}} | wsl -e /root/projects/simple-memory/SimpleMemory/bin/Release/net8.0/linux-x64/publish/SimpleMemory

echo.
echo If you see a JSON response above, your MCP server is accessible from Windows!
pause
