param (
    [Parameter()]
    [int] $cleanOnly = 0,
    [int] $build = 0,
    [int] $Protect = 0,
    [int] $justPack = 0,
    [int] $CommmitImages = 0,
    [int] $publish = 0,
    [int] $updateNuget = 0

)
Set-Location $PSScriptRoot

. "..\..\visualstudio-settings\PowerShellLibrary.ps1"

 # Check if no parameters are provided (i.e., they are using the default values)
    if ($justPack -eq 0 -and $cleanOnly -eq 0 -and $build -eq 0 -and $Protect -eq 0 -and $CommmitImages -eq 0 -and $publish -eq 0 -and $updateNuget -eq 0) {
        Write-Output "No parameters were provided, using default values."
         # Output the parameter values
         Write-Output "[1]: cleanOnly: $cleanOnly"
         Write-Output "[2]: build: $build"
         Write-Output "[3]: Protect: $Protect"
         Write-Output "[4]: JustPack: $justPack"
         Write-Output "[5]: CommitImages: $CommmitImages"
         Write-Output "[6]: publish: $publish"
         Write-Output "[7]: Update SharpBIM nuget: $updateNuget"
        exit
    } 

if($updateNuget -eq 1)
{
    CopyData ..\..\SharpBIM\Sharp_Nugets\*-win* .\nugets\
    dotnet restore
}

# Change the culture to invariant (this uses '.' as the decimal separator)
$originalCulture = [System.Globalization.CultureInfo]::CurrentCulture
[System.Globalization.CultureInfo]::CurrentCulture = [System.Globalization.CultureInfo]::InvariantCulture

 

if($cleanOnly -eq 1 )
{
    clean
    if($build -eq 0)
    {
        Exit
    }
}

Add-Type -AssemblyName "System.IO.Compression.FileSystem"

$Global:oldversion = "0.0"
$Global:newversion = "0.0"

function Increment ($versionRegex, $filePath, $verpos) {
    # Define the path to the file where the version needs to be updated
#$filePath = ".\SharpBIM.GitTracker\source.extension.vsixmanifest"

# Read the content of the file
$fileContent = Get-Content -Path $filePath

# Define a regular expression to match Version="x.xxx"
#$versionRegex = '" Version="(\d+\.\d+)"'

    $was = 0
    $now = 0

    $updated = $false
    # Split the content into individual lines and process each line
    $newfileContent = $fileContent -Split "`r?`n" | ForEach-Object {
    if ($_ -eq "") { return }  # Skips empty lines

    if ($_ -match $versionRegex) {
        # Extract the current version
        $currentVersion = $matches[1]

        # Split the version into major and minor parts
        $versionParts = $currentVersion -split '\.'

        # Increment the last part of the version
        $minorVersion = [decimal]::Parse($versionParts[$verpos])
        $Global:oldversion="$($versionParts[0]).$($versionParts[1]).0.0"
        $minorVersion += 1
        $Global:newversion="$($versionParts[0]).$($minorVersion).0.0"

        # Construct the new version
        $newVersion = "$($versionParts[0]).$([math]::Round($minorVersion, 3))"
        $was = $currentVersion
        $now = $newVersion
        # Replace the old version with the new version
        $_ -replace $currentVersion, $newVersion

    $updated = $true
    } else {
        # If no version is found, leave the line unchanged
        $_
    }
}

if ($updated -eq $true)
{
# Write the updated content back to the file
$newfileContent | Set-Content -Path $filePath

Write-Host "Version updated successfully! from $was to $now"
}
}

# update Versions

