# Get the executing directory and set it to the current directory.
$scriptBin = ""
Try { $scriptBin = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)" } Catch {}
If ([string]::IsNullOrEmpty($scriptBin)) { $scriptBin = $pwd }
Set-Location $scriptBin


$demoServer = Resolve-Path "./test/Beffyman.UdpServer.Demo";
$demoClient = Resolve-Path "./test/Beffyman.UdpServer.Demo.Client";
$performance = Resolve-Path "./test/Beffyman.UdpServer.Performance";

$prevPwd = $pwd;

cd $demoClient;

Write-Host "Starting Client" -ForegroundColor Green;
start powershell { Start-Sleep -Seconds 5; dotnet run -c Release; exit; };

cd $demoServer;

Write-Host "Starting Server" -ForegroundColor Green;
dotnet run -c Release;

cd $prevPwd;

#Write-Host "" -ForegroundColor Green;
#Write-Host "Starting Performance" -ForegroundColor Green;
#cd $performance

#dotnet run -c Release;
