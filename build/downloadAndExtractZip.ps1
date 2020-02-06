param (
    [Parameter(Mandatory=$true)][string]$url,
    [Parameter(Mandatory=$true)][string]$outputDestination,
    [Parameter(Mandatory=$true)][string]$filename,
    [Switch]$skipDownload
)

Add-Type -assemblyname 'System.IO.Compression.FileSystem' #Used for ZipFile.ExtractToDirectoy


#Clean the destination directory
Remove-Item $outputDestination -Recurse -ErrorAction Ignore


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

    # Give some time for the destination directory to be fully deleted
    Sleep 1
}

# Unzip
[System.IO.Compression.ZipFile]::ExtractToDirectory($downloadedZipFilePath, "$($outputDestination)")

# ENHANCE: At this point, the files are unzipped with the wrong timestamp. I suspect the timestamp is in UTC time,
# (which may be in the future for a lot of people). If we really cared about making it accurate,
# I guess we could update the timestamp of those files right here.