@echo 开始批量添加
call "C:\Program Files\TortoiseSVN\bin\svn.exe" add  *  --force
call "C:\Program Files\TortoiseSVN\bin\svn.exe" commit -m "在这个地方填写注释" "F:\新建文件夹 (2)"
echo 添加提交完成，自动退出
pause  