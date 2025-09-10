using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.TestBootstrapping;

public interface ITestBootstrapper
{
    void ConfigureServices(IServiceCollection services);

    void Configure(IApplicationBuilder app);
}
