using AutoSale.SharedKernel.Results;

namespace AutoSale.Domain.UnitTests.SharedKernel;

public sealed class ResultTests
{
    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_PreservesItsError()
    {
        var error = new Error("example.invalid", "Example validation error.", ErrorType.Validation);

        var result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void GenericSuccess_PreservesItsValue()
    {
        var result = Result.Success("value");

        Assert.True(result.IsSuccess);
        Assert.Equal("value", result.Value);
    }
}
