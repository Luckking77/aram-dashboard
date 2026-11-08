$ErrorActionPreference = 'Stop'
$fw = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$wpf = Join-Path $fw 'WPF'
$csc = Join-Path $fw 'csc.exe'
$out = Join-Path $PSScriptRoot 'AramDashboard.exe'
$refs = @(
    (Join-Path $fw 'System.dll'),
    (Join-Path $fw 'System.Core.dll'),
    (Join-Path $fw 'System.Xaml.dll'),
    (Join-Path $fw 'System.Web.Extensions.dll'),
    (Join-Path $fw 'System.Net.Http.dll'),
    (Join-Path $wpf 'PresentationFramework.dll'),
    (Join-Path $wpf 'PresentationCore.dll'),
    (Join-Path $wpf 'WindowsBase.dll')
)
$sources = Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.cs' | ForEach-Object { $_.FullName }
$args = @('/nologo','/target:winexe','/platform:anycpu','/langversion:5','/optimize+',('/out:' + $out))
foreach ($ref in $refs) { $args += ('/reference:' + $ref) }
foreach ($src in $sources) { $args += $src }
& $csc @args
if ($LASTEXITCODE -ne 0) { throw "csc failed: $LASTEXITCODE" }
Write-Host "Built: $out"


