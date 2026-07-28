using System.Data;
using AutoSale.Application.Abstractions.Authentication;
using AutoSale.Application.Abstractions.Clock;
using AutoSale.Application.Abstractions.Persistence;
using AutoSale.Application.Common;
using AutoSale.Application.Sales;
using AutoSale.Application.Vehicles;
using AutoSale.Domain.Sales;
using AutoSale.Domain.Vehicles;

namespace AutoSale.Application.UnitTests;

internal sealed class TestClock : IClock
{
    public TestClock(DateTimeOffset utcNow) => UtcNow = utcNow;

    public DateTimeOffset UtcNow { get; }
}

internal sealed class TestCurrentUser : ICurrentUser
{
    public TestCurrentUser(string? subject) => Subject = subject;

    public string? Subject { get; }
}

internal sealed class TestVehicleRepository : IVehicleRepository
{
    public Vehicle? Vehicle { get; set; }

    public int AddCalls { get; private set; }

    public int PurchaseLookupCalls { get; private set; }

    public PagedResult<VehicleDto> AvailableVehicles { get; set; } = new([], 1, 20, 0);

    public Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        AddCalls++;
        Vehicle = vehicle;
        return Task.CompletedTask;
    }

    public Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Vehicle?.Id == id ? Vehicle : null);

    public Task<Vehicle?> GetByIdForPurchaseAsync(Guid id, CancellationToken cancellationToken)
    {
        PurchaseLookupCalls++;
        return Task.FromResult(Vehicle?.Id == id ? Vehicle : null);
    }

    public Task<PagedResult<VehicleDto>> ListAvailableAsync(int page, int pageSize, CancellationToken cancellationToken) => Task.FromResult(AvailableVehicles);
}

internal sealed class TestSaleRepository : ISaleRepository
{
    public Sale? AddedSale { get; private set; }

    public int AddCalls { get; private set; }

    public PagedResult<SaleDto> SoldVehicles { get; set; } = new([], 1, 20, 0);

    public Task AddAsync(Sale sale, CancellationToken cancellationToken)
    {
        AddCalls++;
        AddedSale = sale;
        return Task.CompletedTask;
    }

    public Task<PagedResult<SaleDto>> ListSoldAsync(int page, int pageSize, CancellationToken cancellationToken) => Task.FromResult(SoldVehicles);
}

internal sealed class TestUnitOfWork : IUnitOfWork
{
    public int BeginTransactionCalls { get; private set; }

    public int SaveChangesCalls { get; private set; }

    public TestTransaction Transaction { get; } = new();

    public Task<ITransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
    {
        Assert.Equal(IsolationLevel.ReadCommitted, isolationLevel);
        BeginTransactionCalls++;
        return Task.FromResult<ITransaction>(Transaction);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCalls++;
        return Task.CompletedTask;
    }
}

internal sealed class TestTransaction : ITransaction
{
    public bool Committed { get; private set; }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        Committed = true;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
