
using ModelContextProtocol.AspNetCore;
using Tools;

var builder = WebApplication.CreateBuilder(args);

// Register MCP server and discover tools from the current assembly
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Add MCP middleware
app.MapMcp();

app.MapGet("/", () => "Hello World!");

app.Run();
