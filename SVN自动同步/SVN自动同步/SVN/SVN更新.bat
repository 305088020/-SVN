==========�Զ� ����SVNĿ¼�ļ�.bat==============================

@echo off
cls
color 0a


set SOURCE="F:\�½��ļ��� (2)"
Set SVN=C:\Program Files\TortoiseSVN\bin


echo. ==========SVN �Զ����¹���==========
echo. ����Ŀ¼%SOURCE%


"%SVN%\TortoiseProc.exe" /command:update /path:"%SOURCE%" /closeonend:2
echo. ==============�������==============