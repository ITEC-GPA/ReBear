[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
& (Join-Path $PSScriptRoot 'Verify.ps1')

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$version = ([xml](Get-Content -LiteralPath (Join-Path $projectRoot 'Directory.Build.props') -Raw)).Project.PropertyGroup.Version
$packageRoot = Join-Path $projectRoot "artifacts/packages/ReBear-$version-$stamp"
$destination = Join-Path $packageRoot 'ReBear'
$output = Join-Path $projectRoot 'bin'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$files = @(
    'ReBear.gha', 'ReBear.deps.json', 'ReBear.runtimeconfig.json',
    'ConcreteSectionChecker.Adapters.dll', 'MathNet.Numerics.dll',
    'GPCUtilities.dll', 'GPCGeometry.dll', 'DelaunayMesh.dll',
    'GPCModel.dll', 'GPCModelData.dll', 'GPCChecker.Concrete.dll'
)
foreach ($file in $files) { Copy-Item -LiteralPath (Join-Path $output $file) -Destination $destination }
Copy-Item -LiteralPath (Join-Path $projectRoot 'lib/GPC/release-set.json') -Destination $destination
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination $destination
Copy-Item -LiteralPath (Join-Path $projectRoot 'docs') -Destination $destination -Recurse
Copy-Item -LiteralPath (Join-Path $projectRoot 'assets') -Destination $destination -Recurse
$hashes = Get-ChildItem -LiteralPath $destination -File | ForEach-Object {
    [ordered]@{ file = $_.Name; sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
}
$manifest = [ordered]@{ product = 'ReBear'; version = $version; created = (Get-Date -Format o); files = @($hashes) }
$manifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $destination 'package-manifest.json') -Encoding utf8
Compress-Archive -LiteralPath $destination -DestinationPath "$packageRoot.zip"
Write-Output "Package: $packageRoot.zip"
