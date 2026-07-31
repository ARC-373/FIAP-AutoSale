using AutoSale.Domain.Sales;

namespace AutoSale.Domain.UnitTests.Sales;

public sealed class SaleTests
{
    private static readonly DateTimeOffset PurchasedAt = new(2026, 7, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WhenValuesAreValid_CreatesSaleWithPriceSnapshotAndBuyerSubject()
    {
        var vehicleId = Guid.NewGuid();

        var result = Sale.Create(vehicleId, " cognito-subject ", 125_000m, PurchasedAt, " request-123 ");

        Assert.True(result.IsSuccess);
        var sale = Assert.IsType<Sale>(result.Value);
        Assert.Equal(vehicleId, sale.VehicleId);
        Assert.Equal("cognito-subject", sale.BuyerSubject);
        Assert.Equal(125_000m, sale.SalePrice);
        Assert.Equal(PurchasedAt, sale.PurchasedAtUtc);
        Assert.Equal("request-123", sale.IdempotencyKey);
    }

    [Fact]
    public void Create_WhenBuyerSubjectIsBlank_ReturnsValidationError()
    {
        var result = Sale.Create(Guid.NewGuid(), " ", 125_000m, PurchasedAt);

        Assert.True(result.IsFailure);
        Assert.Equal(SaleErrors.InvalidBuyerSubject, result.Error);
    }
}
