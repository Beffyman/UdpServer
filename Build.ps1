# Get the executing directory and set it to the current directory.
$scriptBin = ""
Try { $scriptBin = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)" } Catch {}
If ([string]::IsNullOrEmpty($scriptBin)) { $scriptBin = $pwd }
Set-Location $scriptBin

function CreateArtifactsFolder{

	$Global:artifacts = "./artifacts";

	if((Test-Path $artifacts) -eq $true){
		Remove-Item -Path $artifacts -Force -Recurse;
	}

	mkdir "./artifacts" -Force;

	$Global:artifacts = Resolve-Path $artifacts;
}

function GetVersion{

	$Global:version = $env:APPVEYOR_BUILD_VERSION;
	$localBuild = $false;
	#For local builds, appveyor will be provided version
	if([System.String]::IsNullOrEmpty($version)){
		$Global:version = & git describe --tags;
		$localBuild = $true;
	}
	#Filter out - branch commit locally
	if($version -Match "-"){
		$Global:version = $version.Split("-")[0];
	}

	#Filter out +Build# from CI builds
	if($version -Match "\+"){
		$Global:version = $version.Split("+")[0];
	}


	if($localBuild -eq $true){
		$build = & git rev-list --count HEAD;
		$Global:version = "$($version)$build";
	}


	Write-Host "---Version $version will be used---" -ForegroundColor Magenta;
}

GetVersion;
CreateArtifactsFolder;

dotnet pack -c Release -o $artifacts -p:PackageVersion="$version";