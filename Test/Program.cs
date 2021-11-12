using ICSharpCode.SharpZipLib.Zip;
using RRQMCore.ByteManager;
using RRQMSocket;
using RRQMSocket.FileTransfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vampirelwal.Common;
using Vampirewal.Core;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.ReadKey();
            Console.WriteLine("Hello World!");
            CreateFileClient();

            byte[] decBytes = System.Text.Encoding.UTF8.GetBytes("LoadServerVersion-aaaa");
            fileClient.Send(decBytes);

            Console.ReadKey();
        }
        private static FileClient fileClient;

        public static void CreateFileClient()
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

                string[] strs=mes.Split('-');

                if (strs[0]== "ServerVersion")
                {
                    string ServerVersion=strs[1];
                    if(CustomVersion.CheckVersion(ServerVersion, "1.0.0.2"))
                    {
                        byte[] decBytes = System.Text.Encoding.UTF8.GetBytes("DownloadServerFile-aaaa");
                        fileClient.Send(decBytes);
                    }
                }

                if (strs[0]== "ServerFilePath")
                {
                    string filePath = strs[1];

                    if (fileClient != null && fileClient.Online)
                    {
                        UrlFileInfo urlFileInfo = UrlFileInfo.CreateDownload(filePath, TransferFlags.BreakpointResume | TransferFlags.QuickTransfer);
                        urlFileInfo.Message = "请求下载";//请求下载时，同时向服务器传递一个消息
                        fileClient.RequestTransfer(urlFileInfo);
                    }
                }


                
            };

            fileClient.BeforeFileTransfer += (object sender, FileOperationEventArgs e) =>
            {
                //当客户端下载、上传文件时，都会先经过该事件，且可以通过 e.TransferType进行判断传输类型。
                UrlFileInfo urlFileInfo = e.UrlFileInfo;//获取传输文件信息。

                if (e.TransferType == TransferType.Download)
                {
                    urlFileInfo.SaveFullPath = $"C:\\Users\\YangCheng\\Desktop\\youjing\\{urlFileInfo.FileName}";//此处赋值可直接决定下载文件的保存路径。
                }
            };

            fileClient.FinishedFileTransfer += (object sender, TransferFileMessageArgs e) =>
            {
                //当客户端下载、上传文件完成时，都会经过该事件，且可以通过 e.TransferType进行判断传输类型。
                UrlFileInfo urlFileInfo = e.UrlFileInfo;//获取传输完成文件信息。

                List<string> FilePath=Utils.GetAllFilePathRecursion(AppDomain.CurrentDomain.BaseDirectory,null);

                //Decompress(urlFileInfo.SaveFullPath, AppDomain.CurrentDomain.BaseDirectory);

                (new FastZip()).ExtractZip(urlFileInfo.SaveFullPath, AppDomain.CurrentDomain.BaseDirectory, "");
            };
            //声明配置
            var config = new FileClientConfig();

            //继承TcpClient配置
            //config.RemoteIPHost = new IPHost("192.168.93.128:9999");//远程IPHost
            config.RemoteIPHost = new IPHost("127.0.0.1:9999");//远程IPHost
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
        /// 解压缩
        /// </summary>
        /// <param name="sourceFile">源文件</param>
        /// <param name="targetPath">目标路经</param>
        public static bool Decompress(string sourceFile, string targetPath)
        {
            if (!File.Exists(sourceFile))
            {
                throw new FileNotFoundException(string.Format("未能找到文件 '{0}' ", sourceFile));
            }
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(sourceFile)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directorName = Path.Combine(targetPath, Path.GetDirectoryName(theEntry.Name));
                    string fileName = Path.Combine(directorName, Path.GetFileName(theEntry.Name));
                    // 创建目录
                    if (directorName.Length > 0)
                    {
                        Directory.CreateDirectory(directorName);
                    }
                    if (fileName != string.Empty)
                    {
                        using (FileStream streamWriter = File.Create(fileName))
                        {
                            int size = 4096;
                            byte[] data = new byte[4 * 1024];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else break;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
