[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Push-Location $projectRoot
try {
    & dotnet restore tools/ExampleGenerator/ExampleGenerator.csproj --locked-mode -p:NuGetAudit=false --nologo
    if ($LASTEXITCODE -ne 0) { throw "Restore failed: $LASTEXITCODE" }
    & dotnet build tools/ExampleGenerator/ExampleGenerator.csproj -c Release --no-restore --nologo
    if ($LASTEXITCODE -ne 0) { throw "Build failed: $LASTEXITCODE" }
    & dotnet obj/bin/ExampleGenerator/Release/net8.0-windows/ExampleGenerator.dll
    if ($LASTEXITCODE -ne 0) { throw "Example checks failed: $LASTEXITCODE" }
}
finally { Pop-Location }
