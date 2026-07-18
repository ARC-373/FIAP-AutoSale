using AutoSale.Domain.Vehicles;

namespace AutoSale.Domain.UnitTests.Vehicles;

public sealed class VehicleTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WhenDetailsAreValid_CreatesAvailableVehicleWithNormalizedValues()
    {
        var result = Vehicle.Create(" Toyota ", " Corolla ", 2025, " White ", 125_000.50m, Now);

        Assert.True(result.IsSuccess);
        var vehicle = Assert.IsType<Vehicle>(result.Value);
        Assert.NotEqual(Guid.Empty, vehicle.Id);
        Assert.Equal("Toyota", vehicle.Make);
        Assert.Equal("Corolla", vehicle.Model);
        Assert.Equal("White", vehicle.Color);
        Assert.Equal(VehicleStatus.Available, vehicle.Status);
        Assert.Equal(Now, vehicle.CreatedAtUtc);
        Assert.Equal(Now, vehicle.UpdatedAtUtc);
        Assert.Equal(1, vehicle.Version);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(1.999)]
    public void Create_WhenPriceIsInvalid_ReturnsValidationError(decimal price)
    {
        var result = Vehicle.Create("Toyota", "Corolla", 2025, "White", price, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(VehicleErrors.InvalidPrice, result.Error);
    }

    [Theory]
    [InlineData(1885)]
    [InlineData(2028)]
    public void Create_WhenYearIsOutsideAllowedRange_ReturnsValidationError(int year)
    {
        var result = Vehicle.Create("Toyota", "Corolla", year, "White", 125_000m, Now);

        Assert.True(result.IsFailure);
        Assert.Equal(VehicleErrors.InvalidYear, result.Error);
    }

    [Fact]
    public void UpdateDetails_WhenVehicleIsAvailable_UpdatesDetailsTimestampAndVersion()
    {
        var vehicle = CreateVehicle();
        var updatedAt = Now.AddDays(1);

        var result = vehicle.UpdateDetails("Honda", "Civic", 2026, "Black", 150_000m, updatedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal("Honda", vehicle.Make);
        Assert.Equal("Civic", vehicle.Model);
        Assert.Equal(150_000m, vehicle.Price);
        Assert.Equal(updatedAt, vehicle.UpdatedAtUtc);
        Assert.Equal(2, vehicle.Version);
    }

    [Fact]
    public void UpdateDetails_WhenVehicleIsSold_ReturnsConflictWithoutChangingDetails()
    {
        var vehicle = CreateVehicle();
        vehicle.MarkAsSold(Now.AddHours(1));

        var result = vehicle.UpdateDetails("Honda", "Civic", 2026, "Black", 150_000m, Now.AddDays(1));

        Assert.True(result.IsFailure);
        Assert.Equal(VehicleErrors.CannotUpdateSoldVehicle, result.Error);
        Assert.Equal("Toyota", vehicle.Make);
    }

    [Fact]
    public void MarkAsSold_WhenVehicleIsAvailable_ChangesStatusAndVersion()
    {
        var vehicle = CreateVehicle();

        var result = vehicle.MarkAsSold(Now.AddHours(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(VehicleStatus.Sold, vehicle.Status);
        Assert.Equal(Now.AddHours(1), vehicle.UpdatedAtUtc);
        Assert.Equal(2, vehicle.Version);
    }

    [Fact]
    public void MarkAsSold_WhenVehicleIsAlreadySold_ReturnsConflict()
    {
        var vehicle = CreateVehicle();
        vehicle.MarkAsSold(Now.AddHours(1));

        var result = vehicle.MarkAsSold(Now.AddHours(2));

        Assert.True(result.IsFailure);
        Assert.Equal(VehicleErrors.AlreadySold, result.Error);
    }

    private static Vehicle CreateVehicle()
    {
        return Vehicle.Create("Toyota", "Corolla", 2025, "White", 125_000m, Now).Value!;
    }
}
