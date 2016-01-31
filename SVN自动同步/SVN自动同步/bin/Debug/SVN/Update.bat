@echo 开始更新项目
echo %1
call "C:\Program Files\TortoiseSVN\bin\svn.exe" update %1
echo 更新完成，自动退出