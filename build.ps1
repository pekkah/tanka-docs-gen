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

# Prepare UI Bundle
"----------------------------------------"
"Creating UI bundle zip"
$UiBundlePath = ".\ui-bundle"
$ZipPath = ".\src\DocsTool\Resources\ui-bundle.zip"

# Ensure Resources directory exists
$ResourcesDir = Split-Path $ZipPath -Parent
if (!(Test-Path $ResourcesDir)) {
    New-Item -ItemType Directory -Path $ResourcesDir -Force | Out-Null
}

# Remove existing zip if present
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

# Create zip archive
if (Test-Path $UiBundlePath) {
    # Get all items in ui-bundle directory and compress them (not the directory itself)
    $UiBundleItems = Get-ChildItem -Path $UiBundlePath -Recurse
    if ($UiBundleItems.Count -gt 0) {
        Compress-Archive -Path "$UiBundlePath\*" -DestinationPath $ZipPath -CompressionLevel Optimal -Force
        "✓ UI bundle zipped to $ZipPath"
    } else {
        Write-Warning "UI bundle directory is empty at $UiBundlePath"
    }
} else {
    Write-Warning "UI bundle directory not found at $UiBundlePath"
}

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
