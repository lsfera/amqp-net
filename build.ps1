[CmdletBinding()]
Param(
    [String]
    $CakeVersion = '0.23.0',

    [String]
    $CakeScript = "build.cake",
    
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [String]
    $CakeVerbosity = "Normal",

    [String]
    $CakeTarget = "Test",
    
    [ValidateSet("Release", "Debug")]
    [String]
    $BuildConfiguration = "Release",

    [ValidateSet("Quiet", "Minimal", "Normal", "Detailed", "Diagnostic")]
    $BuildVerbosity = "Minimal"
)

if (-not $PSScriptRoot) {
    $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
}

$TOOLS_DIR = Join-Path $PSScriptRoot 'tools'
$CAKE_DLL = "$TOOLS_DIR/cake.coreclr/$CakeVersion/Cake.dll"
$TOOLS_PROJ = Join-Path $TOOLS_DIR 'project.csproj'
$CAKE_FEED = "https://www.myget.org/F/cake/api/v3/index.json"


if (-not (Test-Path $CAKE_DLL)) {
    Write-Host "Setting up Cake..."

    New-Item -Type Directory -Path $TOOLS_DIR -Force | Out-Null
    
    $contents = "<Project Sdk=`"Microsoft.NET.Sdk`"><PropertyGroup><TargetFramework>netstandard1.5</TargetFramework></PropertyGroup><ItemGroup><PackageReference Include=`"Cake.CoreCLR`" Version=`"$CakeVersion`" /></ItemGroup></Project>"
    Out-File -InputObject $contents -FilePath $TOOLS_PROJ
    dotnet restore "$TOOLS_PROJ" --packages "$TOOLS_DIR" --source "$CAKE_FEED"

    if (!(Test-Path $CAKE_DLL)) {
        Throw "could not find Cake assembly '$CAKE_DLL'"
        exit 1
    }
}

$buildNumber = $(git rev-list HEAD --count).trimEnd('\n')

dotnet "$CAKE_DLL" "$CakeScript" -verbosity="$CakeVerbosity" -target="$CakeTarget" -buildConfiguration="$BuildConfiguration" -buildVerbosity="$BuildVerbosity" -buildNumber="$buildNumber"