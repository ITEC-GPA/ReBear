[CmdletBinding()]
param([switch]$AllowCandidate)
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Push-Location $projectRoot
try {
    # Lock files pin the NuGet graph; this check does not perform a vulnerability audit.
    & dotnet restore ConcreteSectionChecker.sln --locked-mode -p:NuGetAudit=false --nologo
    if ($LASTEXITCODE -ne 0) { throw "Restore failed: $LASTEXITCODE" }
    & dotnet build ConcreteSectionChecker.sln -c Release --no-restore --nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed: $LASTEXITCODE" }
    $checkArguments = @("--grasshopper")
    if ($AllowCandidate) { $checkArguments += "--allow-candidate" }
    # Run the built assembly through the host: a freshly built apphost .exe may be blocked by antivirus software.
    & dotnet obj/bin/ConcreteSectionChecker.Checks/Release/net8.0-windows/ConcreteSectionChecker.Checks.dll @checkArguments
    if ($LASTEXITCODE -ne 0) { throw "Checks failed: $LASTEXITCODE" }
}
finally { Pop-Location }
