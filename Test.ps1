# Get the executing directory and set it to the current directory.
$scriptBin = ""
Try { $scriptBin = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)" } Catch {}
If ([string]::IsNullOrEmpty($scriptBin)) { $scriptBin = $pwd }
Set-Location $scriptBin


$demoServer = "./test/Beffyman.UdpServer.Demo/Beffyman.UdpServer.Demo.csproj";
$demoClient = "./test/Beffyman.UdpServer.Demo.Client/Beffyman.UdpServer.Demo.Client.csproj";