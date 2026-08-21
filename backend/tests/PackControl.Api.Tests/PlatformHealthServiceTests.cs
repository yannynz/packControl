using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PackControl.Infrastructure.Persistence;
using PackControl.Infrastructure.Services;

namespace PackControl.Api.Tests;

public sealed class PlatformHealthServiceTests
{
    [Fact]
    public async Task CheckAsync_ShouldReportOk_ForInMemoryPersistenceAndWritableStorage()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), $"packcontrol-health-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempRoot);

        try
        {
            var service = new PlatformHealthService(
                Options.Create(new StatePersistenceOptions
                {
                    Provider = "InMemory"
                }),
                Options.Create(new FileSystemStorageOptions
                {
                    RootPath = tempRoot
                }),
                new FakeHostEnvironment(tempRoot));

            var report = await service.CheckAsync(CancellationToken.None);

            Assert.Equal("ok", report.Status);
            Assert.Equal(2, report.Checks.Count);
            Assert.All(report.Checks, check => Assert.Equal("ok", check.Status));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private sealed class FakeHostEnvironment(string contentRootPath) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "PackControl.Api.Tests";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
