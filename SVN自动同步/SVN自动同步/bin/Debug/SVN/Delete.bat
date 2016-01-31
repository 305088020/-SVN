@echo 开始批量删除
echo %1
call "C:\Program Files\TortoiseSVN\bin\svn.exe" delete %1 --force
echo 删除完成，自动退出