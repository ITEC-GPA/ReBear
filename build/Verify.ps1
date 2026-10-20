[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Push-Location $projectRoot
try {
    # Lock files pin the NuGet graph; this check does not perform a vulnerability audit.
    & dotnet restore ConcreteSectionChecker.sln --locked-mode -p:NuGetAudit=false --nologo
    if ($LASTEXITCODE -ne 0) { throw "Restore failed: $LASTEXITCODE" }
    & dotnet build ConcreteSectionChecker.sln -c Release --no-restore --nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed: $LASTEXITCODE" }
    & dotnet run --project tests/ConcreteSectionChecker.Checks -c Release --no-build -- --grasshopper
    if ($LASTEXITCODE -ne 0) { throw "Checks failed: $LASTEXITCODE" }
}
finally { Pop-Location }
