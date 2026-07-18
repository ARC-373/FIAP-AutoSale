using System.Reflection;

namespace AutoSale.ArchitectureTests;

public sealed class CleanArchitectureTests
{
    [Theory]
    [InlineData("AutoSale.SharedKernel", "AutoSale.Domain")]
    [InlineData("AutoSale.SharedKernel", "AutoSale.Application")]
    [InlineData("AutoSale.SharedKernel", "AutoSale.Infrastructure")]
    [InlineData("AutoSale.SharedKernel", "AutoSale.Api")]
    [InlineData("AutoSale.Domain", "AutoSale.Application")]
    [InlineData("AutoSale.Domain", "AutoSale.Infrastructure")]
    [InlineData("AutoSale.Domain", "AutoSale.Api")]
    [InlineData("AutoSale.Application", "AutoSale.Infrastructure")]
    [InlineData("AutoSale.Application", "AutoSale.Api")]
    public void InnerLayer_DoesNotReferenceOuterLayer(string innerAssemblyName, string outerAssemblyName)
    {
        var innerAssembly = Assembly.Load(innerAssemblyName);

        Assert.DoesNotContain(
            innerAssembly.GetReferencedAssemblies(),
            reference => string.Equals(reference.Name, outerAssemblyName, StringComparison.Ordinal));
    }
}