function updateVersions ($newVersion)
{
    # Define variables
    $folderPath = $PSScriptRoot  # Set the folder path
    #$newVersion = "1.2.3.5"  # Set the new version
    #$newPlatformValue = "NewPlatformValue"  # Set the new value to replace "SA" or "Rvt2024"

    # Define the regex patterns
    $versionPattern = "\d+\.\d+\.\d+\.\d+"
    $csProjPattern = "<AssemblyVersion>$($versionPattern)"  # Matches "SA" or "Rvt" followed by 4 digits

    # Get all relevant files
    $files = Get-ChildItem -Path $folderPath -Recurse -Include *.props, *.xaml, *.cs, *.csproj -File

    foreach ($file in $files) {
    if($file.Directory.FullName -match "bin" -or $file.Directory.FullName -match "obj")
    {
        continue
    }
        # Read the content of the file
        $fileContent = Get-Content -Path $file.FullName -Raw -Encoding UTF8

        # Replace version pattern

        [bool]$updated = $false;

        if ($fileContent -match $versionPattern -and $file.Basename -eq "AssemblyInfo") {
            $fileContent = $fileContent -replace $versionPattern, $newVersion
            Write-Output "Updated version in: $($file.FullName)"
        $updated = $true
        }

        # Replace in csproject
        if ($file.Extension -eq ".csproj" -or $file.Extension -eq ".props") {
            if ($fileContent -cmatch $csProjPattern) {
                $fileContent = $fileContent -replace $csProjPattern, ("<AssemblyVersion>"+$newVersion)
                Write-Output "Updated platform value in: $($file.FullName)"
        $updated = $true
            }
        }

        # Write changes back to the file if there were updates
        if ($updated -eq $true) {
            Set-Content -Path $file.FullName -Value $fileContent.TrimEnd() -Encoding UTF8
        }
    }
}

function updateRelease()
{
    $xmlFile = ".\SharpBim.GitTracker\source.extension.vsixmanifest"  # Update with the correct file path
    $xmlContent = Get-Content -Path $xmlFile -Raw

    $pattern = '(?<=<ReleaseNotes>https:\/\/github\.com\/mostafa901\/SharpBIM-IssueTracker\/releases\/tag\/)([\d\.]+)(?=<\/ReleaseNotes>)'
    $versionParts = $Global:newversion -split '\.'
    $replacement = "$($versionParts[0]).$($versionParts[1])"

    $updatedXmlContent = [regex]::Replace($xmlContent, $pattern, $replacement)

    # Save the updated content back to the file
    $updatedXmlContent | Set-Content -Path $xmlFile -Encoding UTF8

    Write-Output "Version updated to $replacement in $xmlFile"
}

