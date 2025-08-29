# Comprehensive migration script for TestServerBuilder to TestWebApplicationFactory
$files = @(
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\GlobalMiddlewareFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\Multisite\MultisiteFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\RequestExtensionsFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\RequestDefaultsConfigurationFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\RequestHeadersValidationFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\SearchOptimization\EdgeSitemapProxyFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\Localization\DefaultLocalizationFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\Tracking\AttributeBasedTrackingFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\Localization\LocalizationFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\SearchOptimization\SitemapProxyFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\Localization\LocalizationUsingAttributeMiddlewareFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\Tracking\TrackingFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\TagHelpers\AllFieldTagHelpersFixture.cs",
    "c:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures\Tracking\TrackingProxyFixture.cs"
    # Add more files as needed
)

Write-Host "Starting migration of TestServerBuilder to TestWebApplicationFactory..." -ForegroundColor Green

foreach ($file in $files) {
    if (Test-Path $file) {
        Write-Host "Processing: $file" -ForegroundColor Yellow
        
        $content = Get-Content -Path $file -Raw
        $originalContent = $content
        
        # 1. Update using statements - remove TestHost if present
        $content = $content -replace 'using Microsoft\.AspNetCore\.TestHost;\r?\n', ''
        
        # 2. Update class inheritance
        $content = $content -replace 'public class (\w+) : IDisposable', 'public class $1 : IClassFixture<TestWebApplicationFactory<TestProgram>>'
        
        # 3. Replace fields
        $content = $content -replace 'private readonly TestServer _server;', 'private readonly TestWebApplicationFactory<TestProgram> _factory;'
        
        # 4. Update constructor signature and implementation
        $content = $content -replace 'public (\w+)\(\)', 'public $1(TestWebApplicationFactory<TestProgram> factory)'
        
        # 5. Replace TestServerBuilder initialization
        $content = $content -replace 'TestServerBuilder testHostBuilder = new\(\);[^_]*_mockClientHandler = new MockHttpMessageHandler\(\);[^_]*testHostBuilder', '_factory = factory;
        _mockClientHandler = new MockHttpMessageHandler();
        _factory'
        
        # 6. Replace BuildServer call
        $content = $content -replace '\.BuildServer\(new Uri\("http://localhost"\)\);', ';'
        
        # 7. Replace client creation calls
        $content = $content -replace '_server\.CreateClient\(\)', '_factory.CreateClient()'
        
        # 8. Replace server services access
        $content = $content -replace '_server\.Services', '_factory.Services'
        
        # 9. Remove Dispose method content
        $content = $content -replace 'public void Dispose\(\)[^}]*_server\.Dispose\(\);[^}]*\}', ''
        $content = $content -replace 'public void Dispose\(\)[^}]*\}', ''
        
        # Only save if changes were made
        if ($content -ne $originalContent) {
            Set-Content -Path $file -Value $content -Encoding UTF8
            Write-Host "  ✓ Migrated successfully" -ForegroundColor Green
        } else {
            Write-Host "  - No changes needed" -ForegroundColor Gray
        }
    } else {
        Write-Host "File not found: $file" -ForegroundColor Red
    }
}

Write-Host "Migration completed!" -ForegroundColor Green
Write-Host "Please manually review and test each migrated file." -ForegroundColor Yellow
