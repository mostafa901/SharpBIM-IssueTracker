param (
    [Parameter()]
    [int] $cleanOnly = 0,
    [int] $build = 0,
    [int] $Protect = 0,
    [int] $justPack = 1,
    [int] $CommmitImages = 0,
    [int] $publish = 0,
    [int] $updateNuget = 0,
    [int] $NuNugetCore = 0

)
Set-Location $PSScriptRoot

. "..\..\visualstudio-settings\PowerShellLibrary.ps1"

# Get all bound parameters
$boundParams = $PSBoundParameters

# Check if no parameters are provided
if ($boundParams.Count -eq 0) {
    Write-Output "No parameters were provided, using default values."
    Write-Output ""
    
    # Get all script parameters dynamically
    $params = (Get-Command -Name $PSCommandPath).Parameters
    
    $index = 1
    foreach ($paramName in $params.Keys) {
        # Skip common parameters like Verbose, Debug, etc.
        if ($paramName -notin @('Verbose', 'Debug', 'ErrorAction', 'WarningAction', 'InformationAction', 'ErrorVariable', 'WarningVariable', 'InformationVariable', 'OutVariable', 'OutBuffer', 'PipelineVariable')) {
            $value = Get-Variable -Name $paramName -ValueOnly -ErrorAction SilentlyContinue
            Write-Output "[$index]: $paramName : $value"
            $index++
        }
    }
    
    exit
}

if ($updateNuget -eq 1) {
    CopyData ..\..\SharpBIM\Sharp_Nugets\*-win* .\nugets\
    dotnet restore
}

# Change the culture to invariant (this uses '.' as the decimal separator)
$originalCulture = [System.Globalization.CultureInfo]::CurrentCulture
[System.Globalization.CultureInfo]::CurrentCulture = [System.Globalization.CultureInfo]::InvariantCulture

 

if ($cleanOnly -eq 1 ) {
    clean
    & "msbuild" .\SharpBim.GitTracker\SharpBim.GitTracker.csproj /p:Configuration=dwin /t:Restore -clp:Summary`;ErrorsOnly
    if ($build -eq 0) {
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
 $newVersion =""
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
            $Global:oldversion = "$($versionParts[0]).$($versionParts[1]).0.0"
            $minorVersion += 1
            $Global:newversion = "$($versionParts[0]).$($minorVersion).0.0"

            # Construct the new version
            $newVersion = "$($versionParts[0]).$([math]::Round($minorVersion, 3))"
            $was = $currentVersion
            $now = $newVersion
            # Replace the old version with the new version
            $_ -replace $currentVersion, $newVersion

            $updated = $true
        }
        else {
            # If no version is found, leave the line unchanged
            $_
        }

        return $newversion
    }

    if ($updated -eq $true) {
        # Write the updated content back to the file
        $newfileContent | Set-Content -Path $filePath

        Write-Host "Version updated successfully! from $was to $now"
    }
}

# update Versions

function updateVersions ( $folderPath , $newVersion) {
    # Define variables
    #$newVersion = "1.2.3.5"  # Set the new version
    #$newPlatformValue = "NewPlatformValue"  # Set the new value to replace "SA" or "Rvt2024"

    # Define the regex patterns
    $versionPattern = "\d+\.\d+\.\d+\.\d+"
    $csProjPattern = "<AssemblyVersion>$($versionPattern)"  # Matches "SA" or "Rvt" followed by 4 digits

    # Get all relevant files
    $files = Get-ChildItem -Path $folderPath -Recurse -Include *.props, *.xaml, *.cs, *.csproj -File

    foreach ($file in $files) {
        if ($file.Directory.FullName -match "bin" -or $file.Directory.FullName -match "obj") {
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
                $fileContent = $fileContent -replace $csProjPattern, ("<AssemblyVersion>" + $newVersion)
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

function updateRelease() {
    $xmlFile = ".\SharpBim.GitTracker\source.extension.vsixmanifest"  # Update with the correct file path
    # Read the file
    $content = Get-Content $xmlFile -Raw

    $vers = $NewVersion -split '\.'
    $tagVersion = "$($vers[0]).$($vers[1])"

    # Update Version attribute in Identity element
    $versionPattern = '(<Identity\s+[^>]*Version=")[^"]+(")'
    $content = $content -replace $versionPattern, "`${1}$tagVersion`$2"

    # Update ReleaseNotes URL (assuming format: /releases/tag/X.X.X)
    # Extract just the numeric part for the tag (0.8.5.0 -> 0.805)
    $releaseNotesPattern = '(<ReleaseNotes>https://github\.com/[^/]+/[^/]+/releases/tag/)[^<]+(<)'
    $content = $content -replace $releaseNotesPattern, "`${1}$tagVersion`$2"

    # Write back
    Set-Content -Path $xmlFile -Value $content -NoNewline

    Write-Host "✓ Updated Version to: $NewVersion" -ForegroundColor Green
    Write-Host "✓ Updated ReleaseNotes tag to: $tagVersion" -ForegroundColor Green
}


# Build Project
if ($build -eq 1) {
    if ($justPack -eq 1) {
        # $versionRegex = '" Version="(\d+\.\d+)"'
        # Increment $versionRegex ".\SharpBim.GitTracker\source.extension.vsixmanifest" 1
        $Global:newversion = (Get-Item "D:\RevitApi\Shared\Study\SharpBim.Git\SharpBim.GitTracker\bin\RWin\SharpBIM.GitTracker.Core.dll").VersionInfo.FileVersion
        $folderPath = $PSScriptRoot  # Set the folder path
        updateVersions  $folderPath  $Global:newversion
        
        updateRelease
    }

    clean
    
    buildframework2 .\SharpBim.GitTracker\SharpBim.GitTracker.csproj Rwin

    IsAllGood "Building project"
}

if ($Protect -eq 1) {
    $TargetDir = "D:\RevitApi\Shared\Study\SharpBim.Git\SharpBim.GitTracker\bin\RWin"

    if ((IsObfuscated "$TargetDir\SharpBIM.dll") -eq $false) {
        write-host "Failed Obfuscating SharpBIM.dll"
        exit
    }
    
    $tempDir = [System.IO.Path]::GetTempPath()
    $fileName = "SharpBIM.GitTracker.Core.dll"
    $tempFile = "$tempDir\$fileName"
    Delete $tempFile
    
    $targetPath = "$TargetDir\$fileName"

    CopyData2 $targetPath $tempDir
    note "protecting $targetPath"
  
    & "D:\Program Files (x86)\Eziriz\.NET Reactor\dotNET_Reactor.Console.exe" -project "D:\RevitApi\Shared\Study\SharpBim.Git\SharpBim.GitTracker.nrproj"
}   

if ($justPack -eq 1) {
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
        if (Test-Path $removeFilePath) {
`
                Remove-Item $removeFilePath -Force
            Write-Host "Deleted: $file"
        }
        else {
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
    #   cmd /c "`"$env:Signtool`" sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /sha1 $env:SIGN_CERT_HASH `"$newFilePath`""
    & $env:Signtool sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /sha1 $env:SIGN_CERT_HASH $newFilePath
    IsAllGood

    # Step 5: Repack the VSIX
    Delete $newVsixPath
    [System.IO.Compression.ZipFile]::CreateFromDirectory($extractPath, $newVsixPath)

    Write-Host "VSIX file modified and repacked successfully!"
}

