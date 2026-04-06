using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using UTB.Minute.Contracts;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

[Collection("Cantine Collection")]
public class OrderTests(CantineTestFixture fixture)
{
    [Fact]
    public async Task PrintOrders_ReturnsOk_AndListOfOrders()
    {
        var response = await fixture.HttpClient.GetAsync("/orders", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        OrderDto[]? orderDtos = await response.Content.ReadFromJsonAsync<OrderDto[]>(TestContext.Current.CancellationToken);

        Assert.NotNull(orderDtos);
        Assert.True(orderDtos.Length == 1); 
        Assert.Contains(orderDtos, o => o.Status == OrderStatus.Preparing.ToString()); 
    }
    [Fact]
    public async Task CreateNewOrder_ReturnsNotFound_WhenMenuDoesNotExist()
    {
        OrderRequestDto orderRequest = new OrderRequestDto() { MenuId = 999999 };

        var response = await fixture.HttpClient.PostAsJsonAsync("/orders", orderRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateNewOrder_ReturnsBadRequest_WhenMenuIsSoldOut()
    {
        using var context = fixture.CreateContext();
        var soldOutMenu = context.MenuItems.First(m => m.Portions == 0);

        OrderRequestDto orderRequest = new OrderRequestDto() { MenuId = soldOutMenu.MenuId };

        var response = await fixture.HttpClient.PostAsJsonAsync("/orders", orderRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task CreateNewOrder_ReturnsCreated_AndPersistsOrder()
    {
        using var setupContext = fixture.CreateContext();
        var availableMenu = setupContext.MenuItems.First(m => m.Portions > 0);

        OrderRequestDto orderRequest = new OrderRequestDto() { MenuId = availableMenu.MenuId };

        var response = await fixture.HttpClient.PostAsJsonAsync("/orders", orderRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        OrderDto? orderDto = await response.Content.ReadFromJsonAsync<OrderDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(orderDto);
        Assert.Equal(orderRequest.MenuId, orderDto.MenuId);
        Assert.Equal(OrderStatus.Preparing.ToString(), orderDto.Status);

        using var verifyContext = fixture.CreateContext(); 
        var savedOrder = await verifyContext.Orders.FindAsync([orderDto.Id], TestContext.Current.CancellationToken);

        Assert.NotNull(savedOrder);
        Assert.Equal(orderRequest.MenuId, savedOrder.MenuId);
    }

    [Fact]
    public async Task UpdateOrderStatus_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        OrderStatusRequestDto statusRequest = new OrderStatusRequestDto() { Status = OrderStatus.Ready };

        var response = await fixture.HttpClient.PutAsJsonAsync("/orders/999999/status", statusRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task UpdateOrderStatus_ReturnsNoContent_AndUpdatesStatus()
    {
        using var setupContext = fixture.CreateContext();
        var realOrder = setupContext.Orders.First();


        OrderStatusRequestDto statusRequest = new OrderStatusRequestDto() { Status = OrderStatus.Ready };

        var response = await fixture.HttpClient.PutAsJsonAsync($"/orders/{realOrder.OrderId}/status", statusRequest, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var verifyContext = fixture.CreateContext();
        var updatedOrder = await verifyContext.Orders.FindAsync([realOrder.OrderId], TestContext.Current.CancellationToken);

        Assert.NotNull(updatedOrder);
        Assert.Equal(OrderStatus.Ready, updatedOrder.Status);
    }
}   