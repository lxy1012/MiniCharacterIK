@echo off

::echo [INFO] current dir:%curdir%
cd /d %~dp0
cd src
::需配置python环境变量
python ./generate.py

exit