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
    [bool] $All = 0,
    [bool] $IgnoreCheck = 0,
    [bool] $IncrementGit = 0,
    [int] $PublishToServer = 0,
    [bool] $publish = $false
) 
Set-Location $PSScriptRoot
$options = [BuildOptions]::new($Configs, $PSBoundParameters)
$options.IsDotNetBuild = $true

Set-Location $PSScriptRoot
$options.Initialize(".\SharpBIM.IssueTracker.Core\SharpBIM.IssueTracker.Core.csproj")
    


if ($IncrementGit -eq 1) {
    
    $gitPathString = (Get-Item ".\VersionControl.txt").FullName
    $gitPathString 
    $currentGitVersion = (Get-Content $gitPathString)
    # Split the version into major and minor parts
    $versionParts = $currentGitVersion -split '\.'
    
    # Increment the last part of the version
    $minorVersion = [int]::Parse($versionParts[1])
    $minorVersion += 1
    $newGitVersion = "$($versionParts[0]).$($minorVersion).0.0"
    (Set-Content -Path $gitPathString -Value $newGitVersion)    
    
    SetVersionAllFiles "AssemblyVersion>(\d+\.\d+\.\d+\.\d+)</" $gitPathString.$FolderPath "*.props" $newGitVersion
    SetVersionAllFiles "AssemblyVersion>(\d+\.\d+\.\d+\.\d+)</" $gitPathString.$FolderPath "*.cs" $newGitVersion
    SetVersionAllFiles "AssemblyVersion>(\d+\.\d+\.\d+\.\d+)</" $gitPathString.$FolderPath "*.csproj" $newGitVersion
    SetVersionAllFiles """ Version=""(\d+\.\d+\.\d+\.\d+)""" $gitPathString.$FolderPath "*.vsixmanifest" $newGitVersion
    
    IsAllGood "Building IssueTracker $conf"
}
$options.InvokeBuild()



#################################################

  