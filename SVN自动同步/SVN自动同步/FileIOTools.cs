using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SVN自动同步
{
    class FileIOTools
    {
        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="dirPath"></param>
        public static void CreateDirectory(string dirPath)
        {
            Directory.CreateDirectory(@dirPath);
        }
        /// <summary>
        /// 创建文件
        /// 创建文件会出现文件被访问，以至于无法删除以及编辑。建议用上using。
        /// </summary>
        /// <param name="filePath"></param>
        public static void CreateFile(string filePath)
        {
            File.Create(@filePath);
        }
        /// <summary>
        /// 复制文件
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        public static void CopyFile(string srcPath,string destPath)
        {
            File.Copy(srcPath, destPath, true);//覆盖模式
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath"></param>
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="filePath"></param>
        public static void DeleteDir(string dirPath)
        {
            //Directory.Delete(dirPath); //删除空目录，否则需捕获指定异常处理
            Directory.Delete(dirPath, true);//删除该目录以及其所有内容
        }
        /// <summary>
        /// 删除指定目录下所有内容：方法一--删除目录，再创建空目录
        /// </summary>
        /// <param name="dirPath"></param>
        public static void DeleteDirectoryContentEx(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath);
                Directory.CreateDirectory(dirPath);
            }
        }

        /// <summary>
        /// 删除指定目录下所有内容：方法二--找到所有文件和子文件夹删除
        /// </summary>
        /// <param name="dirPath"></param>
        public static void DeleteDirectoryContent(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                foreach (string content in Directory.GetFileSystemEntries(dirPath))
                {
                    if (Directory.Exists(content))
                    {
                        Directory.Delete(content, true);
                    }
                    else if (File.Exists(content))
                    {
                        File.Delete(content);
                    }
                }
            }
        }
        /// <summary>
        /// 复制文件夹中的所有文件夹与文件到另一个文件夹
        /// </summary>
        /// <param name="sourcePath">源文件夹</param>
        /// <param name="destPath">目标文件夹</param>
        public static void CopyFolder(string sourcePath, string destPath)
        {
            if (Directory.Exists(sourcePath))
            {
                if (!Directory.Exists(destPath))
                {
                    //目标目录不存在则创建
                    try
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("创建目标目录失败：" + ex.Message);
                    }
                }
                //获得源文件下所有文件
                List<string> files = new List<string>(Directory.GetFiles(sourcePath));
                files.ForEach(c =>
                {
                    string destFile = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    File.Copy(c, destFile, true);//覆盖模式
                });
                //获得源文件下所有目录文件
                List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));
                folders.ForEach(c =>
                {
                    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //采用递归的方法实现
                    CopyFolder(c, destDir);
                });
            }
            else
            {
                throw new DirectoryNotFoundException("源目录不存在！");
            }
        }
        /// <summary>
        /// 重命名文件夹
        /// </summary>
        /// <param name="OldUrl"></param>
        /// <param name="NewUrl"></param>
        public static void ReName(string OldName, string NewUrl)
        {
            OldName = OldName.Substring(OldName.LastIndexOf("\\") + 1, OldName.Length - 1 - OldName.LastIndexOf("\\"));
            string newName = Path.GetFileName(NewUrl);
            string OldUrl = NewUrl.Replace(newName, OldName);
            if (File.Exists(OldUrl))
            {
                //如果是个文件，则重命名该文件
                FileInfo fileInfo = new FileInfo(OldUrl);
                fileInfo.MoveTo(NewUrl);
                fileInfo.Delete();
              
            }
            else if (Directory.Exists(OldUrl))
            {
                //如果是个文件夹，则重命名该文件夹
                Directory.Move(OldUrl, NewUrl);
            }
        }
        /// <summary>
        /// 删除文件夹或者文件
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFileOrDir(string path)
        {
            if (File.Exists(path))
            {
                //如果是个文件，则删除该文件
                DeleteFile(path);
            }
            else if (Directory.Exists(path))
            {
                //如果是个文件夹，则删除该文件夹
                DeleteDir(path);
            }
        }
    }
}
