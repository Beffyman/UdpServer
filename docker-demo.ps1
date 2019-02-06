$scriptBin = ""
Try { $scriptBin = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)" } Catch {}
If ([string]::IsNullOrEmpty($scriptBin)) { $scriptBin = $pwd }
Set-Location $scriptBin

docker system prune -f

docker build -f DOCKERFILE.demo . -t "beffyman/udpserver-demo:latest"
docker run -p "6002:6002/udp" "beffyman/udpserver-demo:latest" --sysctl net.core.rmem_max=25000000
