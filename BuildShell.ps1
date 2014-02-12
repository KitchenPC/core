#  KitchenPC Dev Shell setup

#  To use, add the following programs to your system path:
#    msbuild.exe (Provided with the .NET Framework)
#    nunit-console.exe (Available at http://nunit.org/)

# Optionally, add the following shortcut to your quick launch or start menu:
# %SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -noexit -file "c:\KitchenPC\BuildShell.ps1" "C:\KitchenPC"
# You can substitute C:\KitchenPC with the path to your KitchenPC enlistment.

param([String]$enlistment="C:\KitchenPC\")
Write-Host "Initializing KitchenPC Dev Shell, Please Wait..." -NoNewLine
$host.ui.RawUI.WindowTitle = "KitchenPC Dev Shell"

New-PSDrive -name kpc -PSProvider FileSystem -root $enlistment >$null
Set-Alias vs "kpc:\KitchenPC.sln"

function cd..
{
   Set-Location ..
}

function cd\
{
   Set-Location \
}

function Guid
{
   Write-Host ([System.Guid]::NewGuid().ToString())
}

function Test
{
   $testpath = Convert-Path('kpc:\src\UnitTests\bin\Debug\')
   nunit-console.exe ($testpath + "KitchenPC.UnitTests.dll")
}

function Build
{
   if ($args.Count -gt 0)
   {
     switch ($args[0].ToUpper())
     {
        "DEBUG"   { msbuild .\build.xml /t:Build /p:Configuration=Staging /verbosity:minimal }
        "RELEASE" { msbuild .\build.xml /t:Build /p:Configuration=Release /verbosity:minimal }
     }
   }
   else
   {
       msbuild .\build.xml /t:Build /p:Configuration=Debug /verbosity:minimal
   }
}

function Clean
{
   msbuild .\build.xml /t:Clean
}

#Set default location
Set-Location kpc:\src\
Write-Host " Done.`n`n"