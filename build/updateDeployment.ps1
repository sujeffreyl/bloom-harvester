param (
    [Switch]$skipDownload
)
$harvestRootDir = "C:\Harvester";
$outputDir= "$($harvestRootDir)\ReleaseNext"
$currentVersionDir ="$($harvestRootDir)\Release"
$backupOfPreviousDir= "$($harvestRootDir)\ReleasePrev"

$VerbosePreference = "Continue";

# Clean the output directory
# Start cleaning sooner rather than later due to timing issues
# (This step can have a small but noticeable delay before it's finished deleting, and I guess Powershell sometimes executes certain commands prematurely,
# So I guess commands can get executed in a funny order?
# If you try to delete the entire directory here, this can cause UnauthorizedAccess exceptions to be thrown
# If just deleting the directory contents, I wonder if it's possible to copy a new file here, then end up accidentally deleting it if things were executed in the wrong order.
#
# To avoid any strange stuff like that, we start deleting this directory long before it's needed)
Remove-Item -Path "$($outputDir)" -Recurse -ErrorAction Ignore
Remove-Item -Path "$($backupOfPreviousDir)" -Recurse -ErrorAction Ignore


$downloadDir = "$PSScriptRoot\Download"

# ENHANCE: Check if we already have the latest version

# Download the latest build (if necessary)
$unzipDestination = "$($downloadDir)\Unzipped"
$command = "$($PSScriptRoot)\downloadAndExtractZip.ps1 -URL https://build.palaso.org/guestAuth/repository/downloadAll/Bloom_HarvesterMasterContinuous/latest.lastSuccessful -Filename harvester.zip -Output $($downloadDir)\Unzipped $(If ($skipDownload) { "-skipDownload"})"
Invoke-Expression $command

# Download/extract/copy dependencies from Bloom Desktop
$dependenciesDir = "$($downloadDir)\UnzippedDependencies"
$command = "$($PSScriptRoot)\downloadAndExtractZip.ps1 -URL https://build.palaso.org/guestAuth/repository/downloadAll/bt222/latest.lastSuccessful -Filename bloom.zip -Output $($dependenciesDir) $(If ($skipDownload) { "-skipDownload"})"
Invoke-Expression $command

# Ensure the output directory exists
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
New-Item -ItemType Directory -Force -Path "$($outputDir)\Firefox" | Out-Null

# Copy from the unzip destination into the correct output structure
Copy-Item "$($unzipDestination)\Release\*" -Destination "$($outputDir)\"
Copy-Item "$($dependenciesDir)\bin\Release\Firefox\*" -Destination "$($outputDir)\Firefox"
Copy-Item "$($dependenciesDir)\output\browser" -Destination "$($outputDir)\" -Recurse
Copy-Item "$($dependenciesDir)\DistFiles" -Destination "$($outputDir)\" -Recurse
Move-Item "$($outputDir)\DistFiles\localization" -Destination "$($outputDir)\"
Write-Host "Setup of Next version done."
Write-Host


# Close current Harvester and RestartOnCrash
Write-Host "Killing any existing Harvester-related processes."
$restartOnCrashPath = "$($harvestRootDir)\RestartOnCrash\RestartOnCrash.exe"
$harvesterExePath = "$($currentVersionDir)\BloomHarvester.exe"
Get-Process -Name "RestartOnCrash" -ErrorAction Ignore | Where-Object { $_.Path -eq $restartOnCrashPath } | Stop-Process -Verbose
Get-Process -Name "BloomHarvester" -ErrorAction Ignore | Where-Object { $_.Path -eq $harvesterExePath } | Stop-Process -Verbose
#Stop-Process -Name "RestartOnCrash" #ENHANCE: It's better to find a process whose path is the expected path, gets its ID, and then kill it by ID
#Stop-Process -Name "BloomHarvester"

Write-Host
Write-Host "Swapping to next version"
Write-Host "Moving current to backup location $($backupOfPreviousDir)";
Copy-Item "$($currentVersionDir)" -Destination "$($backupOfPreviousDir)" -Recurse
Remove-Item -Path "$($currentVersionDir)" -Recurse

# Wait a while to make sure items are fully removed.
Start-Sleep -Seconds 3
Write-Host "Copying new build to current location $($currentVersionDir)";
New-Item -ItemType Directory -Force -Path $currentVersionDir | Out-Null
Copy-Item "$($outputDir)\*" -Destination $currentVersionDir -Recurse -Force


# Check for environment variables
# Enhance: I suppose you could allow an optional parameter which would specify whether it wants dev or prod. And only check the relevant keys.
$anyEnvVarsMissing = $false
$envVarKeys = "BloomBooksS3KeyDev", "BloomBooksS3KeyProd", "BloomBooksS3SecretKeyDev", "BloomBooksS3SecretKeyProd", "BloomHarvesterAzureAppInsightsKeyDev", "BloomHarvesterAzureAppInsightsKeyProd", "BloomHarvesterParseAppIdDev", "BloomHarvesterParseAppIdProd", "BloomHarvesterS3KeyDev", "BloomHarvesterS3KeyProd", "BloomHarvesterS3SecretKeyDev", "BloomHarvesterS3SecretKeyProd", "BloomHarvesterUserName", "BloomHarvesterUserPasswordDev", "BloomHarvesterUserPasswordProd"
ForEach ($key in $envVarKeys) {
    if (-Not (Test-Path "env:$($key)")) {
        Write-Error "Missing environment variable: $key"
        $anyEnvVarsMissing=$true
    }
}

If ($anyEnvVarsMissing) {
    Write-Host "Aborting starting up new Harvester process because some environment variables are missing. Please set the environment variables, (possibly restart the console window/etc), and try again."
    return
}

# Startup RestartOnCrash, or just startup the Harvester directly as a fallback
If ((Test-Path -Path $restartOnCrashPath -PathType Leaf)) {
    Write-Host "Running RestartOnCrash from: $($restartOnCrashPath)"
    Start-Process -FilePath $restartOnCrashPath
} Else {
    Write-Host "RestartOnCrash not found at $($restartOnCrashPath)"
    Write-Host "Starting Harvester"
    Start-Process -FilePath $harvesterExePath -ArgumentList  @("harvest", "--mode=default", "--environment=dev", "--parseDBEnvironment=dev", "--count=1", "--loop")
}

Write-Host "Done"