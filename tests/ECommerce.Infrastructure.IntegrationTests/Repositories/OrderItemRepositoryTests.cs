namespace ECommerce.Infrastructure.IntegrationTests.Repositories;
using ECommerce.Domain.ValueObjects;

public class OrderItemRepositoryTests : RepositoryTestBase
{
    private readonly OrderItemRepository _repository;

    public OrderItemRepositoryTests()
    {
        _repository = new OrderItemRepository(Context);
    }

    [Fact]
    public async Task GetOrderItemsAsync_ReturnsItemsWithProduct()
    {
        var category = Category.Create("Books");
        category.Id = Guid.NewGuid();
        var product = Product.Create("Book", null, 5m, category.Id, 10);
        var order = Order.Create(Guid.NewGuid(), new Address("s", "Istanbul", "34000", "Turkey"), new Address("b", "Istanbul", "34000", "Turkey"));

        Context.Categories.Add(category);
        Context.Products.Add(product);
        Context.Orders.Add(order);
        await Context.SaveChangesAsync();

        order.AddItem(product.Id, Price.Create(5m), 2);
        Context.Orders.Update(order);
        await Context.SaveChangesAsync();

        var result = await _repository.GetOrderItemsAsync(order.Id);

        result.Should().HaveCount(1);
        result.First().Product.Should().NotBeNull();
    }
}
