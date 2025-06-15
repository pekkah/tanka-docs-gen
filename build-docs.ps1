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
$Basepath = "/"


"Output: $DocsOutput"
"BasePath: $Basepath"

"Publishing DocsTool..."
dotnet publish ./src/DocsTool --runtime win-x64 --output ./temp/DocsTool --no-self-contained
EnsureLastExitCode("dotnet publish DocsTool failed")

"Running published DocsTool..."
dotnet ./temp/DocsTool/Tanka.DocsGen.dll build --output $DocsOutput --base $Basepath
EnsureLastExitCode("DocsTool execution failed")

"----------------------------------------"
Write-Host "Listing contents of output directory:"
Get-ChildItem -Path $DocsOutput -Recurse

"DONE"
Set-Location $Location
