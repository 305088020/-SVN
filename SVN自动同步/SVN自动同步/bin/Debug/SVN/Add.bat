@echo ��ʼ�������
echo %1
call "C:\Program Files\TortoiseSVN\bin\svn.exe" add  %1  --force
call "C:\Program Files\TortoiseSVN\bin\svn.exe" commit -m "SVN�Զ�ת��" %1
echo ����ύ��ɣ��Զ��˳�
