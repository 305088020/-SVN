@echo 开始批量添加
echo %1
call "C:\Program Files\TortoiseSVN\bin\svn.exe" add  %1  --force
call "C:\Program Files\TortoiseSVN\bin\svn.exe" commit -m "SVN自动转换" %1
echo 添加提交完成，自动退出
