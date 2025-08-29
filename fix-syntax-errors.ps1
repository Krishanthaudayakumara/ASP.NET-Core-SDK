# PowerShell script to fix common syntax errors introduced by automated migration
param([string]$ProjectPath = "tests\Sitecore.AspNetCore.SDK.RenderingEngine.Integration.Tests")

Write-Host "Fixing syntax errors in migrated files..."

# Get all CS files in the Fixtures directory
$files = Get-ChildItem "$ProjectPath\Fixtures" -Recurse -Filter "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Fix common syntax errors from automated migration
    
    # Fix missing semicolon after _factory = factory
    $content = $content -replace '(_factory = factory)\s*\n\s*(_mockClientHandler)', '$1;
        $2'
    
    # Fix missing semicolon and proper structure
    $content = $content -replace '(\w+Handler = new Mock\w+MessageHandler\(\);)\s*\n\s*(testHostBuilder)', '$1
        _factory = factory'
    
    # Remove leftover testHostBuilder references
    $content = $content -replace 'testHostBuilder\s*\n\s*\.', '_factory.'
    $content = $content -replace 'testHostBuilder\s*\.', '_factory.'
    
    # Fix BuildServer calls that weren't properly removed
    $content = $content -replace '\s*_server = _factory\.BuildServer\(new Uri\("http://localhost"\)\);\s*', ''
    
    # Remove empty Dispose methods that may have been malformed
    $content = $content -replace '\s*\n\s*\}\s*$', '
}'
    
    # Clean up multiple blank lines
    $content = $content -replace '\n\s*\n\s*\n', "

"
    
    # Fix closing braces preceded by blank lines
    $content = $content -replace '\n\s*\n\s*\}', "
}"
    
    if ($content -ne $originalContent) {
        Set-Content $file.FullName $content
        Write-Host "Fixed syntax errors in: $($file.Name)"
    }
}

Write-Host "Syntax error fixing completed."
