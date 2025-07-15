@echo off
chcp 936 
set "tool=Compiler/excel2json.exe"
set configDir=%~dp0

REM 检查工具是否存在
if not exist "%tool%" (
    echo 错误：未找到工具 "%tool%"
    pause
    exit /b 1
)

REM 执行命令并处理错误
"%tool%" --export %configDir%
if errorlevel 1 (
    echo excel转Json或C#文件时出错（错误码：%errorlevel%）
    pause
    exit /b 1
)
pause