param (
    [Parameter(Mandatory=$true)][string]$url,
    [Parameter(Mandatory=$true)][string]$outputDestination,
    [Parameter(Mandatory=$true)][string]$filename,
    [Switch]$skipDownload
)

Add-Type -assemblyname 'System.IO.Compression.FileSystem' #Used for ZipFile.ExtractToDirectoy

# Reference a custom commandlet that allows a synchronous delete
. "$PSScriptRoot\removeFileSystemItemSynchronous.ps1"


# Clean the destination directory, if necessary
# Note that recursively removely files/directories is inherently asynchronous at the Windows API level,
# so using a custom commandlet instead of the native Remove-Item -Recurse,
# which is both unreliable (may fail) and a bit misleading (even after it returns, we don't know for how much longer deletes will still be pending)
if (Test-Path $outputDestination) {
    Remove-FileSystemItem $outputDestination -Recurse
}


$downloadDir = "$PSScriptRoot\Download"

#Download the latest build (if necessary)
$downloadedZipFilePath = "$($downloadDir)\$($filename)"
If (-NOT $skipDownload -OR -Not (Test-Path -Path $downloadedZipFilePath -PathType Leaf)) {
    # Yep, need to download
    Write-Host "Downloading to: $($downloadedZipFilePath)"

    # Make sure the download directory exists
    New-Item -ItemType Directory -Force -Path $downloadDir | Out-Null

    $webClient = New-Object System.Net.WebClient
    $webClient.DownloadFile($url, $downloadedZipFilePath)
} else {
    Write-Host "Re-using previous download: $($downloadedZipFilePath)";
}

# Unzip
[System.IO.Compression.ZipFile]::ExtractToDirectory($downloadedZipFilePath, "$($outputDestination)")