[CmdletBinding()]
Param(
    [String]
    $CakeVersion = '0.23.0',

    [String]
    $Script = 'build.cake',

    [ValidateSet('Quiet', 'Minimal', 'Normal', 'Verbose', 'Diagnostic')]
    [String]
    $Verbosity = 'Normal',

    [Parameter(ValueFromRemainingArguments=$true)]
    [string[]]
    $Args
)

$rootDir = $PSScriptRoot
$toolsDir = Join-Path $rootDir 'tools'
$cakeDll = "$toolsDir\cake.coreclr\$CakeVersion\Cake.dll"

if (-not (Test-Path $cakeDll)) {
    Write-Host 'Setting up Cake...'

    New-Item -Type Directory -Path $toolsDir -Force | Out-Null

    $contents = '<Project Sdk="Microsoft.NET.Sdk">' + `
        '<PropertyGroup><TargetFramework>netstandard1.5</TargetFramework></PropertyGroup>' + `
        "<ItemGroup><PackageReference Include=`"Cake.CoreCLR`" Version=`"$CakeVersion`" /></ItemGroup>"  + `
        '</Project>'

    $toolsProj = Join-Path $toolsDir 'project.csproj'
    $cakeFeed = 'https://www.myget.org/F/cake/api/v3/index.json'
    Out-File -InputObject $contents -FilePath $toolsProj
    dotnet restore "$toolsProj" --packages "$toolsDir" --source "$cakeFeed"

    if (!(Test-Path $cakeDll)) {
        Throw "Cannot find Cake assembly '$cakeDll'"
        exit 1
    }
}

& dotnet "$cakeDll" "$Script" "-verbosity=$Verbosity" $Args "-commitHash=$(git rev-parse --short HEAD)"