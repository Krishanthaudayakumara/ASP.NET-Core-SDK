# Simple batch migration script
Write-Host "Starting migration..." -ForegroundColor Green

$files = Get-ChildItem "tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures" -Recurse -Filter "*.cs" | 
    Where-Object { (Get-Content $_.FullName -Raw) -match "TestServerBuilder" }

Write-Host "Found $($files.Count) files with TestServerBuilder"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    if ($content -match "TestServerBuilder testHostBuilder = new\(\);" -and $content -notmatch "public static.*TestServerBuilder") {
        Write-Host "Migrating: $($file.Name)"
        
        # Apply transformations
        $content = $content -replace "using Microsoft\.AspNetCore\.TestHost;", "using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;"
        $content = $content -replace "public class (\w+) : IDisposable", "public class `$1 : IClassFixture<TestWebApplicationFactory<TestPagesProgram>>"
        $content = $content -replace "private readonly TestServer _server;", "private readonly WebApplicationFactory<TestPagesProgram> _factory;"
        $content = $content -replace "public (\w+)\(\)", "public `$1(TestWebApplicationFactory<TestPagesProgram> factory)"
        $content = $content -replace "TestServerBuilder testHostBuilder = new\(\);", "_factory = factory"
        $content = $content -replace "testHostBuilder\.", "_factory."
        $content = $content -replace "_server = testHostBuilder\.BuildServer\(new Uri\(.*?\)\);", ""
        $content = $content -replace "_server\.CreateClient\(\)", "_factory.CreateClient()"
        $content = $content -replace "_server\.Services", "_factory.Services"
        $content = $content -replace "public void Dispose\(\)\s*\{[^{}]*\}", ""
        
        Set-Content $file.FullName $content -Encoding UTF8
        Write-Host "Completed: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "Migration completed!" -ForegroundColor Green
