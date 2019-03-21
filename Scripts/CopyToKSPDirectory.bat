::You must keep this file in the solution folder for it to work. 
::Make sure to pass the solution configuration when calling it (either Debug or Release)

::Set the directories in the setdirectories.bat file if you want a different folder than Kerbal Space Program
::EXAMPLE:
SET KSPPATH=G:\GAMES\KERBAL\Kerbal Space Program_DEV_161_M
SET KSPPATH2=G:\GAMES\KERBAL\Kerbal Space Program_DEV_161
call "%~dp0\SetDirectories.bat"

IF DEFINED KSPPATH (ECHO KSPPATH is defined) ELSE (SET KSPPATH=C:\Kerbal Space Program)
IF DEFINED KSPPATH2 (ECHO KSPPATH2 is defined)
::%1
SET SOLUTIONCONFIGURATION=Debug

mkdir "%KSPPATH%\GameData\LunaMultiplayer\"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\")

mkdir "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

del "%KSPPATH%\GameData\LunaMultiplayer\Plugins\*.*" /Q /F
IF DEFINED KSPPATH2 (del "%KSPPATH2%\GameData\LunaMultiplayer\Plugins\*.*" /Q /F)

mkdir "%KSPPATH%\GameData\LunaMultiplayer\Button"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\Button")

del "%KSPPATH%\GameData\LunaMultiplayer\Button\*.*" /Q /F
IF DEFINED KSPPATH2 (del "%KSPPATH2%\GameData\LunaMultiplayer\Button\*.*" /Q /F)

mkdir "%KSPPATH%\GameData\LunaMultiplayer\Localization"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\Localization")

del "%KSPPATH%\GameData\LunaMultiplayer\Localization\*.*" /Q /F
IF DEFINED KSPPATH2 (del "%KSPPATH2%\GameData\LunaMultiplayer\Localization\*.*" /Q /F)

mkdir "%KSPPATH%\GameData\LunaMultiplayer\PartSync"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\PartSync")

del "%KSPPATH%\GameData\LunaMultiplayer\PartSync\*.*" /Q /F
IF DEFINED KSPPATH2 (del "%KSPPATH2%\GameData\LunaMultiplayer\PartSync\*.*" /Q /F)

mkdir "%KSPPATH%\GameData\LunaMultiplayer\Icons"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\Icons")

del "%KSPPATH%\GameData\LunaMultiplayer\Icons\*.*" /Q /F
IF DEFINED KSPPATH2 (del "%KSPPATH2%\GameData\LunaMultiplayer\Icons\*.*" /Q /F)

mkdir "%KSPPATH%\GameData\LunaMultiplayer\Flags"
IF DEFINED KSPPATH2 (mkdir "%KSPPATH2%\GameData\LunaMultiplayer\Flags")

del "%KSPPATH%\GameData\LunaMultiplayer\Flags\*.*" /Q /F
IF DEFINED KSPPATH2 (del "%KSPPATH2%\GameData\LunaMultiplayer\Flags\*.*" /Q /F)

"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaClient.dll"
"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LmpGlobal.dll"
"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaCommon.dll"
"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\LunaUpdater.dll"
"%~dp0..\External\pdb2mdb\pdb2mdb.exe" "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\Lidgren.Network.dll"

xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\*.*" "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\bin\%SOLUTIONCONFIGURATION%\*.*" "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

xcopy /Y "%~dp0..\External\Dependencies\*.*" "%KSPPATH%\GameData\LunaMultiplayer\Plugins"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\External\Dependencies\*.*" "%KSPPATH2%\GameData\LunaMultiplayer\Plugins")

xcopy /Y "%~dp0..\Client\Resources\*.png" "%KSPPATH%\GameData\LunaMultiplayer\Button"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\Resources\*.png" "%KSPPATH2%\GameData\LunaMultiplayer\Button")

xcopy /Y /E "%~dp0..\Client\Localization\XML\*.*" "%KSPPATH%\GameData\LunaMultiplayer\Localization"
IF DEFINED KSPPATH2 (xcopy /Y /E "%~dp0..\Client\Localization\XML\*.*" "%KSPPATH2%\GameData\LunaMultiplayer\Localization")

xcopy /Y "%~dp0..\Client\ModuleStore\XML\*.xml" "%KSPPATH%\GameData\LunaMultiplayer\PartSync"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\ModuleStore\XML\*.xml" "%KSPPATH2%\GameData\LunaMultiplayer\PartSync")

xcopy /Y "%~dp0..\Client\Resources\Icons\*.*" "%KSPPATH%\GameData\LunaMultiplayer\Icons"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\Resources\Icons\*.*" "%KSPPATH2%\GameData\LunaMultiplayer\Icons")

xcopy /Y "%~dp0..\Client\Resources\Flags\*.*" "%KSPPATH%\GameData\LunaMultiplayer\Flags"
IF DEFINED KSPPATH2 (xcopy /Y "%~dp0..\Client\Resources\Flags\*.*" "%KSPPATH2%\GameData\LunaMultiplayer\Flags")