function CommitImages {
    Write-Host "Committing Images"
    Copy-Item .\SharpBim.GitTracker\OverView.md .\SharpBim.GitTracker\README.md
    # Copy-Item .\SharpBim.GitTracker\Images\*.*  .\SharpBim.GitTracker\Images -Force
    git -C .\ add .
    git -C .\ commit -m "Updated Images"
    git -C .\ push origin main-code -f
}

if ($CommmitImages -eq 1 -and $publish -eq 0 ) {
    CommitImages
}

if ($publish -eq 1) {
    CommitImages
    
    dotnet build .\SharpBIM.GitTracker.Console\SharpBIM.GitTracker.Console.csproj -c dnowin
    Write-Host "Updating Release"
    & .\SharpBIM.GitTracker.Console\bin\Debug\net48\SharpBIM.GitTracker.Console.exe
    
    IsAllGood "update release"

    Write-Host "Publishing..."
    
    & "C:\Program Files\Microsoft Visual Studio\2022\Community\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe" publish -payload ".\SharpBim.GitTracker\GitPublish\SharpBim.GitTracker.vsix" -publishManifest ".\SharpBim.GitTracker\jsonmainfest.json" -ignoreWarnings "VSIXValidatorWarning01,VSIXValidatorWarning02" -personalAccessToken $env:vsMarketToken
    Write-Host "Finished publish"
}

if ($NuNugetCore -eq 1) { 
    foreach ($conf in $confs) {
        $confDir = $conf.Substring(1)

        if ($conf.ToString().StartsWith("R", [System.StringComparison]::OrdinalIgnoreCase)) {
            if ($Protect -eq 0) {
                IsObfuscated .\Publish\$conf\net47\SharpBIM.dll
                IsObfuscated .\Publish\$conf\net472\SharpBIM.dll
                IsObfuscated .\Publish\$conf\net48\SharpBIM.dll
                if ($conf -eq "rwin") {
                    IsObfuscated .\Publish\$conf\net8.0-windows\SharpBIM.dll
                    IsObfuscated .\Publish\$conf\net8.0-windows8\SharpBIM.dll
                }
            }
        }
        else {
            Copy-Item "D:\RevitApi\Shared\SharpBIM\Publish\$conf\*" "D:\RevitApi\Shared\SharpBIM\Publish\R$confDir\" -Force -Recurse
        }

        $version = (Get-Item "publish\$conf\net48\SharpBIM.dll").VersionInfo.FileVersion

        $nuspecFileName = "R$($confDir)_SharpBIM.nuspec"
        (Get-Content .\$($nuspecFileName)) -replace "<version>.*?</version>", "<version>$newversion</version>" | Set-Content .\$($nuspecFileName)
        (Get-Content .\$($nuspecFileName)) -replace "<id>.*?</id>", "<id>SharpBIM-$confDir</id>" | Set-Content $($nuspecFileName)

        & "D:\RevitApi\Shared\Lib\Compiled\nuget.exe" pack .\$nuspecFileName -OutputDirectory .\Sharp_Nugets
    }
}