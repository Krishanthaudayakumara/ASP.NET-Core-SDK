#!/usr/bin/env pwsh

# Script to identify integration test fixtures that need conversion from shared to individual factory pattern

$testDir = "C:\github\ASP.NET-Core-SDK\tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests\Fixtures"

Write-Host "Scanning for fixtures using shared IClassFixture pattern..." -ForegroundColor Yellow

# Find all fixture files that use IClassFixture pattern
$fixtureFiles = Get-ChildItem -Path $testDir -Filter "*.cs" -Recurse | 
    Where-Object { 
        $content = Get-Content $_.FullName -Raw
        $content -match "IClassFixture<TestWebApplicationFactory" -and
        $content -match "_factory\s*=" -and
        $content -notmatch "CreateFactory\(\)" -and
        $_.Name -ne "RequestHeadersValidationFixture.cs" -and
        $_.Name -ne "RequestMappingFixture.cs"
    }

Write-Host "Found $($fixtureFiles.Count) fixtures that need conversion:" -ForegroundColor Green

foreach ($file in $fixtureFiles) {
    $relativePath = $file.FullName.Replace("C:\github\ASP.NET-Core-SDK\", "")
    Write-Host "  $relativePath" -ForegroundColor Cyan
    
    # Check if it has constructor configuration
    $content = Get-Content $file.FullName -Raw
    if ($content -match "ConfigureServices\(") {
        Write-Host "    ^ Has ConfigureServices - HIGH PRIORITY" -ForegroundColor Red
    }
}

Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Convert fixtures with ConfigureServices first (these cause the most conflicts)" -ForegroundColor White
Write-Host "2. Use individual factory pattern like RequestHeadersValidationFixture" -ForegroundColor White
Write-Host "3. Test each conversion to ensure it works" -ForegroundColor White
