Param(
	[String] $folderPath
)

$destination = "https://nuget.dotnetbrightener.com/v3/index.json"
$apiKey = "FK1gPH8dGy0rzIBeUZ2DWnpvPcIxENJ7LwJceNd27uVGq5otgJBi8usACG4mrG9g7L3EzQf6hCx/LAtAsGo3LK"

Get-ChildItem -Path $folderPath | Where-Object { $_.Extension.Equals(".nupkg") } | ForEach-Object {
	dotnet nuget push -s $destination -k $apiKey $_.FullName
}

& .\nuget-cleanup.ps1