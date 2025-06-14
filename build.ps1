param (
    [string]$Output = "./artifacts",
    [string]$CurrentBranch ='',
    [bool]$OnlyBuild = $False
 )

# Utils
function EnsureLastExitCode($message){
    if ($LASTEXITCODE -ne 0) {
        throw $message
    } 
}

# Parameters
"----------------------------------------"
"Output: $Output"
$Location = Get-Location
"Location: $Location"
"OnlyBuild: $OnlyBuild"

if ((Test-Path $Output) -eq $True) {
    "Clean: $Output"
    Remove-Item -Recurse -Force $Output
}

# Ensure output directory exists and get absolute path
if (-not (Test-Path $Output)) {
    New-Item -ItemType Directory -Path $Output | Out-Null
}

$ResultsDir = (Resolve-Path $Output).Path

# Git Information
if ($CurrentBranch -eq '') {
    $CurrentBranch = git branch --show-current | Out-String
    EnsureLastExitCode("git branch --show-current failed")
}

$CurrentBranch = $CurrentBranch.Trim()

"----------------------------------------"
"CurrentBranch: $CurrentBranch"

if ($CurrentBranch -eq '') {
    Write-Error "Not branch or tag"
    return
}

# Install tools
"----------------------------------------"
"Restoring dotnet tools"
dotnet tool restore

EnsureLastExitCode("Restore failed")

# Get GitVersion
"----------------------------------------"
"Getting GitVersion"
$Version = dotnet gitversion /output json /showvariable SemVer | Out-String -NoNewline
EnsureLastExitCode("Could not get SemVer from gitversion")
$Version = $Version.Trim()
"Git version: '$Version'"
"##vso[build.updatebuildnumber]$Version"

$PreReleaseTag = dotnet gitversion /output json /showvariable PreReleaseTag | Out-String -NoNewline
EnsureLastExitCode("Could not get PrReleaseTag from gitversion")
$PreReleaseTag = $PreReleaseTag.Trim()
$IsPreRelease = $PreReleaseTag -ne ''
"PreReleaseTag: $PreReleaseTag, IsPreRelease: $IsPreRelease"

# Build and test
"----------------------------------------"
"Build"
dotnet restore
dotnet build -c Release --no-restore
EnsureLastExitCode("dotnet build failed")

if ($OnlyBuild -eq $False) {
    "----------------------------------------"
    "Run tests"
    dotnet test -c Release --logger trx --results-directory $ResultsDir --no-restore --no-build
    EnsureLastExitCode("dotnet test failed")

    "----------------------------------------"
    "Pack NuGet"
    dotnet pack -c Release -o $Output -p:Version=$Version -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
    EnsureLastExitCode("dotnet pack failed")

    Set-Location $Location

}
"----------------------------------------"
"DONE"
Set-Location $Location
