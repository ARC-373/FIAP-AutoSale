using AutoSale.Application.Common;
using AutoSale.Application.Vehicles.Create;
using AutoSale.Application.Vehicles.ListAvailable;
using AutoSale.Application.Vehicles.Update;
using AutoSale.Domain.Vehicles;

namespace AutoSale.Application.UnitTests.Vehicles;

public sealed class VehicleHandlersTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 18, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Create_WhenCommandIsValid_PersistsVehicleAndReturnsDto()
    {
        var repository = new TestVehicleRepository();
        var unitOfWork = new TestUnitOfWork();
        var handler = new CreateVehicleHandler(repository, unitOfWork, new TestClock(Now));

        var result = await handler.HandleAsync(new CreateVehicleCommand("Toyota", "Corolla", 2025, "White", 120_000m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, repository.AddCalls);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
        Assert.Equal("Toyota", result.Value!.Make);
    }

    [Fact]
    public async Task Update_WhenVehicleDoesNotExist_ReturnsNotFoundWithoutSaving()
    {
        var repository = new TestVehicleRepository();
        var unitOfWork = new TestUnitOfWork();
        var handler = new UpdateVehicleHandler(repository, unitOfWork, new TestClock(Now));

        var result = await handler.HandleAsync(new UpdateVehicleCommand(Guid.NewGuid(), "Toyota", "Corolla", 2025, "White", 120_000m), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.VehicleNotFound, result.Error);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }

    [Fact]
    public async Task ListAvailable_WhenPageIsInvalid_ReturnsValidationWithoutCallingRepository()
    {
        var repository = new TestVehicleRepository();
        var handler = new ListAvailableVehiclesHandler(repository);

        var result = await handler.HandleAsync(new ListAvailableVehiclesQuery(0, 20), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ApplicationErrors.InvalidPage, result.Error);
    }

    [Fact]
    public async Task Update_WhenVehicleIsSold_ReturnsDomainConflictWithoutSaving()
    {
        var vehicle = Vehicle.Create("Toyota", "Corolla", 2025, "White", 120_000m, Now).Value!;
        vehicle.MarkAsSold(Now.AddMinutes(1));
        var repository = new TestVehicleRepository { Vehicle = vehicle };
        var unitOfWork = new TestUnitOfWork();
        var handler = new UpdateVehicleHandler(repository, unitOfWork, new TestClock(Now));

        var result = await handler.HandleAsync(new UpdateVehicleCommand(vehicle.Id, "Honda", "Civic", 2025, "Black", 130_000m), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(VehicleErrors.CannotUpdateSoldVehicle, result.Error);
        Assert.Equal(0, unitOfWork.SaveChangesCalls);
    }
}
