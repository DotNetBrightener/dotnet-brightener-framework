Param(
	[String] $packagePath,
	[String] $package = "",
	[String] $version = ""
)

$destination = "https://nuget.dotnetbrightener.com/v3/index.json"
$apiKey = "FK1gPH8dGy0rzIBeUZ2DWnpvPcIxENJ7LwJceNd27uVGq5otgJBi8usACG4mrG9g7L3EzQf6hCx/LAtAsGo3LK"

If ("" -ne $package -and "" -ne $version) {
	nuget delete $package $version -Source $destination -ApiKey $apiKey -NonInteractive
}

dotnet nuget push -s $destination -k $apiKey $packagePath

& .\nuget-cleanup.ps1 