@echo 开始批量提交
echo %1
call "C:\Program Files\TortoiseSVN\bin\svn.exe" commit -m "SVN自动转换" %1
echo 提交完成，自动退出