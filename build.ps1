#! /usr/bin/env pwsh
param(
    [Parameter(Mandatory = $false)][string] $Configuration = "Release",
    [Parameter(Mandatory = $false)][string] $OutputPath = "",
    [Parameter(Mandatory = $false)][switch] $SkipTests
)

# These make CI builds faster
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "true"
$env:NUGET_XMLDOC_MODE = "skip"

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$solutionPath = Split-Path $MyInvocation.MyCommand.Definition
$sdkFile = Join-Path $solutionPath "global.json"

$dotnetVersion = (Get-Content $sdkFile | Out-String | ConvertFrom-Json).sdk.version

if ($OutputPath -eq "") {
    $OutputPath = Join-Path "$(Convert-Path "$PSScriptRoot")" "artifacts"
}

$installDotNetSdk = $false;

if (($null -eq (Get-Command "dotnet" -ErrorAction SilentlyContinue)) -and ($null -eq (Get-Command "dotnet.exe" -ErrorAction SilentlyContinue))) {
    Write-Host "The .NET SDK is not installed."
    $installDotNetSdk = $true
}
else {
    Try {
        $installedDotNetVersion = (dotnet --version 2>&1 | Out-String).Trim()
    }
    Catch {
        $installedDotNetVersion = "?"
    }

    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Host "The required version of the .NET SDK is not installed. Expected $dotnetVersion."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk -eq $true) {

    $env:DOTNET_INSTALL_DIR = Join-Path "$(Convert-Path "$PSScriptRoot")" ".dotnetcli"
    $sdkPath = Join-Path $env:DOTNET_INSTALL_DIR "sdk" "$dotnetVersion"

    if (!(Test-Path $sdkPath)) {
        if (!(Test-Path $env:DOTNET_INSTALL_DIR)) {
            mkdir $env:DOTNET_INSTALL_DIR | Out-Null
        }
        [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor "Tls12"
        if (($PSVersionTable.PSVersion.Major -ge 6) -And !$IsWindows) {
            $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.sh"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.sh" -OutFile $installScript -UseBasicParsing
            chmod +x $installScript
            & $installScript --version "$dotnetVersion" --install-dir "$env:DOTNET_INSTALL_DIR" --no-path --skip-non-versioned-files
        }
        else {
            $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.ps1"
            Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
            & $installScript -Version "$dotnetVersion" -InstallDir "$env:DOTNET_INSTALL_DIR" -NoPath -SkipNonVersionedFiles
        }
    }
}
else {
    $env:DOTNET_INSTALL_DIR = Split-Path -Path (Get-Command dotnet).Path
}

$dotnet = Join-Path "$env:DOTNET_INSTALL_DIR" "dotnet"

if ($installDotNetSdk -eq $true) {
    $env:PATH = "$env:DOTNET_INSTALL_DIR;$env:PATH"
}

function DotNetTest {
    param([string]$Project)

    & $dotnet test $Project --output $OutputPath

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }

    $nugetPath = $env:NUGET_PACKAGES ?? (Join-Path ($env:USERPROFILE ?? "~") ".nuget" "packages")
    $propsFile = Join-Path $solutionPath "tests" "TodoApp.Tests" "TodoApp.Tests.csproj"
    $reportGeneratorVersion = (Select-Xml -Path $propsFile -XPath "//PackageReference[@Include='ReportGenerator']/@Version").Node.'#text'
    $reportGeneratorPath = Join-Path $nugetPath "reportgenerator" $reportGeneratorVersion "tools" "net6.0" "ReportGenerator.dll"

    $coverageOutput = Join-Path $OutputPath "coverage.cobertura.xml"
    $reportOutput = Join-Path $OutputPath "coverage"

    & $dotnet `
        $reportGeneratorPath `
        `"-reports:$coverageOutput`" `
        `"-targetdir:$reportOutput`" `
        -reporttypes:HTML `
        -verbosity:Warning
}

function DotNetPublish {
    param([string]$Project)

    $publishPath = Join-Path $OutputPath "publish"
    & $dotnet publish $Project --output $publishPath --configuration $Configuration

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

$testProjects = @(
    (Join-Path $solutionPath "tests\TodoApp.Tests\TodoApp.Tests.csproj")
)

$publishProjects = @(
    (Join-Path $solutionPath "src\TodoApp\TodoApp.csproj")
)

Write-Host "Publishing solution..." -ForegroundColor Green
ForEach ($project in $publishProjects) {
    DotNetPublish $project $Configuration
}

if ($SkipTests -eq $false) {
    Write-Host "Testing $($testProjects.Count) project(s)..." -ForegroundColor Green
    ForEach ($project in $testProjects) {
        DotNetTest $project
    }
}
