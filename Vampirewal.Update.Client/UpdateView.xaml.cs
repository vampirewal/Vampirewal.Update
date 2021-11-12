using ICSharpCode.SharpZipLib.Zip;
using RRQMCore.ByteManager;
using RRQMSocket.FileTransfer;
using RRQMSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Vampirewal.Core.WpfTheme.WindowStyle;
using Vampirewal.Core;

namespace Vampirewal.Update.Client
{
    /// <summary>
    /// UpdateView.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateView : WindowBase
    {
        /// <summary>
        /// 更新窗体
        /// </summary>
        /// <param name="Title">App中文名称</param>
        /// <param name="ServerIp">服务器IP地址</param>
        /// <param name="ServerProt">服务器端口号</param>
        /// <param name="AppVersion">本地程序版本号</param>
        /// <param name="AppToken">与服务器i对应的Token</param>
        public UpdateView(string Title, string ServerIp, string ServerProt, string AppVersion, string AppToken)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(Title))
            {
                throw new Exception("未传入更新窗体标题！");
            }
            if (string.IsNullOrEmpty(ServerIp))
            {
                throw new Exception("未传入更新服务器IP地址");
            }
            if (string.IsNullOrEmpty(ServerProt))
            {
                throw new Exception("未传入更新服务器端口号");
            }
            if (string.IsNullOrEmpty(AppVersion))
            {
                throw new Exception("未传入程序本地版本号");
            }
            if (string.IsNullOrEmpty(AppToken))
            {
                throw new Exception("未传入程序对应服务器的Token");
            }



            this.Title = $"{Title}更新程序";
            _AppToken = AppToken;
            CreateFileClient(ServerIp, ServerProt, AppVersion, AppToken);

            byte[] decBytes = System.Text.Encoding.UTF8.GetBytes($"LoadUpdateDes-{AppToken}-{AppVersion}");
            fileClient.Send(decBytes);

            
        }

        private string _AppToken { get; set; } = "";

        #region 更新使用
        private FileClient fileClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ServerIp"></param>
        /// <param name="ServerProt"></param>
        /// <param name="AppVersion"></param>
        /// <param name="AppToken"></param>
        private void CreateFileClient(string ServerIp,string ServerProt,string AppVersion,string AppToken)
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

                if (strs[0]== "ServerUpdateDes")
                {
                    string updateDes = strs[1];
                    

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.UpdateDes.Text = updateDes;

                        if (strs[1] == "当前版本无更新！")
                        {
                            StartUpdate.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            StartUpdate.Visibility = Visibility.Visible;
                        }
                    }));

                    
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

                (new FastZip()).ExtractZip(urlFileInfo.SaveFullPath, AppDomain.CurrentDomain.BaseDirectory, "");

                File.Delete(urlFileInfo.SaveFullPath);
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

        private void StartUpdate_Click(object sender, RoutedEventArgs e)
        {
            byte[] decBytes = System.Text.Encoding.UTF8.GetBytes($"LoadServerVersion-{_AppToken}");
            fileClient.Send(decBytes);
        }
    }
}
