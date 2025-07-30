# FluentAssertions to Shouldly migration script
#
# This script migrates most common FluentAssertions usages to Shouldly in all .cs files (excluding obj/bin folders).
#
# Optional: To enable file backup before migration, uncomment the backup section below.

$files = Get-ChildItem -Recurse -Include *.cs | Where-Object {
    $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\'
} | Sort-Object FullName
$total = $files.Count

# --- Optional: Backup files before migration ---
# $timestamp = Get-Date -Format "yyyyMMddHHmmss"
# $backupDir = "backup-$timestamp"
# New-Item -ItemType Directory -Path $backupDir | Out-Null
# $i = 0
# foreach ($file in $files) {
#     $i++
#     Write-Progress -Activity "Backing up files" -Status "$i of $total" -PercentComplete (($i/$total)*100)
#     if (Test-Path $file.FullName) {
#         $relativePath = $file.FullName.Substring($PWD.Path.Length).TrimStart('\\','/')
#         $destPath = Join-Path $backupDir $relativePath
#         $destDir = Split-Path $destPath
#         if (!(Test-Path $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }
#         Copy-Item $file.FullName -Destination $destPath -Force
#     }
# }
# --- End backup section ---

$i = 0
foreach ($file in $files) {
    $i++
    Write-Progress -Activity "Migrating assertions" -Status "$i of $total" -PercentComplete (($i/$total)*100)
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    if (![string]::IsNullOrWhiteSpace($content)) {
        # Replace using
        $content = $content -replace 'using FluentAssertions;', 'using Shouldly;'

        # Exception assertions (sync/async)
        $content = [regex]::Replace($content, 'await (\w+)\.Should\(\)\.ThrowAsync<([^>]+)>\(\)', 'var ex = await Should.ThrowAsync<$2>($1); // TODO: Assert exception properties manually')
        $content = [regex]::Replace($content, '(\w+)\.Should\(\)\.Throw<([^>]+)>\(\)', 'var ex = Should.Throw<$2>(() => $1()); // TODO: Assert exception properties manually')
        $content = [regex]::Replace($content, '(\w+)\.Should\(\)\.NotThrow<([^>]+)>\(\)', 'Should.NotThrow<$2>(() => $1())')
        $content = [regex]::Replace($content, '(\w+)\.Should\(\)\.Throw\(\)', 'var ex = Should.Throw<Exception>(() => $1()); // TODO: Assert exception properties manually')
        $content = [regex]::Replace($content, '(\w+)\.Should\(\)\.NotThrow\(\)', 'Should.NotThrow(() => $1())')

        # Chained WithMessage/WithParameterName/And/Which/Subject
        $content = [regex]::Replace($content, '\.WithMessage\([^)]+\)', '// TODO: Assert exception.Message manually')
        $content = [regex]::Replace($content, '\.WithParameterName\([^)]+\)', '// TODO: Assert exception.ParamName manually')
        $content = [regex]::Replace($content, '\.And\.', '// TODO: Split assertion chain manually')
        $content = [regex]::Replace($content, '\.Which', '// TODO: Migrate .Which manually')
        $content = [regex]::Replace($content, '\.Subject', '// TODO: Migrate .Subject manually')

        # As<T>
        $content = [regex]::Replace($content, '\.Should\(\)\.As<([^>]+)>\(\)', ' -as $1 | ShouldNotBeNull() // TODO: Check cast manually')

        # ContainItemsAssignableTo<T>
        $content = [regex]::Replace($content, '\.Should\(\)\.ContainItemsAssignableTo<([^>]+)>\(\)', '.All(x => x is $1).ShouldBeTrue() // TODO: Check type safety manually')

        # Split .And. chains into separate lines (after other replacements)
        $content = [regex]::Replace($content, '\.Should\(\)((?:\.[A-Za-z0-9<>]+\([^\)]*\))+).And.([A-Za-z0-9<>]+\([^\)]*\))', {
            param($m)
            $first = $m.Groups[1].Value
            $second = $m.Groups[2].Value
            ".Should()$first;`r`n// TODO: Split assertion chain manually`r`n$($m.Value.Split('.And.')[0]).$second"
        })

        # Replace advanced and basic assertions
        $replacements = @{
            '\.Should\(\)\.NotBeNullOrWhiteSpace\(\)' = '.ShouldNotBeNullOrWhiteSpace()'
            '\.Should\(\)\.BeOfType\(typeof\(([^)]+)\)\)' = '.ShouldBeOfType<$1>()'
            '\.Should\(\)\.BeEquivalentTo\(([^)]+)\)' = '.ShouldBe($1)'
            '\.Should\(\)\.As<([^>]+)>\(\)' = ' -as $1 | ShouldNotBeNull() // TODO: Check cast manually'
            '\.Should\(\)\.BeOfType<([^>]+)>\(\)' = '.ShouldBeOfType<$1>()'
            '\.Should\(\)\.BeAssignableTo<([^>]+)>\(\)' = '.ShouldBeAssignableTo<$1>()'
            '\.Should\(\)\.Be\(([^)]+)\)' = '.ShouldBe($1)'
            '\.Should\(\)\.NotBe\(([^)]+)\)' = '.ShouldNotBe($1)'
            '\.Should\(\)\.BeNull\(\)' = '.ShouldBeNull()'
            '\.Should\(\)\.NotBeNull\(\)' = '.ShouldNotBeNull()'
            '\.Should\(\)\.BeTrue\(\)' = '.ShouldBeTrue()'
            '\.Should\(\)\.BeFalse\(\)' = '.ShouldBeFalse()'
            '\.Should\(\)\.BeEmpty\(\)' = '.ShouldBeEmpty()'
            '\.Should\(\)\.NotBeEmpty\(\)' = '.ShouldNotBeEmpty()'
            '\.Should\(\)\.Contain\(([^)]+)\)' = '.ShouldContain($1)'
            '\.Should\(\)\.NotContain\(([^)]+)\)' = '.ShouldNotContain($1)'
            '\.Should\(\)\.ContainSingle\(([^)]+)\)' = '.Single($1).ShouldNotBeNull()'
            '\.Should\(\)\.ContainSingle\(\)' = '.Count.ShouldBe(1)'
            '\.Should\(\)\.HaveCount\(([^)]+)\)' = '.Count.ShouldBe($1)'
            '\.Should\(\)\.Equal\(([^)]+)\)' = '.ShouldBe($1)'
            '\.Should\(\)\.NotEqual\(([^)]+)\)' = '.ShouldNotBe($1)'
            '\.Should\(\)\.BeSameAs\(([^)]+)\)' = '.ShouldBeSameAs($1)'
            '\.Should\(\)\.NotBeSameAs\(([^)]+)\)' = '.ShouldNotBeSameAs($1)'
            '\.Should\(\)\.BePositive\(\)' = '.ShouldBePositive()'
            '\.Should\(\)\.BeNegative\(\)' = '.ShouldBeNegative()'
            '\.Should\(\)\.BeGreaterThan\(([^)]+)\)' = '.ShouldBeGreaterThan($1)'
            '\.Should\(\)\.BeLessThan\(([^)]+)\)' = '.ShouldBeLessThan($1)'
            '\.Should\(\)\.BeGreaterOrEqualTo\(([^)]+)\)' = '.ShouldBeGreaterThanOrEqualTo($1)'
            '\.Should\(\)\.BeLessOrEqualTo\(([^)]+)\)' = '.ShouldBeLessThanOrEqualTo($1)'
            '\.Should\(\)\.ContainItemsAssignableTo<([^>]+)>\(\)' = '.All({ $_ -is [$1] }).ShouldBeTrue() // TODO: Manual review for type safety'
        }
        foreach ($pattern in $replacements.Keys) {
            $content = [regex]::Replace($content, $pattern, $replacements[$pattern])
        }

        if ($content -ne $originalContent) {
            Set-Content $file.FullName $content
        }
    }
}

Write-Host "Migration script completed. Please search for 'TODO' in your codebase, review flagged spots, and run your tests."