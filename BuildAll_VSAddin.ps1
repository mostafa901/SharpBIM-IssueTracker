<# 
.NOTES
    File Name      : PSHelpers.ps1
    Author         : Moustafa K. Elsayed
    Created        : June 15, 2025
    Last Modified  : Feb 17, 2026
    Version        : $Scriptversion
    
    Requirements:
    - PowerShell 7.0 or higher you can run this in any powershell to install powershell 7:  winget install --id Microsoft.Powershell --source winget
    - Git installed and in PATH
    - Valid Git repository

    History log:
    #1.0 Initial $global:Release
 
    #>
    
######### GLOBAL VARIABLES #########
###
# IF THERE IS LOAD ERROR Add WSBLib\WSBLib\SolutionItems to the PSMOdulePath under the enviromentalvariable
###
using module "D:\RevitAPI\Shared\visualstudio-settings\TypicalProps\ProjectBuildingEngine.psm1"
 
[CmdletBinding()]
param (
    [string[]] $Configs = @("Rwin"),
    [bool] $Clean = 0,
    [bool] $Build = 1,
    [bool] $Protect = 0,
    [bool] $All = 1,
    [bool] $IgnoreCheck = 0,
    [bool] $justPack = 0,
    [int] $PublishToServer = 0,
    [bool] $publishToVSMarket = $true
) 
Set-Location $PSScriptRoot
$options = [BuildOptions]::new($Configs, $PSBoundParameters)
$options.BuildFrameworks = @($global:Framework48)
$options.IsDotNetBuild = $false
Set-Location $PSScriptRoot
if ($All) {
    $dep = [DependantProject]::new(
        "SharpBIm",
        "$env:RevitLibPath\ExternalLibraries\SharpBIM\|Config|\|Framework|\SharpBIM.dll",
        "D:\RevitAPI\Shared\SharpBIM\BuildAll.ps1"
    )
    $dep.options = @{
        Configs = "Rwin"
        Build   = 1
        Protect = 1
        Pack    = 1
    }
    $options.Dependants += $dep 
}
$options.Initialize(".\SharpBIM.IssueTracker\SharpBIM.IssueTracker.csproj")
$options.InvokeBuild()

if ($justPack -eq 1 -OR $publishToVSMarket) {
    # Define paths
    $vsixPath =  "$($env:RevitLibPath)\ExternalLibraries\SharpBIM.IssueTracker\Rwin\net48\SharpBIM.IssueTracker.vsix" 
    $extractPath = "C:\Temp\VSIX_Extracted"
    $newVsixPath = "$($env:RevitLibPath)\ExternalLibraries\SharpBIM.IssueTracker\vsix\SharpBIM.IssueTracker.vsix" 

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
                Log_Info "Deleted: $file"
        }
        else {
            Log_Warning "File not found: $file"
        }
    }

    # Step 3: Remove any .pdb files
    $pdbAndConfigFiles = Get-ChildItem -Path $extractPath -Recurse | Where-Object { $_.Extension -in ('.pdb', '.config') }
    foreach ($pdb in $pdbFiles) {
        Remove-Item $pdb.FullName -Force
        Write-Host "Deleted: $($pdb.FullName)"
    }

   # Step 2: Remove the files
    $filesToSign = @(
        Join-Path $extractPath "SharpBIM.IssueTracker.dll"
    )
    foreach ($file in $filesToSign)
    {
        # Step 4: Sign the file
        #   cmd /c "`"$env:Signtool`" sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /sha1 $env:SIGN_CERT_HASH `"$newFilePath`""
        & $env:Signtool sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /sha1 $env:SIGN_CERT_HASH $file
        IsAllGood "Signed file $file"
    }

    # Step 5: Repack the VSIX
    Delete $newVsixPath
    [System.IO.Compression.ZipFile]::CreateFromDirectory($extractPath, $newVsixPath)

    Write-Host "VSIX file modified and repacked successfully!"
}

if ($publishToVSMarket) {
    #  CommitImages
    & "$env:msbuild10" .\SharpBIM.IssueTracker.Console\SharpBIM.IssueTracker.Console.csproj /p:Configuration=rnowin /t:Restore -clp:Summary`;ErrorsOnly
    dotnet build .\SharpBIM.IssueTracker.Console\SharpBIM.IssueTracker.Console.csproj -c rnowin
    IsAllGood "issue tracker console build"
     
    Log_Info "Updating Release"
    & .\SharpBIM.IssueTracker.Console\bin\rnowin\net48\SharpBIM.IssueTracker.Console.exe
    IsAllGood "update release"
    Clean
    git -C .\ add .
    git -C .\ commit -m "Release $($(Get-Content -Path .\VersionControl.txt))"
    git -C .\ push origin main-code -f

    Log_Warning  "Publishing..."
    
    & "C:\Program Files\Microsoft Visual Studio\18\Community\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe" publish -payload "$($env:RevitLibPath)\ExternalLibraries\SharpBIM.IssueTracker\vsix\SharpBIM.IssueTracker.vsix" -publishManifest ".\SharpBim.IssueTracker\jsonmainfest.json" -ignoreWarnings "VSIXValidatorWarning01,VSIXValidatorWarning02" -personalAccessToken $env:vsMarketToken
    Write-Host "Finished publish"
}
#################################################

  