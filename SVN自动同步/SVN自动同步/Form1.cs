using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace SVN自动同步
{
    public partial class Form1 : Form
    {
        private FileSystemWatcher watcher_W;
        private delegate void UpdateWatchTextDelegate(string name);

        private FileSystemWatcher watcher_N;
        //private delegate void UpdateWatchTextDelegate(string newText);

        private Dictionary<string, List<string>> map_W = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> map_N = new Dictionary<string, List<string>>();

        private List<string> list_add_W = new List<string>();
        private List<string> list_add_N = new List<string>();
        private List<string> list_change_W = new List<string>();
        private List<string> list_change_N = new List<string>();
        private List<string> list_delete_W = new List<string>();
        private List<string> list_delete_N = new List<string>();
        private List<string> list_rename_W = new List<string>();
        private List<string> list_rename_N = new List<string>();
        private string create_first_name_W = "";
        private string create_first_name_N = "";
        private Boolean svn_w_flg = true;

        public Form1()
        {
            InitializeComponent();
            //初始化得到配置文件中的SVN内网外网CheckOut地址
            string SVN_W = AppSettings.GetValue("SVN_W");
            string SVN_N = AppSettings.GetValue("SVN_N");
            string SVN_TIME = AppSettings.GetValue("SVN_TIME");
            string SlEEP_TIME = AppSettings.GetValue("SlEEP_TIME");

            textBox1.Text = SVN_W;
            textBox2.Text = SVN_N;
            textBox3.Text = SlEEP_TIME;
            textBox4.Text = SVN_TIME;
            //定时器打开
            InitalizeTimer();
            //监视外网文件
            this.watcher_W = new FileSystemWatcher();
            this.watcher_W.Deleted += new FileSystemEventHandler(watcher_Deleted);
            this.watcher_W.Renamed += new RenamedEventHandler(watcher_Renamed);
            this.watcher_W.Changed += new FileSystemEventHandler(watcher_Changed);
            this.watcher_W.Created += new FileSystemEventHandler(watcher_Created);
            //监视内网仓库
            this.watcher_N = new FileSystemWatcher();
            this.watcher_N.Deleted += new FileSystemEventHandler(watcher_Deleted_N);
            this.watcher_N.Renamed += new RenamedEventHandler(watcher_Renamed_N);
            this.watcher_N.Changed += new FileSystemEventHandler(watcher_Changed_N);
            this.watcher_N.Created += new FileSystemEventHandler(watcher_Created_N);
        }
        //监视方法
        private void watchFile()
        {
            watcher_W.Path = AppSettings.GetValue("SVN_W");//监控路径（文件夹）配置文件中内容
            watcher_W.IncludeSubdirectories = true; //是否包含子目录

            watcher_W.Filter = "*.*";//如果filter为文件名称则表示监控该文件，如果为*.txt则表示要监控指定目录当中的所有.txt文件
            watcher_W.NotifyFilter = NotifyFilters.LastWrite |
                NotifyFilters.FileName |
                NotifyFilters.DirectoryName | NotifyFilters.Size;
            //begin watching.
            watcher_W.EnableRaisingEvents = true;
            
            
            //内网文件监视
            watcher_N.Path = AppSettings.GetValue("SVN_N"); ;//监控路径（文件夹）配置文件中内容
            watcher_N.IncludeSubdirectories = true; //是否包含子目录

            watcher_N.Filter = "*.*";//如果filter为文件名称则表示监控该文件，如果为*.txt则表示要监控指定目录当中的所有.txt文件
            watcher_N.NotifyFilter = NotifyFilters.LastWrite |
                NotifyFilters.FileName |
                NotifyFilters.DirectoryName | NotifyFilters.Size;
            //begin watching.
            watcher_N.EnableRaisingEvents = true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            fbdlg.ShowDialog();
            string name = fbdlg.SelectedPath;
            textBox1.Text = name;

            //将选择的CheckOut地址写入配置文件
            AppSettings.SetValue("SVN_W", textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            fbdlg.ShowDialog();
            string name = fbdlg.SelectedPath;
            textBox2.Text = name;
            //将选择的CheckOut地址写入配置文件
            AppSettings.SetValue("SVN_N", textBox2.Text);
        }

        //开始监控按钮启动 程序
        private void button3_Click(object sender, EventArgs e)
        {

            //开始监控之后，将启动两方面的内容，
            //1、将计时器打开，每隔10分钟，将外网的源代码down（Update）到外网地址一次
            timer1.Enabled = true;//打开计时器

            //Ping网络是否通畅
            if (!message())
            { 
                return;
            }
            else
            {
                //2、分别监控两个代码仓库，并且在某个仓库有改动的时候 将改动体现到另一个仓库中去
                watchFile();

                if (svn_w_flg)
                {
                    //执行更新bat文件，从内网SVN上面获取最新的代码跟文件
                    Runbat(AppSettings.GetValue("SVN_W"), "Update.bat");
                    //执行文件变动
                    FileCaoZuo_W();
                    //Thread.Sleep(1000 * 60 * int.Parse(AppSettings.GetValue("SlEEP_TIME")));  
                    //执行更新bat文件，从内网SVN上面获取最新的代码跟文件
                    Runbat(AppSettings.GetValue("SVN_N"), "Update.bat");
                    //执行文件变动
                    FileCaoZuo_N();
                }
                else
                {
                    //执行更新bat文件，从内网SVN上面获取最新的代码跟文件
                    Runbat(AppSettings.GetValue("SVN_N"), "Update.bat");
                    //执行文件变动
                    FileCaoZuo_N();

                    //执行更新bat文件，从内网SVN上面获取最新的代码跟文件
                    Runbat(AppSettings.GetValue("SVN_W"), "Update.bat");
                    //执行文件变动
                    FileCaoZuo_W();
                }

                Runbat(AppSettings.GetValue("SVN_W"), "Commit.bat");
                Runbat(AppSettings.GetValue("SVN_N"), "Commit.bat");
                //初始化
                svn_w_flg = true;
                this.WindowState = FormWindowState.Minimized;
            }
        }

        private bool message()
        {
            if ((AppSettings.GetValue("PING_ADD_W").Equals("")) || (AppSettings.GetValue("PING_ADD_N").Equals(""))) {
                return true;
            }

            if (!Internet.PingIpOrDomainName(AppSettings.GetValue("PING_ADD_W")) || (!Internet.PingIpOrDomainName(AppSettings.GetValue("PING_ADD_N"))))
            {
                MessageBox.Show("局域网/外部SVN服务器网络不通！");
                return false;
            }
            else {
                return true;
            }
        }
        /// <summary>
        /// 云代码文件操作
        /// </summary>
        private void FileCaoZuo_W()
        {
            //执行文件create操作
            if (list_add_W.Count > 0)
            {
                foreach (string str in list_add_W)
                {
                    //创建了一个新文件，要将这个文件同步到代码仓库中去
                    watcher_N.EnableRaisingEvents = false;

                    String desdir = desdir_N(str);
                    if (File.Exists(str))
                    {
                        //如果是个文件，则只复制该文件
                        //复制文件
                        FileIOTools.CopyFile(str, desdir);
                    }
                    else if (Directory.Exists(str))
                    {
                        //如果第一个过来的是文件夹，则只创建文件夹
                        //复制文件夹
                        FileIOTools.CreateDirectory(desdir);
                        //FileIOTools.CopyFolder(str, desdir);
                    }
                    
                    //完成之后再打开另一个bat
                    watcher_N.EnableRaisingEvents = true;
                }
                //执行完后，清空List
                list_add_W.Clear();
                create_first_name_W = "";
                //执行更新bat文件，从内网SVN上面commit最新的代码跟文件
                Runbat(AppSettings.GetValue("SVN_W"), "Add.bat");
                Runbat(AppSettings.GetValue("SVN_N"), "Add.bat");
            }
            //执行文件change操作
            if (list_change_W.Count > 0)
            {
                foreach (string str in list_change_W)
                {
                    //停掉另一个监听
                    watcher_N.EnableRaisingEvents = false;

                    if (File.Exists(str))
                    {
                        //如果是个文件，则只复制该文件
                        //复制文件
                        FileIOTools.CopyFile(str, desdir_N(str));
                    }
                    else if (Directory.Exists(str))
                    {
                        //如果第一个过来的是文件夹，则直接把该文件全部复制过去
                        //复制文件夹
                        FileIOTools.CopyFolder(str, desdir_N(str));
                    }
                    //完成之后再打开另一个bat
                    watcher_N.EnableRaisingEvents = true;
                }
                //执行完后，清空List
                list_change_W.Clear();
                //执行更新bat文件，从内网SVN上面commit最新的代码跟文件
                Runbat(AppSettings.GetValue("SVN_W"), "Commit.bat");
                Runbat(AppSettings.GetValue("SVN_N"), "Commit.bat");
            }
            //执行文件delete操作
            if (list_delete_W.Count > 0)
            {
                foreach (string str in list_delete_W)
                {
                    //停掉另一个监听
                    watcher_N.EnableRaisingEvents = false;
                    //删除文件
                    FileIOTools.DeleteFileOrDir(desdir_N(str));
                    //删除之后要执行delete.bat文件
                    //Runbat(desdir_N(e.FullPath), "Delete.bat");
                    Runbat("\"" + str + "\"", "Delete.bat");
                    Runbat("\"" + str + "\"", "Commit.bat");
                    Runbat("\"" + str.Replace(AppSettings.GetValue("SVN_W"), AppSettings.GetValue("SVN_N")) + "\"", "Delete.bat");
                    Runbat("\"" + str.Replace(AppSettings.GetValue("SVN_W"), AppSettings.GetValue("SVN_N")) + "\"", "Commit.bat");
                    //完成之后再打开另一个bat
                    watcher_N.EnableRaisingEvents = true;
                }
                //执行完后，清空List
                list_change_W.Clear();
                //执行更新bat文件，从内网SVN上面commit最新的代码跟文件
                Runbat(AppSettings.GetValue("SVN_W"), "Commit.bat");
                Runbat(AppSettings.GetValue("SVN_N"), "Commit.bat");
            }

        }
        /// <summary>
        /// 本地代码文件操作
        /// </summary>
        private void FileCaoZuo_N()
        {
            //执行文件create操作
            if (list_add_N.Count > 0)
            {
                foreach (string str in list_add_N)
                {
                    watcher_W.EnableRaisingEvents = false;

                    String desdir = desdir_N(str);
                    if (File.Exists(str))
                    {
                        //如果是个文件，则只复制该文件
                        //复制文件
                        FileIOTools.CopyFile(str, desdir);
                    }
                    else if (Directory.Exists(str))
                    {
                        //如果第一个过来的是文件夹，则直接把该文件全部复制过去
                        //复制文件夹
                        FileIOTools.CopyFolder(str, desdir);
                    }
                    //完成之后再打开另一个bat
                    watcher_W.EnableRaisingEvents = true;
                }
                //执行完后，清空List
                list_add_N.Clear();
                create_first_name_W = "";
                //执行更新bat文件，从内网SVN上面commit最新的代码跟文件
                Runbat(AppSettings.GetValue("SVN_N"), "Add.bat");
                Runbat(AppSettings.GetValue("SVN_W"), "Add.bat");
            }
            //执行文件change操作
            if (list_change_N.Count > 0)
            {
                foreach (string str in list_change_N)
                {
                    //停掉另一个监听
                    watcher_W.EnableRaisingEvents = false;

                    if (File.Exists(str))
                    {
                        //如果是个文件，则只复制该文件
                        //复制文件
                        FileIOTools.CopyFile(str, desdir_N(str));
                    }
                    else if (Directory.Exists(str))
                    {
                        //如果第一个过来的是文件夹，则直接把该文件全部复制过去
                        //复制文件夹
                        FileIOTools.CopyFolder(str, desdir_N(str));
                    }

                    //完成之后再打开另一个bat
                    watcher_W.EnableRaisingEvents = true;
                }
                //执行完后，清空List
                list_change_N.Clear();
                //执行更新bat文件，从内网SVN上面commit最新的代码跟文件
                Runbat(AppSettings.GetValue("SVN_N"), "Commit.bat");
                Runbat(AppSettings.GetValue("SVN_W"), "Commit.bat");
            }
            //执行文件delete操作
            if (list_delete_N.Count > 0)
            {
                foreach (string str in list_delete_N)
                {
                    //停掉另一个监听
                    watcher_W.EnableRaisingEvents = false;
                    //删除文件
                    FileIOTools.DeleteFileOrDir(desdir_N(str));
                    //删除之后要执行delete.bat文件
                    //Runbat(desdir_N(e.FullPath), "Delete.bat");
                    Runbat("\"" + str + "\"", "Delete.bat");
                    Runbat("\"" + str + "\"", "Commit.bat");
                    Runbat("\"" + str.Replace(AppSettings.GetValue("SVN_N"), AppSettings.GetValue("SVN_W")) + "\"", "Delete.bat");
                    Runbat("\"" + str.Replace(AppSettings.GetValue("SVN_N"), AppSettings.GetValue("SVN_W")) + "\"", "Commit.bat");
                    //完成之后再打开另一个bat
                    watcher_W.EnableRaisingEvents = true;
                }
                //执行完后，清空List
                list_change_W.Clear();
                //执行更新bat文件，从内网SVN上面commit最新的代码跟文件
                Runbat(AppSettings.GetValue("SVN_W"), "Commit.bat");
                Runbat(AppSettings.GetValue("SVN_N"), "Commit.bat");
            }

        }
        /// <summary>
        /// Run Bat
        /// </summary>
        /// <param name="svn">src</param>
        /// <param name="name">batName</param>
        public void Runbat(string svn,string name)
        {
            string batName = name;
            Process proc = null;
            try
            {
                proc = new Process();
                proc.StartInfo.FileName = Application.StartupPath + @"\SVN\" + batName;
                if (svn.Contains("\\"))
                {
                    proc.StartInfo.Arguments = svn;//this is argument
                }
                else
                {
                    proc.StartInfo.Arguments = @svn;//this is argument
                }
                proc.StartInfo.Arguments = @svn;//this is argument
                proc.StartInfo.CreateNoWindow = false;
                //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                proc.Start();
                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
            }
        }
        //计时方法
        private void InitalizeTimer()
        {
            string SVN_TIME = AppSettings.GetValue("SVN_TIME");
            timer1.Interval = 1000 * 60 * int.Parse(SVN_TIME);//设置时钟周期为10分钟
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Enabled = false;
        }

        //加入需要定时执行的方法
        private void timer1_Tick(object sender, EventArgs e)
        {
            //定时要做的事情
            button3_Click(sender, e);
            //执行更新bat文件，从内网SVN上面获取最新的代码跟文件
            Runbat(AppSettings.GetValue("SVN_W"), "Update.bat");
            //执行更新bat文件，从内网SVN上面获取最新的代码跟文件
            Runbat(AppSettings.GetValue("SVN_N"), "Update.bat");
        }
        //////////////////////////////////////////////////////////////
        /// <summary>
        /// 创建文件夹跟文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (e.FullPath.Contains(".svn"))
                {
                    return;
                }
                else
                {
                    svn_w_flg = true;
                    WriteLog(String.Format("{0} File: {1} Created", DateTime.Now, e.FullPath));
                    if (!list_add_W.Contains(e.FullPath))
                    {
                        list_add_W.Add(e.FullPath);
                    }
                    //if (create_first_name_W.Equals(""))
                    //{
                    //    create_first_name_W = e.FullPath;
                    //    WriteLog(String.Format("{0} File: {1} Created", DateTime.Now, e.FullPath));
                    //}
                    //else
                    //{
                    //    if (File.Exists(e.FullPath))
                    //    {
                    //        //如果是个文件，则只复制该文件
                    //        //复制文件
                    //        list_add_W.Add(e.FullPath);
                    //        WriteLog(String.Format("{0} File: {1} Created", DateTime.Now, e.FullPath));
                    //    }
                    //    else if (Directory.Exists(e.FullPath))
                    //    {
                    //        if ((!e.FullPath.Contains(create_first_name_W)) && (Directory.Exists(e.FullPath)))
                    //        {
                    //            create_first_name_W = e.FullPath;
                    //            list_add_W.Add(e.FullPath);
                    //            WriteLog(String.Format("{0} File: {1} Created", DateTime.Now, e.FullPath));
                    //        }
                    //    }
                    //}
                    ////创建了一个新文件，要将这个文件同步到代码仓库中去
                    //watcher_N.EnableRaisingEvents = false;

                    //String desdir = desdir_N(e.FullPath);   
                    //if (File.Exists(e.FullPath))
                    //{
                    //    //如果是个文件，则只复制该文件
                    //    //复制文件
                    //    FileIOTools.CopyFile(e.FullPath, desdir);
                    //}
                    //else if (Directory.Exists(e.FullPath))
                    //{
                    //    //如果第一个过来的是文件夹，则直接把该文件全部复制过去
                    //    //复制文件夹
                    //    FileIOTools.CopyFolder(e.FullPath, desdir);
                    //}
                    ////执行更新bat文件，从内网SVN上面commit最新的代码跟文件
                    //Runbat(AppSettings.GetValue("SVN_W"), "Add.bat");
                    //Runbat(AppSettings.GetValue("SVN_N"), "Add.bat");      

                    ////完成之后再打开另一个bat
                    //watcher_N.EnableRaisingEvents = true;
                }
            }
            catch (IOException)
            {

            }
        }
        /// <summary>
        /// 创建文件夹跟文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_Created_N(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (e.FullPath.Contains(".svn"))
                {
                    return;
                }
                else
                {
                    svn_w_flg = false;
                    WriteLog(String.Format("{0} File: {1} Created", DateTime.Now, e.FullPath));
                    if (!list_add_N.Contains(e.FullPath))
                    {
                        list_add_N.Add(e.FullPath);
                    }
                    //if (create_first_name_N.Equals(""))
                    //{
                    //    create_first_name_N = e.FullPath;
                    //    WriteLog(String.Format("{0} File: {1} Created", DateTime.Now, e.FullPath));
                    //}
                    //else
                    //{
                    //    if (File.Exists(e.FullPath))
                    //    {
                    //        //如果是个文件，则只复制该文件
                    //        //复制文件
                    //        list_add_N.Add(e.FullPath);
                    //        WriteLog(String.Format("{0} File: {1} Created", DateTime.Now, e.FullPath));
                    //    }
                    //    else if (Directory.Exists(e.FullPath))
                    //    {
                    //        if (!e.FullPath.Contains(create_first_name_N))
                    //        {
                    //            create_first_name_N = e.FullPath;
                    //            list_add_N.Add(e.FullPath);
                    //            WriteLog(String.Format("{0} File: {1} Created", DateTime.Now, e.FullPath));
                    //        }
                    //    }
                    //}
                    ////创建了一个新文件，要将这个文件同步到代码仓库中去
                    //watcher_W.EnableRaisingEvents = false;

                    //String desdir = desdir_N(e.FullPath);
                    //if (File.Exists(e.FullPath))
                    //{
                    //    //如果是个文件，则只复制该文件
                    //    //复制文件
                    //    FileIOTools.CopyFile(e.FullPath, desdir);
                    //}
                    //else if (Directory.Exists(e.FullPath))
                    //{
                    //    //如果第一个过来的是文件夹，则直接把该文件全部复制过去
                    //    //复制文件夹
                    //    FileIOTools.CopyFolder(e.FullPath, desdir);
                    //}
                    ////执行更新bat文件，从内网SVN上面commit最新的代码跟文件
                    //Runbat(AppSettings.GetValue("SVN_N"), "Add.bat");
                    //Runbat(AppSettings.GetValue("SVN_W"), "Add.bat");     

                    ////完成之后再打开另一个bat
                    //watcher_W.EnableRaisingEvents = true;
                }
            }
            catch (IOException)
            {

            }
        }
        /// <summary>
        /// 文件夹和文件改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (e.FullPath.Contains(".svn"))
                {
                    return;
                }
                else
                {
                    svn_w_flg = true;
                    WriteLog(String.Format("{0} File: {1} {2}",  DateTime.Now, e.FullPath, e.ChangeType.ToString()));
                    if (!list_change_W.Contains(e.FullPath))
                    {
                        list_change_W.Add(e.FullPath);
                    }
                    ////停掉另一个监听
                    //watcher_N.EnableRaisingEvents = false;

                    //if (File.Exists(e.FullPath))
                    //{
                    //    //如果是个文件，则只复制该文件
                    //    //复制文件
                    //    FileIOTools.CopyFile(e.FullPath, desdir_N(e.FullPath));
                    //}
                    //else if (Directory.Exists(e.FullPath))
                    //{
                    //    //如果第一个过来的是文件夹，则直接把该文件全部复制过去
                    //    //复制文件夹
                    //    FileIOTools.CopyFolder(e.FullPath, desdir_N(e.FullPath));
                    //}
                    //Runbat(AppSettings.GetValue("SVN_W"), "Commit.bat");
                    //Runbat(AppSettings.GetValue("SVN_N"), "Commit.bat");
                    ////完成之后再打开另一个bat
                    //watcher_N.EnableRaisingEvents = true;
                }
            }
            catch (IOException)
            {
               // this.BeginInvoke(new UpdateWatchTextDelegate(UpdateWatchText), "修改日志写入失败!");
            }
        }
        /// <summary>
        /// 文件夹和文件改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_Changed_N(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (e.FullPath.Contains(".svn"))
                {
                    return;
                }
                else
                {
                    svn_w_flg = false;
                    WriteLog(String.Format("{0} File: {1} {2}", DateTime.Now, e.FullPath, e.ChangeType.ToString()));
                    if (!list_change_N.Contains(e.FullPath))
                    {
                        list_change_N.Add(e.FullPath);
                    }
                    //WriteLog(String.Format("File: {0} {1}", e.FullPath, e.ChangeType.ToString()));
                    ////停掉另一个监听
                    //watcher_W.EnableRaisingEvents = false;

                    //if (File.Exists(e.FullPath))
                    //{
                    //    //如果是个文件，则只复制该文件
                    //    //复制文件
                    //    FileIOTools.CopyFile(e.FullPath, desdir_N(e.FullPath));
                    //}
                    //else if (Directory.Exists(e.FullPath))
                    //{
                    //    //如果第一个过来的是文件夹，则直接把该文件全部复制过去
                    //    //复制文件夹
                    //    FileIOTools.CopyFolder(e.FullPath, desdir_N(e.FullPath));
                    //}
                    //Runbat(AppSettings.GetValue("SVN_N"), "Commit.bat");
                    //Runbat(AppSettings.GetValue("SVN_W"), "Commit.bat");
                    ////完成之后再打开另一个bat
                    //watcher_W.EnableRaisingEvents = true;
                }
            }
            catch (IOException)
            {
                // this.BeginInvoke(new UpdateWatchTextDelegate(UpdateWatchText), "修改日志写入失败!");
            }
        }
        /// <summary>
        /// 重命名文件夹和文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                if (e.FullPath.Contains(".svn"))
                {
                    return;
                }
                else
                {
                    svn_w_flg = true;
                    WriteLog(String.Format("File renamed from {0} to {1}", e.OldName, e.FullPath));
                    //停掉另一个监听
                    watcher_N.EnableRaisingEvents = false;
                    //this.BeginInvoke(new UpdateWatchTextDelegate(UpdateWatchText), "文件" + e.OldName + "被重命名为" + e.FullPath);
                    FileIOTools.ReName(e.OldName, desdir_N(e.FullPath));
                    //删除Old
                    Runbat("\"" + e.OldFullPath + "\"", "Delete.bat");
                    //创建New
                    Runbat(e.FullPath, "Add.bat");
                    Runbat(AppSettings.GetValue("SVN_W"), "Commit.bat");
                    //删除Old
                    Runbat("\"" + desdir_N(e.OldFullPath) + "\"", "Delete.bat");
                    //创建New
                    Runbat(desdir_N(e.FullPath), "Add.bat");
                    Runbat(AppSettings.GetValue("SVN_N"), "Commit.bat");
                    //完成之后再打开另一个bat
                    watcher_N.EnableRaisingEvents = true;
                }
            }
            catch (IOException)
            {
                //this.BeginInvoke(new UpdateWatchTextDelegate(UpdateWatchText), "重命名日志写入失败!");
            }
        }
        /// <summary>
        /// 重命名文件夹和文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_Renamed_N(object sender, RenamedEventArgs e)
        {
            try
            {
                if (e.FullPath.Contains(".svn"))
                {
                    return;
                }
                else
                {
                    svn_w_flg = false;
                    WriteLog(String.Format("File renamed from {0} to {1}", e.OldName, e.FullPath));
                    //停掉另一个监听
                    watcher_W.EnableRaisingEvents = false;
                    FileIOTools.ReName(e.OldName, desdir_N(e.FullPath));
                    //删除Old
                    Runbat("\"" + e.OldFullPath + "\"", "Delete.bat");
                    Runbat(e.FullPath, "Add.bat");
                    Runbat(AppSettings.GetValue("SVN_N"), "Commit.bat");
                    //创建New
                    Runbat("\"" + desdir_N(e.OldFullPath) + "\"", "Delete.bat");
                    Runbat(desdir_N(e.FullPath), "Add.bat");
                    Runbat(AppSettings.GetValue("SVN_W"), "Commit.bat");
                    //完成之后再打开另一个bat
                    watcher_W.EnableRaisingEvents = true;
                }
            }
            catch (IOException)
            {
                //this.BeginInvoke(new UpdateWatchTextDelegate(UpdateWatchText), "重命名日志写入失败!");
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (e.FullPath.Contains(".svn"))
                {
                    return;
                }
                else
                {
                    svn_w_flg = true;
                    WriteLog(String.Format("{0} File: {1} Deleted",DateTime.Now, e.FullPath));
                    if (!list_delete_W.Contains(e.FullPath))
                    {
                        list_delete_W.Add(e.FullPath);
                    }
                    ////停掉另一个监听
                    //watcher_N.EnableRaisingEvents = false;
                    ////删除文件
                    //FileIOTools.DeleteFileOrDir(desdir_N(e.FullPath));
                    ////删除之后要执行delete.bat文件
                    ////Runbat(desdir_N(e.FullPath), "Delete.bat");
                    //Runbat("\"" + e.FullPath + "\"", "Delete.bat");
                    //Runbat("\"" + e.FullPath + "\"", "Commit.bat");
                    //Runbat("\"" + e.FullPath.Replace(AppSettings.GetValue("SVN_W"), AppSettings.GetValue("SVN_N")) + "\"", "Delete.bat");
                    //Runbat("\"" + e.FullPath.Replace(AppSettings.GetValue("SVN_W"), AppSettings.GetValue("SVN_N")) + "\"", "Commit.bat");
                    ////完成之后再打开另一个bat
                    //watcher_N.EnableRaisingEvents = true;
                }
            }
            catch (IOException)
            {

            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void watcher_Deleted_N(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (e.FullPath.Contains(".svn"))
                {
                    return;
                }
                else
                {
                    svn_w_flg = false;
                    WriteLog(String.Format("{0} File: {1} Deleted", DateTime.Now, e.FullPath));
                    if (!list_delete_N.Contains(e.FullPath))
                    {
                        list_delete_N.Add(e.FullPath);
                    }
                    ////停掉另一个监听
                    //watcher_W.EnableRaisingEvents = false;
                    ////删除文件
                    //FileIOTools.DeleteFileOrDir(desdir_N(e.FullPath));
                    ////删除之后要执行delete.bat文件
                    ////Runbat(desdir_N(e.FullPath), "Delete.bat");
                    //Runbat("\"" + e.FullPath + "\"", "Delete.bat");
                    //Runbat("\"" + e.FullPath + "\"", "Commit.bat");
                    //Runbat("\"" + e.FullPath.Replace(AppSettings.GetValue("SVN_N"), AppSettings.GetValue("SVN_W")) + "\"", "Delete.bat");
                    //Runbat("\"" + e.FullPath.Replace(AppSettings.GetValue("SVN_N"), AppSettings.GetValue("SVN_W")) + "\"", "Commit.bat"); 
                    ////完成之后再打开另一个bat
                    //watcher_W.EnableRaisingEvents = true;
                }
            }
            catch (IOException)
            {

            }
        }
        /// <summary>
        /// 填写日志文件
        /// </summary>
        /// <param name="LogContent"></param>
        public void WriteLog(string LogContent)
        {
            using (StreamWriter sw = new StreamWriter("c:\\Log.txt", true))
            {
                sw.WriteLine(LogContent);
                sw.Close();
            }

        }
        /// <summary>
        /// 得到内网的文件地址
        /// </summary>
        /// <param name="srcPath"></param>
        /// <returns></returns>
        public string desdir_N(string srcPath)
        {
            //创建了一个新文件，要将这个文件同步到代码仓库中去
            if (watcher_W.EnableRaisingEvents)
            {
                return srcPath.Replace(AppSettings.GetValue("SVN_W"), AppSettings.GetValue("SVN_N"));
            }
            else if (watcher_N.EnableRaisingEvents)
            {
                return srcPath.Replace(AppSettings.GetValue("SVN_N"), AppSettings.GetValue("SVN_W"));
            }
            else
                return "";
            
        }
        /// <summary>
        /// 停止监视
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;//关闭计时器
            watcher_W.EnableRaisingEvents = false;//监视外网代码仓库已关闭
            watcher_N.EnableRaisingEvents = false;//监视内网代码仓库已关闭
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (svn_w_flg)
            {
                //执行文件变动
                FileCaoZuo_W();
                //执行文件变动
                FileCaoZuo_N();
            }
            else
            {
                //执行文件变动
                FileCaoZuo_N();
                //执行文件变动
                FileCaoZuo_W();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //将选择的CheckOut地址写入配置文件
            AppSettings.SetValue("SVN_TIME", textBox4.Text);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //将选择的CheckOut地址写入配置文件
            AppSettings.SetValue("SlEEP_TIME", textBox3.Text);
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                notifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized) //判断是否最小化
            {
                this.ShowInTaskbar = false; //不显示在系统任务栏
                notifyIcon.Visible = true; //托盘图标可见
            }
        }
    }
}
