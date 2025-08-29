# Batch migration script for remaining TestServerBuilder files
param(
    [string]$TestProjectPath = "tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests"
)

Write-Host "Starting batch migration of TestServerBuilder files..." -ForegroundColor Green

# Get all fixture files that still contain TestServerBuilder
$filesToMigrate = Get-ChildItem "$TestProjectPath\Fixtures" -Recurse -Filter "*.cs" | 
    Where-Object { 
        $content = Get-Content $_.FullName -Raw
        $content -match "TestServerBuilder" -and 
        $content -notmatch "public static.*TestServerBuilder" # Skip complex static method files
    }

Write-Host "Found $($filesToMigrate.Count) files to migrate" -ForegroundColor Yellow

foreach ($file in $filesToMigrate) {
    Write-Host "Migrating: $($file.Name)" -ForegroundColor Cyan
    
    try {
        $content = Get-Content $file.FullName -Raw
        $originalContent = $content
        
        # Apply standard migration pattern
        $content = $content -replace "using Microsoft\.AspNetCore\.TestHost;", "using Microsoft.AspNetCore.Mvc.Testing;"
        $content = $content -replace "using Microsoft\.AspNetCore\.Mvc\.Testing;\s*using", "using Microsoft.AspNetCore.Mvc.Testing;
using Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests.Fixtures.Pages;
using"
        
        $content = $content -replace "public class (\w+) : IDisposable", "public class `$1 : IClassFixture<TestWebApplicationFactory<TestPagesProgram>>"
        $content = $content -replace "private readonly TestServer _server;", "private readonly WebApplicationFactory<TestPagesProgram> _factory;"
        
        # Handle constructor pattern
        $content = $content -replace "public (\w+)\(\)\s*\{\s*TestServerBuilder testHostBuilder = new\(\);", "public `$1(TestWebApplicationFactory<TestPagesProgram> factory)
    {
        _factory = factory"
        
        # Fix configuration chain
        $content = $content -replace "_factory = factory\s*([^.]*)\.\s*ConfigureServices", "_factory = factory.ConfigureServices"
        $content = $content -replace "testHostBuilder\s*\.", "_factory."
        
        # Remove BuildServer call
        $content = $content -replace "\s*_server = testHostBuilder\.BuildServer\(new Uri\(.*?\)\);\s*", ""
        
        # Replace client/service references
        $content = $content -replace "_server\.CreateClient\(\)", "_factory.CreateClient()"
        $content = $content -replace "_server\.Services", "_factory.Services"
        
        # Remove Dispose method
        $content = $content -replace "\s*public void Dispose\(\)\s*\{[^{}]*(?:\{[^{}]*\}[^{}]*)*\}\s*", "
"
        
        # Only save if content changed
        if ($content -ne $originalContent) {
            Set-Content $file.FullName $content -Encoding UTF8
            Write-Host "  ✓ Successfully migrated $($file.Name)" -ForegroundColor Green
        } else {
            Write-Host "  - No changes needed for $($file.Name)" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "  ✗ Error migrating $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "
Batch migration completed!" -ForegroundColor Green
Write-Host "Run 'dotnet build' to check remaining TestServerBuilder errors." -ForegroundColor Yellow
