using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Sitecore.AspNetCore.SDK.ExperienceEditor.Extensions;
using Sitecore.AspNetCore.SDK.LayoutService.Client.Interfaces;
using Sitecore.AspNetCore.SDK.RenderingEngine.Extensions;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests;
using Sitecore.AspNetCore.SDK.TestData;

namespace Sitecore.AspNetCore.SDK.RenderingEngine.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
[ExcludeFromCodeCoverage]
public class ExperienceEditorMiddlewareBenchmarks : IDisposable
{
    private readonly TestWebApplicationFactory<TestProgram> _factory;
    private readonly HttpClient _client;
    private readonly StringContent _content;
    private RenderingEngineBenchmarks? _baseLineTestInstance;

    public ExperienceEditorMiddlewareBenchmarks()
    {
        _factory = new TestWebApplicationFactory<TestProgram>();
        _factory
            .ConfigureServices(builder =>
            {
                builder.AddSingleton(Substitute.For<ISitecoreLayoutClient>());
                builder.AddSitecoreRenderingEngine(options =>
                {
                    options.AddDefaultComponentRenderer();
                }).WithExperienceEditor(options =>
                {
                    options.Endpoint = TestConstants.EEMiddlewarePostEndpoint;
                    options.JssEditingSecret = TestConstants.JssEditingSecret;
                });
            })
            .Configure(app =>
            {
                app.UseSitecoreExperienceEditor();
                app.UseRouting();
                app.UseSitecoreRenderingEngine();
            });

        _client = _factory.CreateClient();
        _content = new StringContent(TestConstants.EESampleRequest);
    }

    [GlobalSetup(Target = nameof(RegularHomePageRequest))]
    public void RenderingEngineBenchmarks()
    {
        _baseLineTestInstance = new RenderingEngineBenchmarks();
        _baseLineTestInstance.Setup();
    }

    [Benchmark(Baseline = true)]
    public Task RegularHomePageRequest()
    {
        return _baseLineTestInstance!.RegularHomePageRequest();
    }

    [Benchmark]
    public async Task RegularExperienceEditorRequestHandling()
    {
        HttpResponseMessage response = await _client
            .PostAsync(TestConstants.EEMiddlewarePostEndpoint, _content)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        _factory.Dispose();
        _client.Dispose();
        _content.Dispose();
        _baseLineTestInstance?.Dispose();
        GC.SuppressFinalize(this);
    }
}