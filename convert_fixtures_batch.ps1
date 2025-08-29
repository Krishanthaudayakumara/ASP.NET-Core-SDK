#!/usr/bin/env pwsh

# Script to batch convert integration test fixtures from shared IClassFixture to individual factory pattern

$testDir = "C:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures"

Write-Host "Converting fixtures to individual factory pattern..." -ForegroundColor Yellow

# Get all fixture files that need conversion (excluding already converted ones)
$fixtureFiles = Get-ChildItem -Path $testDir -Filter "*.cs" -Recurse | 
    Where-Object { 
        $content = Get-Content $_.FullName -Raw
        $content -match "IClassFixture<TestWebApplicationFactory" -and
        $content -match "ConfigureServices\(" -and
        $_.Name -ne "RequestHeadersValidationFixture.cs" -and
        $_.Name -ne "RequestMappingFixture.cs" -and
        $_.Name -ne "ImageFieldTagHelperFixture.cs"
    }

Write-Host "Found $($fixtureFiles.Count) fixtures to convert" -ForegroundColor Green

# Convert a smaller batch first for testing
$batchSize = 5
$batch = $fixtureFiles | Select-Object -First $batchSize

foreach ($file in $batch) {
    Write-Host "Converting: $($file.Name)" -ForegroundColor Cyan
    
    $content = Get-Content $file.FullName -Raw
    
    # Extract the class name
    if ($content -match "public class (\w+) :") {
        $className = $matches[1]
        Write-Host "  Class: $className" -ForegroundColor White
    }
    
    # Pattern 1: Remove IClassFixture inheritance
    $content = $content -replace "public class ($className) : IClassFixture<TestWebApplicationFactory<(\w+)>>", "public class `$1"
    
    # Pattern 2: Remove factory field and constructor - more complex, so we'll do it step by step
    if ($content -match "private readonly WebApplicationFactory<(\w+)> _factory;") {
        $programType = $matches[1]
        
        # Remove the factory field
        $content = $content -replace "private readonly WebApplicationFactory<\w+> _factory;[^\r\n]*", ""
        
        # Remove other private readonly fields that are typically used
        $content = $content -replace "private readonly MockHttpMessageHandler _mockClientHandler;[^\r\n]*", ""
        $content = $content -replace "private readonly Uri _layoutServiceUri[^;]*;[^\r\n]*", ""
        
        # Find and replace the constructor
        $constructorPattern = "public $className\(TestWebApplicationFactory<$programType> factory\)\s*{[^}]+}"
        if ($content -match $constructorPattern) {
            # Replace constructor with CreateFactory method
            $createFactoryMethod = @"
    private readonly Uri _layoutServiceUri = new("http://layout.service");

    private TestWebApplicationFactory<$programType> CreateFactory(MockHttpMessageHandler mockClientHandler)
    {
        return new TestWebApplicationFactory<$programType>()
            .ConfigureServices(builder =>
            {
                // Configuration will be extracted from original constructor
            })
            .Configure(app =>
            {
                // Configuration will be extracted from original constructor
            });
    }
"@
            $content = $content -replace $constructorPattern, $createFactoryMethod
        }
        
        # Create backup
        $backupPath = $file.FullName + ".backup"
        Copy-Item $file.FullName $backupPath
        
        # Write the updated content
        $content | Set-Content $file.FullName -NoNewline
        
        Write-Host "  ✓ Converted (backup saved)" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ Could not find factory field pattern" -ForegroundColor Yellow
    }
}

Write-Host "`nBatch conversion complete!" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Review converted files and fix CreateFactory method implementations" -ForegroundColor White
Write-Host "2. Update test methods to use individual factory instances" -ForegroundColor White
Write-Host "3. Test each converted fixture" -ForegroundColor White
