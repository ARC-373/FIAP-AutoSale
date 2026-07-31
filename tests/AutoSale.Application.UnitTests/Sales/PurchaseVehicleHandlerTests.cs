using AutoSale.Application.Common;
using AutoSale.Application.Sales.Purchase;
using AutoSale.Domain.Vehicles;

namespace AutoSale.Application.UnitTests.Sales;

public sealed class PurchaseVehicleHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenUserIsUnauthenticated_ReturnsUnauthorizedWithoutOpeningTransaction()
    {
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(new TestVehicleRepository(), new TestSaleRepository(), unitOfWork, new TestCurrentUser(null));

        var result = await handler.HandleAsync(new PurchaseVehicleCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.Unauthenticated, result.Error);
        Assert.Equal(0, unitOfWork.BeginTransactionCalls);
    }

    [Fact]
    public async Task Handle_WhenVehicleIsAvailable_CreatesSaleAndCommitsTransaction()
    {
        var vehicle = Vehicle.Create("Toyota", "Corolla", 2025, "White", 120_000m, Now).Value!;
        var vehicles = new TestVehicleRepository { Vehicle = vehicle };
        var sales = new TestSaleRepository();
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(vehicles, sales, unitOfWork, new TestCurrentUser("buyer-subject"));

        var result = await handler.HandleAsync(new PurchaseVehicleCommand(vehicle.Id, "request-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleStatus.Sold, vehicle.Status);
        Assert.Equal(1, sales.AddCalls);
        Assert.Equal(vehicle.Id, sales.AddedSale!.VehicleId);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.True(unitOfWork.Transaction.Committed);
    }

    [Fact]
    public async Task Handle_WhenVehicleIsAlreadySold_ReturnsConflictWithoutSavingOrCommitting()
    {
        var vehicle = Vehicle.Create("Toyota", "Corolla", 2025, "White", 120_000m, Now).Value!;
        vehicle.MarkAsSold(Now.AddMinutes(1));
        var sales = new TestSaleRepository();
        var unitOfWork = new TestUnitOfWork();
        var handler = CreateHandler(new TestVehicleRepository { Vehicle = vehicle }, sales, unitOfWork, new TestCurrentUser("buyer-subject"));

        var result = await handler.HandleAsync(new PurchaseVehicleCommand(vehicle.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(VehicleErrors.AlreadySold, result.Error);
        Assert.Equal(0, sales.AddCalls);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
        Assert.False(unitOfWork.Transaction.Committed);
    }

    private static PurchaseVehicleHandler CreateHandler(
        TestVehicleRepository vehicles,
        TestSaleRepository sales,
        TestUnitOfWork unitOfWork,
        TestCurrentUser currentUser)
    {
        return new PurchaseVehicleHandler(vehicles, sales, unitOfWork, currentUser, new TestClock(Now));
    }
}
