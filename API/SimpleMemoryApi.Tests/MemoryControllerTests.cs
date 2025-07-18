
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SimpleMemoryApi.Tests;

public class MemoryControllerTests : IClassFixture<WebApplicationFactory<SimpleMemoryApi.Program>>
{
    private readonly HttpClient _client;

    public MemoryControllerTests(WebApplicationFactory<SimpleMemoryApi.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Store_And_Get_Value_Works()
    {
        var storeReq = new { Key = "foo", Value = "bar" };
        var storeResp = await _client.PostAsJsonAsync("/api/memory/store", storeReq);
        Assert.Equal(HttpStatusCode.OK, storeResp.StatusCode);

        var getResp = await _client.GetAsync("/api/memory/foo");
        Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
        var getContent = await getResp.Content.ReadFromJsonAsync<dynamic>();
        Assert.Contains("bar", getContent.GetProperty("message").GetString());
    }

    [Fact]
    public async Task List_Keys_Returns_Stored_Keys()
    {
        await _client.PostAsJsonAsync("/api/memory/store", new { Key = "a", Value = "1" });
        await _client.PostAsJsonAsync("/api/memory/store", new { Key = "b", Value = "2" });
        var resp = await _client.GetAsync("/api/memory/list");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var content = await resp.Content.ReadFromJsonAsync<dynamic>();
        Assert.Contains("a", content.GetProperty("message").GetString());
        Assert.Contains("b", content.GetProperty("message").GetString());
    }

    [Fact]
    public async Task Delete_Key_Removes_It()
    {
        await _client.PostAsJsonAsync("/api/memory/store", new { Key = "del", Value = "gone" });
        var delResp = await _client.DeleteAsync("/api/memory/del");
        Assert.Equal(HttpStatusCode.OK, delResp.StatusCode);
        var getResp = await _client.GetAsync("/api/memory/del");
        Assert.Equal(HttpStatusCode.NotFound, getResp.StatusCode);
    }

    [Fact]
    public async Task Clear_Removes_All_Keys()
    {
        await _client.PostAsJsonAsync("/api/memory/store", new { Key = "x", Value = "1" });
        await _client.PostAsJsonAsync("/api/memory/store", new { Key = "y", Value = "2" });
        var clearResp = await _client.DeleteAsync("/api/memory/clear");
        Assert.Equal(HttpStatusCode.OK, clearResp.StatusCode);
        var listResp = await _client.GetAsync("/api/memory/list");
        var content = await listResp.Content.ReadFromJsonAsync<dynamic>();
        Assert.Contains("No keys stored in memory", content.GetProperty("message").GetString());
    }
}
