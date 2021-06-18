$nugetCacheFolder = "$($Env:USERPROFILE)\.nuget\packages"
$reesoftNugetCachedFolders = Get-ChildItem -Path $nugetCacheFolder -Directory -ErrorAction SilentlyContinue

ForEach($folder in $reesoftNugetCachedFolders) {
  If ($folder.Name.Contains("dotnetbrightener")) {
    Remove-Item -Path $folder -Recurse -Force
  }
}

Write-Host
Write-Host
Write-Host "Cleaned up DotNetBrightener Nuget caches"
Write-Host
Write-Host