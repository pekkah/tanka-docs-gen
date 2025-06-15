param (
    [string]$Output = "./artifacts/gh-pages",
    [string]$CurrentBranch = $Env:GITHUB_REF_NAME
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

if ((Test-Path $Output) -eq $True) {
    "Clean: $Output"
    Remove-Item -Recurse -Force $Output
}

# Ensure output directory exists
if (-not (Test-Path $Output)) {
    New-Item -ItemType Directory -Path $Output | Out-Null
}

# Git Information
"----------------------------------------"
if ($CurrentBranch -eq '') {
    $CurrentBranch = git branch --show-current | Out-String
    EnsureLastExitCode("git branch --show-current failed")
}

$CurrentBranch = $CurrentBranch.Trim()

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

"----------------------------------------"
"Docs"
$DocsOutput = $Output
$Basepath = "/tanka-docs-gen/"


"Output: $DocsOutput"
"BasePath: $Basepath"

"Publishing DocsTool..."
dotnet publish ./src/DocsTool --output ./temp/DocsTool
EnsureLastExitCode("dotnet publish DocsTool failed")

"----------------------------------------"
"Inspecting checkout directory before running DocsTool..."
$checkoutPath = Get-Location
Write-Host "Current directory (should be repo root): $checkoutPath"
Write-Host "Listing contents of $checkoutPath:"
Get-ChildItem -Path $checkoutPath -Force | ForEach-Object { Write-Host "  $($_.Mode) $($_.Name)" }

$gitPath = Join-Path $checkoutPath ".git"
Write-Host "Checking for .git directory at: $gitPath"
if (Test-Path $gitPath) {
    Write-Host ".git directory EXISTS."
    Write-Host "Listing contents of .git directory:"
    Get-ChildItem -Path $gitPath -Recurse -Force | ForEach-Object { Write-Host "  $($_.Mode) $($_.FullName.Replace($checkoutPath, ''))" }
} else {
    Write-Host ".git directory DOES NOT EXIST."
}
Write-Host "Finished inspecting checkout directory."
"----------------------------------------"

"Running published DocsTool..."
dotnet ./temp/DocsTool/Tanka.DocsGen.dll build --output $DocsOutput --base $Basepath
EnsureLastExitCode("DocsTool execution failed")

"----------------------------------------"
"DONE"
Set-Location $Location
