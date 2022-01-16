#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：MainViewModel
// 创 建 者：杨程
// 创建时间：2021/11/12 15:53:29
// 文件版本：V1.0.0
// ===============================================================
// 功能描述：
//		
//
//----------------------------------------------------------------*/
#endregion

using RRQMCore.ByteManager;
using RRQMSocket.FileTransfer;
using RRQMSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vampirewal.Core.Interface;
using Vampirewal.Core.SimpleMVVM;
using System.IO;
using Vampirewal.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Tar;
using System.Windows.Forms;

namespace Vampirewal.Update.ClientApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(IAppConfig config) : base(config)
        {
            //构造函数
            config.LoadAppConfig();
            Title = Config.UpdateSetting.UpdateName;



            if (string.IsNullOrEmpty(Title))
            {
                throw new Exception("未传入更新窗体标题！");
            }
            if (string.IsNullOrEmpty(Config.UpdateSetting.ServerIp))
            {
                throw new Exception("未传入更新服务器IP地址");
            }
            if (string.IsNullOrEmpty(Config.UpdateSetting.ServerPort))
            {
                throw new Exception("未传入更新服务器端口号");
            }
            if (string.IsNullOrEmpty(Config.AppVersion))
            {
                throw new Exception("未传入程序本地版本号");
            }
            if (string.IsNullOrEmpty(Config.UpdateSetting.AppToken))
            {
                throw new Exception("未传入程序对应服务器的Token");
            }

            CreateFileClient(Config.UpdateSetting.ServerIp, Config.UpdateSetting.ServerPort, Config.AppVersion, Config.UpdateSetting.AppToken);

            byte[] decBytes = System.Text.Encoding.UTF8.GetBytes($"LoadUpdateDes-{Config.UpdateSetting.AppToken}-{ Config.AppVersion}");
            fileClient.Send(decBytes);
        }

        #region 属性
        private string _UpdateDes;

        public string UpdateDes
        {
            get { return _UpdateDes; }
            set { _UpdateDes = value; DoNotify(); }
        }

        private string _UpdateStateStr;

        public string UpdateStateStr
        {
            get { return _UpdateStateStr; }
            set { _UpdateStateStr = value; DoNotify(); }
        }

        private bool _IsCanUpdate;

        public bool IsCanUpdate
        {
            get { return _IsCanUpdate; }
            set { _IsCanUpdate = value; DoNotify(); }
        }

        private string ServerVersion { get; set; }
        #endregion

        #region 公共方法

        #endregion

        #region 私有方法

        #region 更新使用
        private FileClient fileClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ServerIp"></param>
        /// <param name="ServerProt"></param>
        /// <param name="AppVersion"></param>
        /// <param name="AppToken"></param>
        private void CreateFileClient(string ServerIp, string ServerProt, string AppVersion, string AppToken)
        {
            fileClient = new FileClient();


            fileClient.ConnectedService += (object sender, MesEventArgs e) =>
            {
                //成功连接到服务器
                //sender对象为FileClient类型
                FileClient client = (FileClient)sender;
            };

            fileClient.DisconnectedService += (object sender, MesEventArgs e) =>
            {
                //从服务器断开连接，当连接不成功时不会触发。
                //sender对象为FileClient类型
                FileClient client = (FileClient)sender;
            };

            fileClient.Received += (IClient sender, short? procotol, ByteBlock byteBlock) =>
            {
                //当服务器通过Send或SendAsync发送数据时，此处会收到，且协议procotol为null。
                //为性能考虑，并未去除协议头，所以解析数据应当偏移2个字节，长度也应当减少2个字节。
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 2, byteBlock.Len - 2);

                Console.WriteLine(mes);

                string[] strs = mes.Split('-');

                if (strs[0] == "ServerVersion")
                {
                    string ServerVersion = strs[1];
                    if (CheckVersion(ServerVersion, AppVersion))
                    {
                        this.ServerVersion = ServerVersion;

                        byte[] decBytes = System.Text.Encoding.UTF8.GetBytes($"DownloadServerFile-{AppToken}");
                        fileClient.Send(decBytes);
                    }
                }

                if (strs[0] == "ServerFilePath")
                {
                    string filePath = strs[1];

                    if (fileClient != null && fileClient.Online)
                    {
                        UrlFileInfo urlFileInfo = UrlFileInfo.CreateDownload(filePath, TransferFlags.BreakpointResume | TransferFlags.QuickTransfer);
                        urlFileInfo.Message = "请求下载";//请求下载时，同时向服务器传递一个消息
                        fileClient.RequestTransfer(urlFileInfo);
                    }
                }

                if (strs[0] == "ServerUpdateDes")
                {
                    string updateDes = strs[1];

                    UpdateDes = updateDes;

                    if (strs[1] == "当前版本无更新！")
                    {
                        IsCanUpdate = false;

                        UpdateCommand.RaiseCanExecuteChanged();
                    }
                    else
                    {
                        IsCanUpdate = true;

                        UpdateCommand.RaiseCanExecuteChanged();
                    }

                }


            };

            fileClient.BeforeFileTransfer += (object sender, FileOperationEventArgs e) =>
            {
                //当客户端下载、上传文件时，都会先经过该事件，且可以通过 e.TransferType进行判断传输类型。
                UrlFileInfo urlFileInfo = e.UrlFileInfo;//获取传输文件信息。

                if (e.TransferType == TransferType.Download)
                {
                    urlFileInfo.SaveFullPath = $"{AppDomain.CurrentDomain.BaseDirectory}{urlFileInfo.FileName}";//此处赋值可直接决定下载文件的保存路径。
                }
            };

            fileClient.FinishedFileTransfer += (object sender, TransferFileMessageArgs e) =>
            {
                //当客户端下载、上传文件完成时，都会经过该事件，且可以通过 e.TransferType进行判断传输类型。
                UrlFileInfo urlFileInfo = e.UrlFileInfo;//获取传输完成文件信息。

                List<string> FilePath = Utils.GetAllFilePathRecursion(AppDomain.CurrentDomain.BaseDirectory, null);

                //Decompress(urlFileInfo.SaveFullPath, AppDomain.CurrentDomain.BaseDirectory);

                //(new FastZip()).ExtractZip(urlFileInfo.SaveFullPath, AppDomain.CurrentDomain.BaseDirectory, "");
                //UnZipFile.UnZip(urlFileInfo.SaveFullPath, AppDomain.CurrentDomain.BaseDirectory);

                System.IO.Compression.ZipFile.ExtractToDirectory(urlFileInfo.SaveFullPath, AppDomain.CurrentDomain.BaseDirectory, Encoding.UTF8); //解压

                File.Delete(urlFileInfo.SaveFullPath);

                Config.AppVersion = this.ServerVersion;
                Config.Save();

            };
            //声明配置
            var config = new FileClientConfig();

            //继承TcpClient配置
            //config.RemoteIPHost = new IPHost("192.168.93.128:9999");//远程IPHost
            config.RemoteIPHost = new IPHost($"{ServerIp}:{ServerProt}");//远程IPHost
            config.BytePool = BytePool.Default;//设置内存池实例。
            config.BufferLength = 1024 * 64;//缓存池容量
            config.BytePoolMaxSize = 512 * 1024 * 1024;//单个线程内存池容量
            config.BytePoolMaxBlockSize = 20 * 1024 * 1024;//单个线程内存块限制
            config.Logger = new Log();//日志记录器，可以自行实现ILog接口。
            config.SeparateThreadReceive = false;//独立线程接收，当为true时可能会发生内存池暴涨的情况
            config.DataHandlingAdapter = new FixedHeaderDataHandlingAdapter();//设置数据处理适配器,此处只能设置FixedHeaderDataHandlingAdapter类
            config.OnlySend = false;//仅发送，即不开启接收线程（此设置严谨开启）。
            config.SeparateThreadSend = false;//在异步发送时，使用独立线程发送
            //config.SeparateThreadSendBufferLength = 1024 * 1024;// 独立线程发送缓存区长度。

            //继承TokenClient配置
            config.VerifyToken = "FileServer";//连接验证令箭，可实现多租户模式
            config.VerifyTimeout = 3 * 1000;//验证3秒超时

            //继承TcpRpcParser配置，以实现RPC交互
            //config.SerializeConverter = new BinarySerializeConverter();
            //config.ProxyToken = "FileServerRPC";//代理令箭，用于获取代理文件,或服务时需验证令箭

            //文件传输相关配置
            config.ReceiveDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}新建文件夹";//设置默认接收目录。
            config.Timeout = 10 * 1000;//文件传输单次请求超时为10秒钟
            //config.PacketSize = 1024 * 64;//每次上传或下载的包体长度，该值不能大于固定包头最大值，且受网络环境影响。

            //注入配置
            fileClient.Setup(config);
            try
            {
                //连接服务器
                fileClient.Connect();
                //fileClient.DiscoveryService();//需要RPC交互时需要发现服务。
                Console.WriteLine("连接成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        /// <summary>
        /// 版本验证
        /// </summary>
        /// <param name="ServerVersion"></param>
        /// <param name="ClientVersion"></param>
        /// <returns></returns>
        private bool CheckVersion(string ServerVersion, string ClientVersion)
        {
            //先处理服务器的版本号
            string[] ServerStrs = ServerVersion.Split('.');
            int Server1 = Convert.ToInt32(ServerStrs[0]);
            int Server2 = Convert.ToInt32(ServerStrs[1]);
            int Server3 = Convert.ToInt32(ServerStrs[2]);
            int Server4 = Convert.ToInt32(ServerStrs[3]);

            //处理客户端版本号
            string[] ClientStrs = ClientVersion.Split('.');
            int Client1 = Convert.ToInt32(ClientStrs[0]);
            int Client2 = Convert.ToInt32(ClientStrs[1]);
            int Client3 = Convert.ToInt32(ClientStrs[2]);
            int Client4 = Convert.ToInt32(ClientStrs[3]);

            //验证
            if (Server1 > Client1)
            {
                return true;
            }
            else if (Server1 == Client1)
            {
                if (Server2 > Client2)
                {
                    return true;
                }
                else if (Server2 == Client2)
                {
                    if (Server3 > Client3)
                    {
                        return true;
                    }
                    else if (Server3 == Client3)
                    {
                        if (Server4 > Client4)
                        {
                            return true;
                        }
                        else if (Server4 == Client4)
                        {
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        #endregion

        #region 命令
        public RelayCommand<string> UpdateCommand => new RelayCommand<string>((s) =>
        {
            byte[] decBytes = System.Text.Encoding.UTF8.GetBytes($"DownloadServerFile-{Config.UpdateSetting.AppToken}");
            fileClient.Send(decBytes);
        }, (c) =>
        {
            return IsCanUpdate;
        });


        public RelayCommand yasuoCommand => new RelayCommand(() =>
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            //fb.ShowDialog();
            if (fb.ShowDialog() == DialogResult.OK)
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(fb.SelectedPath, $"{AppDomain.CurrentDomain.BaseDirectory}test.zip", System.IO.Compression.CompressionLevel.Fastest, false, Encoding.UTF8); //压缩
            }
        });
        #endregion
    }

    /// <summary>
    /// 压缩文件
    /// </summary>
    public class ZipHelp
    {
        public string ZipName { get; set; }
        /// <summary>
        /// 压缩文件夹
        /// </summary>
        /// <param name="zipSourcePath">需要压缩的文件夹路径（全路径）</param>
        /// <param name="zipToFilePath">压缩后保存的路径且必须带后缀名如：D:\\aa.zip（如果为空字符则默认保存到同级文件夹名称为源文件名）</param>
        public void ZipFileMain(string zipSourcePath, string zipToFilePath)
        {
            string[] filenames = Directory.GetFiles(zipSourcePath);
            ZipName = zipSourcePath.Substring(zipSourcePath.LastIndexOf("\\") + 1);
            //定义压缩更目录对象
            Crc32 crc = new Crc32();
            ZipOutputStream s = new ZipOutputStream(File.Create(zipToFilePath.Equals("") ? zipSourcePath + ".zip" : zipToFilePath));

            s.SetLevel(6); // 设置压缩级别
            //递归压缩文件夹下的所有文件和字文件夹
            AddDirToDir(crc, s, zipSourcePath);

            s.Finish();
            s.Close();
        }
        /// <summary>
        /// 压缩单个文件
        /// </summary>
        /// <param name="zipSourcePath">需要压缩的文件路径(全路径)</param>
        /// <param name="zipToFilePath">压缩后保存的文件路径（如果是空字符则默认压缩到同目录下文件名为源文件名）</param>
        public void ZipByFile(string zipSourcePath, string zipToFilePath)
        {
            //定义压缩更目录对象
            Crc32 crc = new Crc32();
            string dirName = zipSourcePath.Substring(zipSourcePath.LastIndexOf("\\") + 1, zipSourcePath.LastIndexOf(".") - (zipSourcePath.LastIndexOf("\\") + 1)) + ".zip";
            ZipOutputStream s = new ZipOutputStream(File.Create(zipToFilePath.Equals("") ? zipSourcePath.Substring(0, zipSourcePath.LastIndexOf("\\")) + "\\" + dirName : zipToFilePath));
            s.SetLevel(6); // 设置压缩级别
            AddFileToDir(crc, s, zipSourcePath, 0);
            s.Finish();
            s.Close();
        }
        /// <summary>
        /// 压缩单个文件到指定压缩文件夹下(内部调用)
        /// </summary>
        /// <param name="crc"></param>
        /// <param name="s"></param>
        /// <param name="file">文件路径</param>
        public void AddFileToDir(Crc32 crc, ZipOutputStream s, string file, int dotype)
        {
            FileStream fs = File.OpenRead(file);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            string filename = "";
            if (dotype == 0)
                filename = file.Substring(file.LastIndexOf("\\") + 1);
            else
                filename = file.Substring(file.IndexOf(ZipName));
            ZipEntry entry = new ZipEntry(filename);
            entry.DateTime = DateTime.Now;
            entry.Size = fs.Length;
            fs.Close();
            crc.Reset();
            crc.Update(buffer);
            entry.Crc = crc.Value;
            s.PutNextEntry(entry);
            s.Write(buffer, 0, buffer.Length);
        }
        /// <summary>
        /// 递归文件夹层级（内部调用）
        /// </summary>
        /// <param name="crc"></param>
        /// <param name="s"></param>
        /// <param name="file"></param>
        public void AddDirToDir(Crc32 crc, ZipOutputStream s, string file)
        {
            //添加此文件夹下的文件
            string[] files = Directory.GetFiles(file);
            foreach (string i in files)
            {
                AddFileToDir(crc, s, i, 1);
            }
            //查询此文件夹下的子文件夹
            string[] dirs = Directory.GetDirectories(file);
            foreach (string i in dirs)
            {
                AddDirToDir(crc, s, i);
            }
        }
    }

    /// <summary>
    /// 解压文件
    /// </summary>
    public class UnZipFile
    {
        /// <summary>
        /// 解压文件方法
        /// </summary>
        /// <param name="UnSourceZip">源文件</param>
        /// <param name="UnZipToPath">解压到目录路径（如果为空字符则是解压到当前目录）</param>
        public static void UnZip(string UnSourceZip, string UnZipToPath)
        {
            //System.Text.Encoding encode = System.Text.Encoding.GetEncoding("gbk");
            //ZipStrings.CodePage = encode.CodePage;

            ZipInputStream s = new ZipInputStream(File.OpenRead(UnSourceZip));

            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {

                string fileName = Path.GetFileName(theEntry.Name);

                //生成解压目录
                if (!UnZipToPath.Equals(""))
                    Directory.CreateDirectory(UnZipToPath);
                else
                    UnZipToPath = UnSourceZip.Substring(0, UnSourceZip.LastIndexOf("\\") > 0 ? UnSourceZip.LastIndexOf("\\") : 0);
                if (fileName != String.Empty)
                {
                    //创建文件夹
                    int startIndex = 0;
                    while (theEntry.Name.IndexOf("/", startIndex) > 0)
                    {
                        //计算文件夹路径
                        string dirpath = theEntry.Name.Substring(0, theEntry.Name.IndexOf("/", startIndex));
                        //添加文件夹
                        Directory.CreateDirectory(UnZipToPath.Equals("") ? dirpath : UnZipToPath + "//" + dirpath);
                        startIndex = theEntry.Name.IndexOf("/", startIndex) + 1;
                    }
                    //解压文件到指定的目录
                    FileStream streamWriter = File.Create(UnZipToPath.Equals("") ? theEntry.Name : UnZipToPath + "//" + theEntry.Name);
                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }
                    streamWriter.Close();
                }
            }
            s.Close();
        }
    }
}
