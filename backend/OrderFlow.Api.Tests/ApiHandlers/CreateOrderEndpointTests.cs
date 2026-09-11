using OrderFlow.Application.Orders.Command;

namespace OrderFlow.Api.Tests.ApiHandlers;

public class CreateOrderEndpointTests
{
    [Fact]
    public async Task PostOrders_WithInvalidPayload_ReturnsBadRequestEndToEnd()
    {
        await using var app = new ApiApp();
        using var client = app.CreateClient();

        var response = await client.PostAsJsonAsync("/orders", new CreateOrderCommand(string.Empty, "SKU-001", 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("errors", body);
    }
}
