# PowerShell script to migrate TestServerBuilder to TestWebApplicationFactory
param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath
)

Write-Host "Migrating: $FilePath"

# Read the file content
$content = Get-Content $FilePath -Raw

# 1. Update using statements
$content = $content -replace "using Microsoft.AspNetCore.TestHost;", "using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;"

# 2. Change class inheritance
$content = $content -replace "public class (\w+) : IDisposable", "public class `$1 : IClassFixture<TestWebApplicationFactory<TestPagesProgram>>"

# 3. Update field declaration
$content = $content -replace "private readonly TestServer _server;", "private readonly WebApplicationFactory<TestPagesProgram> _factory;"

# 4. Update constructor pattern
$content = $content -replace "public (\w+)\(\)\s*\{\s*TestServerBuilder testHostBuilder = new\(\);", "public `$1(TestWebApplicationFactory<TestPagesProgram> factory)
    {
        _factory = factory"

# 5. Replace TestServerBuilder usage pattern
$content = $content -replace "testHostBuilder\s*\.ConfigureServices", "_factory.ConfigureServices"
$content = $content -replace "testHostBuilder\s*\.Configure", "_factory.Configure"

# 6. Remove BuildServer call
$content = $content -replace "\s*_server = testHostBuilder\.BuildServer\(new Uri\(.*?\)\);\s*", ""

# 7. Replace client creation
$content = $content -replace "_server\.CreateClient\(\)", "_factory.CreateClient()"

# 8. Remove Dispose method entirely
$content = $content -replace "\s*public void Dispose\(\)\s*\{[^}]*\}\s*", ""

# Write the updated content back
Set-Content $FilePath $content

Write-Host "Migration completed for: $FilePath"
