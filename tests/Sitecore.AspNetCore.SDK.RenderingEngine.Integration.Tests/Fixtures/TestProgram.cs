using System;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Binding;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;

// Minimal-hosting top-level program used by integration tests.
// Mode can be switched by setting the TEST_MODE environment variable to "Binding".
var mode = Environment.GetEnvironmentVariable("TEST_MODE") ?? "Pages";

var builder = WebApplication.CreateBuilder(args);

if (string.Equals(mode, "Binding", StringComparison.OrdinalIgnoreCase))
{
    TestBindingProgram.ConfigureServices(builder);
}
else
{
    TestPagesProgram.ConfigureServices(builder);
}

var app = builder.Build();

if (string.Equals(mode, "Binding", StringComparison.OrdinalIgnoreCase))
{
    TestBindingProgram.ConfigureApp(app);
}
else
{
    TestPagesProgram.ConfigureApp(app);
}

app.Run();