# Build Project
if($build -eq 1)`
{
    if($justPack -eq 1)
    {
        $versionRegex = '" Version="(\d+\.\d+)"'
        Increment $versionRegex ".\SharpBim.GitTracker\source.extension.vsixmanifest" 1
        updateVersions $Global:newversion
       # updateRelease
    }


    #dotnet nuget remove .\SharpBim.GitTracker.Core/SharpBIM.GitTracker.Core.csproj package SharpBIM-win
  #  dotnet nuget update .\SharpBim.GitTracker.Core\SharpBIM.GitTracker.Core.csproj package SharpBIM-win --source ..\..\SharpBIM\Sharp_Nugets\

    clean

  buildframework2 .\SharpBim.GitTracker.sln Rwin

  IsAllGood "Building project"
}

if($Protect -eq 1)
{
    $TargetDir ="D:\RevitApi\Shared\Study\SharpBim.Git\SharpBim.GitTracker\bin\RWin"

    if((IsObfuscated "$TargetDir\SharpBIM.dll") -eq $false)
    {
        write-host "Failed Obfuscating SharpBIM.dll"
        exit
    }
    
    $tempDir = [System.IO.Path]::GetTempPath()
    $fileName ="SharpBIM.GitTracker.Core.dll"
    $tempFile = "$tempDir\$fileName"
    Delete $tempFile
    
    $targetPath = "$TargetDir\$fileName"

    CopyData2 $targetPath $tempDir
    note "protecting $targetPath"
  
    & "D:\Program Files (x86)\Eziriz\.NET Reactor\dotNET_Reactor.Console.exe" -project "D:\RevitApi\Shared\Study\SharpBim.Git\SharpBim.GitTracker.nrproj"
}

if($justPack -eq 1)
{
    # Define paths
    $vsixPath = "D:\RevitApi\Shared\Study\SharpBim.Git\SharpBim.GitTracker\bin\RWin\SharpBim.GitTracker.vsix"
    $extractPath = "C:\Temp\VSIX_Extracted"
    $newVsixPath = "D:\RevitApi\Shared\Study\SharpBim.Git\SharpBim.GitTracker\GitPublish\SharpBim.GitTracker.vsix"
    #$fileToRemove = "SharpBIM.GitTracker.Core.dll"   # First file to delete (relative to extracted folder)
    #$fileToRemove2 = "SharpBIM.dll"   # Second file to delete (relative to extracted folder)
    $fileToReplace = "SharpBIM.GitTracker.dll"  # File to replace (relative to extracted folder)
    $newFilePath = "$extractPath\SharpBIM.GitTracker.dll"  # Path to the new file
    #$folderToAdd = "D:\RevitApi\Shared\Study\SharpBim.Git\SharpBim.GitTracker\GitPublish\Resources"        # Folder to add
    #$destinationFolder = "$extractPath\Resources"  # Destination inside extracted VSIX

# Step 1: Extract the VSIX using .NET's ZipFile class
Delete $extractPath

if (Test-Path $extractPath) {
    Remove-Item -Recurse -Force $extractPath
}
[System.IO.Compression.ZipFile]::ExtractToDirectory($vsixPath, $extractPath)

# Step 2: Remove the files
$filesToRemove = @()

foreach ($file in $filesToRemove) {
    $removeFilePath = Join-Path $extractPath $file
    if (Test-Path $removeFilePath) {`
        Remove-Item $removeFilePath -Force
        Write-Host "Deleted: $file"
    } else {
        Write-Host "File not found: $file"
    }
}

# Step 3: Remove any .pdb files
$pdbAndConfigFiles = Get-ChildItem -Path $extractPath -Recurse | Where-Object { $_.Extension -in ('.pdb', '.config') }
foreach ($pdb in $pdbFiles) {
    Remove-Item $pdb.FullName -Force
    Write-Host "Deleted: $($pdb.FullName)"
}


# Step 4: Sign the file
    cmd /c "`"$env:Signtool`" sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /sha1 $env:SIGN_CERT_HASH `"$newFilePath`""
    IsAllGood

# Step 5: Repack the VSIX
Delete $newVsixPath
[System.IO.Compression.ZipFile]::CreateFromDirectory($extractPath, $newVsixPath)

Write-Host "VSIX file modified and repacked successfully!"
}

function CommitImages
{
    Write-Host "Committing Images"
    Copy-Item .\SharpBim.GitTracker\OverView.md .\SharpBim.GitTracker\README.md
   # Copy-Item .\SharpBim.GitTracker\Images\*.*  .\SharpBim.GitTracker\Images -Force
    git -C .\ add .
    git -C .\ commit -m "Updated Images"
    git -C .\ push origin main-code -f
}

if($CommmitImages -eq 1 -and $publish -eq 0 )
{
    CommitImages
}

if($publish -eq 1)
{
    CommitImages
   
    Write-Host "Updating Release"
    & .\SharpBIM.GitTracker.Console\bin\Debug\net48\SharpBIM.GitTracker.Console.exe
    
    IsAllGood "update release"

    Write-Host "Publishing..."
    
    & "C:\Program Files\Microsoft Visual Studio\2022\Professional\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe" publish -payload ".\SharpBim.GitTracker\GitPublish\SharpBim.GitTracker.vsix" -publishManifest ".\SharpBim.GitTracker\jsonmainfest.json" -ignoreWarnings "VSIXValidatorWarning01,VSIXValidatorWarning02" -personalAccessToken $env:vsMarketToken
    Write-Host "Finished publish"
}