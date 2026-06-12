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

if ($publishToVSMarket) {
    #  CommitImages
    
    & "$env:msbuild10" .\SharpBIM.IssueTracker.Console\SharpBIM.IssueTracker.Console.csproj /p:Configuration=rnowin /t:Restore -clp:Summary`;ErrorsOnly
    # dotnet restore .\SharpBIM.IssueTracker.Console\SharpBIM.IssueTracker.Console.csproj 
    IsAllGood "update release"
    dotnet build .\SharpBIM.IssueTracker.Console\SharpBIM.IssueTracker.Console.csproj -c rnowin
    IsAllGood "update release"
    Write-Host "Updating Release"
    & .\SharpBIM.IssueTracker.Console\bin\rnowin\net48\SharpBIM.IssueTracker.Console.exe
    IsAllGood "update release"
    
    git -C .\ add .
    git -C .\ commit -m "Release $($(Get-Content -Path .\VersionControl.txt))"
    git -C .\ push origin main-code -f

    Log_Warning  "Publishing..."
    
   # & "C:\Program Files\Microsoft Visual Studio\18\Community\VSSDK\VisualStudioIntegration\Tools\Bin\VsixPublisher.exe" publish -payload "$(RevitLibPath)\ExternalLibraries\SharpBIM.IssueTracker\Rwin\net48\SharpBIM.IssueTracker.vsix" -publishManifest ".\SharpBim.IssueTracker\jsonmainfest.json" -ignoreWarnings "VSIXValidatorWarning01,VSIXValidatorWarning02" -personalAccessToken $env:vsMarketToken
    Write-Host "Finished publish"
}
#################################################

  