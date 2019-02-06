# Get the executing directory and set it to the current directory.
$scriptBin = ""
Try { $scriptBin = "$(Split-Path -Parent $MyInvocation.MyCommand.Definition)" } Catch {}
If ([string]::IsNullOrEmpty($scriptBin)) { $scriptBin = $pwd }
Set-Location $scriptBin

try{Get-ChildItem -Path "./" -Include "bin" -Recurse -Force | Remove-Item -Force -Recurse | Out-Host;}catch{}
try{Get-ChildItem -Path "./" -Include "obj" -Recurse -Force | Remove-Item -Force -Recurse | Out-Host;}catch{}
try{Get-ChildItem -Path "./" -Include ".vs" -Recurse -Force | Remove-Item -Force -Recurse | Out-Host;}catch{}