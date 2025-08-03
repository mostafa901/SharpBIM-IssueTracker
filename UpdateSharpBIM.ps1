#winget upgrade --id SharpBIM-nowin --accept-source-agreements --accept-package-agreements
#winget upgrade --id SharpBIM-win --accept-source-agreements --accept-package-agreements

#Update-Package SharpBIM-win -Source "D:\RevitAPI\Shared\SharpBIM\Sharp_Nugets"

Set-Location $PSScriptRoot

. "..\..\visualstudio-settings\PowerShellLibrary.ps1"

#winget upgrade --Id SharpBIM-win --accept-source-agreements --accept-package-agreements --source "D:\RevitAPI\Shared\SharpBIM\Sharp_Nugets"

#.\SharpBim.GitTracker.Core\SharpBIM.GitTracker.Core.csproj `  package  `
  
updte-package SharpBIM-win
