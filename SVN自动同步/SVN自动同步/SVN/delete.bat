@echo 开始批量删除
call "C:\Program Files\TortoiseSVN\bin\svn.exe" delete "%1"
call "C:\Program Files\TortoiseSVN\bin\svn.exe" commit -m "在这个地方填写注释" "F:\新建文件夹 (2)"
echo 删除完成，自动退出
pause  