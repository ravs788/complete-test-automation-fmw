param(
    [Parameter(Mandatory = $true)]
    [string]$ResultsDirectory
)

# Run with Windows PowerShell 5.1: Add-Type emits a small .NET Framework executable.
# The real helper runs only in a temporary copy and never invokes the actual .NET SDK.
$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') {
    throw 'This verification must run on Windows (the hosted Azure Windows agent is sufficient).'
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$runRoot = Join-Path ([IO.Path]::GetTempPath()) ('r25-' + [Guid]::NewGuid().ToString('N'))
$previousProbeRoot = $env:R25_PROBE_ROOT
New-Item -ItemType Directory -Force -Path $ResultsDirectory | Out-Null
$ResultsDirectory = (Resolve-Path $ResultsDirectory).Path
$result = [ordered]@{
    finding = 'R25'
    platform = 'Windows'
    expectedExitCode = 37
    actualExitCode = $null
    commands = @()
    staleBinaryPresent = $false
    testCommandStarted = $null
    passed = $false
    error = $null
}

try {
    $scriptDirectory = Join-Path $runRoot 'scripts\windows'
    $stubDirectory = Join-Path $runRoot 'tools\dotnet'
    $binaryDirectory = Join-Path $runRoot 'Tests\bin\Debug\net10.0'
    New-Item -ItemType Directory -Force -Path $scriptDirectory, $stubDirectory, $binaryDirectory | Out-Null
    $helper = Join-Path $scriptDirectory '_run_playwright_tests.bat'
    Copy-Item (Join-Path $PSScriptRoot '_run_playwright_tests.bat') $helper
    $staleBinary = Join-Path $binaryDirectory 'Tests.dll'
    [IO.File]::WriteAllText($staleBinary, 'stale compiled output for failure-path verification')

    $stub = @'
using System;
using System.IO;
public static class DotnetFailureProbe
{
    public static int Main(string[] args)
    {
        string root = Environment.GetEnvironmentVariable("R25_PROBE_ROOT");
        string command = args.Length == 0 ? "missing-command" : args[0];
        File.AppendAllText(Path.Combine(root, "commands.txt"), command + Environment.NewLine);
        if (command == "build") return 37;
        File.WriteAllText(Path.Combine(root, "tests-started"), command);
        return 0;
    }
}
'@
    Add-Type -TypeDefinition $stub -OutputAssembly (Join-Path $stubDirectory 'dotnet.exe') -OutputType ConsoleApplication
    $env:R25_PROBE_ROOT = $runRoot
    # CALL returns control to cmd.exe; the helper must return the stub build's exit code.
    & $env:ComSpec /d /c ('call "' + $helper + '" verification') 2>&1 |
        Tee-Object -FilePath (Join-Path $ResultsDirectory 'helper-output.log')
    $helperExit = $LASTEXITCODE
    $commands = @(Get-Content (Join-Path $runRoot 'commands.txt'))
    $result.actualExitCode = $helperExit
    $result.commands = $commands
    $result.staleBinaryPresent = Test-Path $staleBinary
    $result.testCommandStarted = Test-Path (Join-Path $runRoot 'tests-started')
    if ($helperExit -ne 37) { throw "Expected exit 37, got $helperExit" }
    if ($commands.Count -ne 1 -or $commands[0] -ne 'build') { throw 'The helper invoked a command after the failed build.' }
    if ($result.testCommandStarted) { throw 'The test command ran despite a failed build.' }
    if (-not $result.staleBinaryPresent) { throw 'The stale binary fixture was unexpectedly removed.' }
    $result.passed = $true
    Write-Host 'PASS R25: build exit 37 preserved; stale binary present; test command never invoked.'
    # PowerShell@2 propagates LASTEXITCODE; the nonzero probe exit was expected and checked.
    $global:LASTEXITCODE = 0
}
catch {
    $result.error = $_.Exception.Message
    throw
}
finally {
    $env:R25_PROBE_ROOT = $previousProbeRoot
    Remove-Item -LiteralPath $runRoot -Recurse -Force -ErrorAction SilentlyContinue
    $result | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 (Join-Path $ResultsDirectory 'verification.json')
}
