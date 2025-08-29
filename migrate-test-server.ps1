# Migration script to convert TestServerBuilder usage to TestWebApplicationFactory
param(
    [Parameter(Mandatory=$true)]
    [string]$FilePath
)

$content = Get-Content -Path $FilePath -Raw

# Replace the using statements if they exist
$content = $content -replace 'using Microsoft\.AspNetCore\.TestHost;', 'using Microsoft.AspNetCore.TestHost;'

# Replace the IDisposable pattern with IClassFixture pattern
$content = $content -replace 'public class (\w+) : IDisposable', 'public class $1 : IClassFixture<TestWebApplicationFactory<TestProgram>>'

# Replace the TestServer and TestServerBuilder fields
$content = $content -replace 'private readonly TestServer _server;', 'private readonly TestWebApplicationFactory<TestProgram> _factory;'
$content = $content -replace 'private readonly TestServer _server', 'private readonly TestWebApplicationFactory<TestProgram> _factory'

# Replace constructor pattern
$constructorPattern = '(?s)public (\w+)\(\)\s*\{[^}]*TestServerBuilder testHostBuilder = new\(\);[^}]*testHostBuilder[^}]*\.BuildServer\([^)]*\);[^}]*\}'
$newConstructorPattern = 'public $1(TestWebApplicationFactory<TestProgram> factory)
    {
        _factory = factory;
        _mockClientHandler = new MockHttpMessageHandler();
        
        _factory.ConfigureServices(builder =>
        {
            // Service configuration will be preserved from original
        })
        .Configure(app =>
        {
            // App configuration will be preserved from original
        });
    }'

# This is complex, so let's handle it manually for now
Write-Host "File: $FilePath needs manual migration"
Write-Host "Please apply these changes:"
Write-Host "1. Change class inheritance to: IClassFixture<TestWebApplicationFactory<TestProgram>>"
Write-Host "2. Replace TestServer _server field with TestWebApplicationFactory<TestProgram> _factory"
Write-Host "3. Update constructor to accept factory parameter and configure it"
Write-Host "4. Replace _server.CreateClient() calls with _factory.CreateClient()"
Write-Host "5. Remove Dispose method if it only disposes _server"
