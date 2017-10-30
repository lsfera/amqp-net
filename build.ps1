[CmdletBinding()]
Param(
    [String]
    $CakeVersion = '0.23.0',

    [String]
    $Script = 'build.cake',
    
    [ValidateSet('Quiet', 'Minimal', 'Normal', 'Verbose', 'Diagnostic')]
    [String]
    $Verbosity = 'Normal',

    [ValidateSet('Clean', 'Build', 'Test', 'Pack', 'NuGetPush')]
    [String]
    $Target = 'Test',
    
    [ValidateSet('Release', 'Debug')]
    [String]
    $BuildConfiguration = 'Release',

    [ValidateSet('Quiet', 'Minimal', 'Normal', 'Detailed', 'Diagnostic')]
    $BuildVerbosity = 'Minimal'
)

DynamicParam {
    $swVerAttributeCollection = New-Object -Type System.Collections.ObjectModel.Collection[System.Attribute]
    $swVerParamAttribute = New-Object System.Management.Automation.ParameterAttribute
    $swVerParamAttribute.ParameterSetName = "__AllParameterSets"
    if ($Target -eq 'Publish') {
        $swVerParamAttribute.Mandatory = $true
    }
    else {
        $swVerParamAttribute.Mandatory = $false
    }
    $swVerAttributeCollection.Add($swVerParamAttribute)
    $swVerDynamicParam = New-Object -Type System.Management.Automation.RuntimeDefinedParameter("SoftwareVersion", [String], $swVerAttributeCollection)

    $buildNumberDynamicParam = New-Object -Type System.Management.Automation.RuntimeDefinedParameter("BuildNumber", [Int32], $null)

    $paramDictionary = New-Object -Type System.Management.Automation.RuntimeDefinedParameterDictionary
    $paramDictionary.Add('SoftwareVersion', $swVerDynamicParam)
    $paramDictionary.Add('BuildNumber', $buildNumberDynamicParam)

    return $paramDictionary
}

Begin {
    $toolsDir = Join-Path $PSScriptRoot 'tools'
    $toolsProj = Join-Path $toolsDir 'project.csproj'
    $cakeFeed = 'https://www.myget.org/F/cake/api/v3/index.json'
}

Process {
    if (-not $PSScriptRoot) {
        $PSScriptRoot = Split-Path $MyInvocation.MyCommand.Path -Parent
    }

    $cakeDll = "$toolsDir/cake.coreclr/$CakeVersion/Cake.dll"
    if (-not (Test-Path $cakeDll)) {
        Write-Host 'Setting up Cake...'

        New-Item -Type Directory -Path $toolsDir -Force | Out-Null
        
        $contents = '<Project Sdk="Microsoft.NET.Sdk">' + `
            '<PropertyGroup><TargetFramework>netstandard1.5</TargetFramework></PropertyGroup>' + `
            "<ItemGroup><PackageReference Include=`"Cake.CoreCLR`" Version=`"$CakeVersion`" /></ItemGroup>"  + `
            '</Project>'
        
        Out-File -InputObject $contents -FilePath $toolsProj
        dotnet restore "$toolsProj" --packages "$toolsDir" --source "$cakeFeed"

        if (!(Test-Path $cakeDll)) {
            Throw "Cannot find Cake assembly '$cakeDll'"
            exit 1
        }
    }

    $softwareVersion = $PSBoundParameters['SoftwareVersion']
    $buildNumber = if (-not $PSBoundParameters['BuildNumber'].IsSet) { 0 } else { $PSBoundParameters['BuildNumber'] }
    $commitHash = $(git rev-parse --short HEAD).trimEnd('\n')

    & dotnet @(
        "$cakeDll",
        "$Script",
        "-verbosity=$Verbosity",
        "-target=$Target",
        "-buildConfiguration=$BuildConfiguration",
        "-buildVerbosity=$BuildVerbosity",
        "-softwareVersion=$softwareVersion",
        "-buildNumber=$buildNumber",
        "-commitHash=$commitHash",
        "-nuGetApiKey=$nuGetApiKey"
    )
}