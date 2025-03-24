@echo off
setlocal enabledelayedexpansion
cd /d %~dp0

set "sourcedir=%cd%\binary_data"
::echo [INFO] data:%datadir%

set "targetdir=%cd%\..\Assets\GameAssets\DataConfigs"
::echo [INFO] target:%targetdir%

rem 检查配置源文件夹是否存在
if not exist "%sourcedir%\" (
	echo error:not exist %sourcedir%
	exit /b 1
)

rem 检查目标文件夹是否存在
if not exist "%targetdir%\" (
	mkdir "%targetdir%" >nul 2>&1
	if errorlevel 1 (
		echo error:create folder %targetdir% failed!
		exit /b 1
	)
	exit /b 1
)

rem 文件复制
echo copying...from %datadir% to %targetdir%
for %%F in ("%sourcedir%\*.*") do (
	set "filename=%%~nxF"
	
	::删除目标文件夹中的同名文件
	if exist "%targetdir%\!filename!" (
		del /f /q "%targetdir%\!filename!" >nul 2>&1
		if errorlevel 1 (
			echo error:delete "%targetdir%\!filename!" failed!
			exit /b 1
		)
		
		if exist "%targetdir%\!filename!.meta" (
		del /f /q "%targetdir%\!filename!.meta" >nul 2>&1
		if errorlevel 1 (
			echo error:delete "%targetdir%\!filename!" failed!
			exit /b 1
		)
	)
	)
	
	::文件复制
	copy /y "%%F" "%targetdir%\" >nul 2>&1
	if errorlevel 1 (
		echo error:copy "!filename!" failed!
		exit /b 1
	)
)

echo copy successfully!

endlocal
exit