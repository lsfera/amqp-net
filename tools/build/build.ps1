[CmdletBinding()]
Param(
    [String]
    $CakeVersion = '0.23.0',

    [String]
    $CakeFile = (Join-Path $PSScriptRoot 'build.cake'),

    [ValidateSet('Quiet', 'Minimal', 'Normal', 'Verbose', 'Diagnostic')]
    [String]
    $Verbosity = 'Normal',

    [Parameter(ValueFromRemainingArguments=$true)]
    [string[]]
    $Args
)

$rootDir = $PSScriptRoot
while ($(Get-ChildItem -Force -Filter '.git' $rootDir | Measure-Object).Count -eq 0){
    $rootDir = Join-Path $rootDir '..'
}
$srcDir = Join-Path $rootDir 'src'
$toolsDir = Join-Path $rootDir 'tools'
$cakeDir = Join-Path $toolsDir (Join-Path 'build' 'cake')
$cakeDll = "$cakeDir\cake.coreclr\$CakeVersion\Cake.dll"

if (-not (Test-Path $cakeDll)) {
    Write-Host 'Setting up Cake...'

    New-Item -Type Directory -Path $cakeDir -Force | Out-Null

    $contents = '<Project Sdk="Microsoft.NET.Sdk">' + `
        '<PropertyGroup><TargetFramework>netstandard1.5</TargetFramework></PropertyGroup>' + `
        "<ItemGroup><PackageReference Include=`"Cake.CoreCLR`" Version=`"$CakeVersion`" /></ItemGroup>"  + `
        '</Project>'

    $cakeProj = Join-Path $cakeDir 'project.csproj'
    Out-File -InputObject $contents -FilePath $cakeProj
    dotnet restore "$cakeProj" --packages "$cakeDir"

    if (!(Test-Path $cakeDll)) {
        Throw "Cannot find Cake assembly '$cakeDll'"
        exit 1
    }
}

& dotnet "$cakeDll" "$CakeFile" "-verbosity=$Verbosity" "-srcDir=$srcDir" "-toolsDir=$toolsDir" "-commitHash=$(git rev-parse --short HEAD)" $Args