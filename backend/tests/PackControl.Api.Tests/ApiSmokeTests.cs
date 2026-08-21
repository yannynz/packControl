namespace PackControl.Api.Tests;

public sealed class ApiSmokeTests
{
    [Fact]
    public void ProgramType_ShouldExist()
    {
        Assert.NotNull(typeof(Program));
    }
}
