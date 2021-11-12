#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：MainViewModel
// 创 建 者：杨程
// 创建时间：2021/11/10 12:25:37
// 文件版本：V1.0.0
// ===============================================================
// 功能描述：
//		
//
//----------------------------------------------------------------*/
#endregion

using RRQMCore.ByteManager;
using RRQMSocket;
using RRQMSocket.FileTransfer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vampirelwal.Common;
using Vampirewal.Core;
using Vampirewal.Core.Interface;
using Vampirewal.Core.SimpleMVVM;
using Vampirewal.Service.Model;

namespace Vampirewal.Service.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IDialogMessage Dialog { get; set; }
        public MainViewModel(IAppConfig appConfig, IDialogMessage dialog, IDataContext dc) : base(dc, appConfig)
        {
            //构造函数
            Dialog = dialog;

            Title = Config.AppChineseName;

            CreateFileService(Config.AppSettings["ServerIp"], Config.AppSettings["ServerPort1"], Convert.ToInt32(Config.AppSettings["ServerPort2"]));
        }
        public override void InitData()
        {
            Programs = new ObservableCollection<ProgramModel>();
            GetProgramsData();
        }

        #region 属性
        public ObservableCollection<ProgramModel> Programs { get; set; }
        #endregion

        #region 公共方法

        #endregion

        #region 私有方法
        private void GetProgramsData()
        {
            Programs.Clear();
            var current = DC.Set<ProgramModel>().OrderByDescending(o => o.CreateTime).ToList();
            foreach (var item in current)
            {
                Programs.Add(item);
            }
        }
        #endregion

        #region 命令
        public RelayCommand OpenAddNewFileViewCommand => new RelayCommand(() =>
        {
            //var GetResult= Dialog.OpenDialogWindow(new Core.WpfTheme.WindowStyle.DialogWindowSetting()
            //{
            //    UiView=Messenger.Default.Send<FrameworkElement>("GetView",ViewKeys.AddNewFileView),
            //    WindowWidth=400,
            //    WindowHeight=500,
            //    IconStr = "",
            //    IsShowMaxButton = false,
            //    IsShowMinButton = false,
            //});

            //if (Convert.ToBoolean(GetResult))
            //{
            //    GetProgramsData();
            //}

            var GetResult = Dialog.OpenDialogWindow(new Core.WpfTheme.WindowStyle.DialogWindowSetting()
            {
                UiView = Messenger.Default.Send<FrameworkElement>("GetView", ViewKeys.ProgramInfoView),
                WindowWidth = 600,
                WindowHeight = 600,
                IconStr = "",
                IsShowMaxButton = false,
                IsShowMinButton = false,
                PassData = null
            });

            if (Convert.ToBoolean(GetResult))
            {
                GetProgramsData();
            }

        });

        private bool _IsStartOk;

        public bool IsStartOk
        {
            get { return _IsStartOk; }
            set { _IsStartOk = value; DoNotify(); }
        }

        public RelayCommand StartRunServer => new RelayCommand(() =>
        {
            try
            {
                fileService.Start();
                Console.WriteLine("启动成功");
                IsStartOk = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        });

        public RelayCommand StopRunServerCommand => new RelayCommand(() =>
        {
            try
            {
                fileService.Stop();
                Console.WriteLine("停止成功");
                IsStartOk = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        });

        /// <summary>
        /// 查看程序详细命令
        /// </summary>
        public RelayCommand<ProgramModel> LookDetailedCommand => new RelayCommand<ProgramModel>((p) =>
        {
            var GetResult2 = Dialog.OpenDialogWindow(new Core.WpfTheme.WindowStyle.DialogWindowSetting()
            {
                UiView = Messenger.Default.Send<FrameworkElement>("GetView", ViewKeys.ProgramInfoView),
                WindowWidth = 600,
                WindowHeight = 600,
                IconStr = "",
                IsShowMaxButton = false,
                IsShowMinButton = false,
                PassData = p
            });
        });
        #endregion

        #region TcpService
        FileService fileService { get; set; }
        public void CreateFileService(string Ip, string Port1, int Port2)
        {
            fileService = new FileService();

            fileService.ClientConnected += (object sender, MesEventArgs e) =>
            {
                //有客户端连接
                //sender对象为FileSocketClient类型
                FileSocketClient socketClient = (FileSocketClient)sender;
            };

            fileService.ClientDisconnected += (object sender, MesEventArgs e) =>
            {
                //有客户端断开连接
                //sender对象为FileSocketClient类型
                FileSocketClient socketClient = (FileSocketClient)sender;
            };

            fileService.Received += (IClient sender, short? procotol, ByteBlock byteBlock) =>
            {
                //当客户端通过Send或SendAsync发送数据时，此处会收到，且协议procotol为null。
                //为性能考虑，并未去除协议头，所以解析数据应当偏移2个字节，长度也应当减少2个字节。
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 2, byteBlock.Len - 2);

                string[] Msgs=mes.Split('-');
                if (Msgs[0]=="LoadServerVersion")
                {
                    string Token = Msgs[1];
                    var cur = DC.Set<ProgramModel>().Where(w => w.Token == Token).FirstOrDefault();
                    if (cur != null)
                    {
                        sender.SendAsync(Encoding.UTF8.GetBytes($"ServerVersion-{cur.LatestVersion}"));
                    }
                    else
                    {
                        sender.SendAsync(Encoding.UTF8.GetBytes("Error"));
                    }
                }

                if (Msgs[0]=="DownloadServerFile")
                {
                    string Token = Msgs[1];
                    var cur = DC.Set<ProgramModel>().Where(w => w.Token == Token).FirstOrDefault();
                    if (cur != null)
                    {
                        var dtl=DC.Set<ProgramDtl>().Where(w =>w.ProgramId==cur.ID&&w.CurrentVersion==cur.LatestVersion).FirstOrDefault();

                        sender.SendAsync(Encoding.UTF8.GetBytes($"ServerFilePath-{dtl.FilePath}"));
                    }
                    else
                    {
                        sender.SendAsync(Encoding.UTF8.GetBytes("Error"));
                    }
                }

                if (Msgs[0]=="LoadUpdateDes")
                {
                    string Token = Msgs[1];
                    string ClientVersion=Msgs[2];
                    var cur = DC.Set<ProgramModel>().Where(w => w.Token == Token).FirstOrDefault();
                    if (cur != null)
                    {

                        var dtl = DC.Set<ProgramDtl>().Where(w => w.ProgramId == cur.ID && w.CurrentVersion == cur.LatestVersion).FirstOrDefault();
                        if (CustomVersion.CheckVersion(dtl.CurrentVersion, ClientVersion))
                        {
                            sender.SendAsync(Encoding.UTF8.GetBytes($"ServerUpdateDes-{dtl.UpdateDescription}"));
                        }
                        else
                        {
                            sender.SendAsync(Encoding.UTF8.GetBytes($"ServerUpdateDes-当前版本无更新！"));
                        }
                        
                    }
                    else
                    {
                        sender.SendAsync(Encoding.UTF8.GetBytes("Error"));
                    }
                }
            };

            fileService.BeforeFileTransfer += (object sender, FileOperationEventArgs e) =>
            {
                //当客户端下载、上传文件时，都会先经过该事件，且可以通过 e.TransferType进行判断传输类型。
                UrlFileInfo urlFileInfo = e.UrlFileInfo;//获取传输文件信息。

                if (e.TransferType == TransferType.Upload)
                {
                    urlFileInfo.SaveFullPath = "C:\\1.txt";//此处赋值可直接决定上传文件的保存路径。
                }
                e.IsPermitOperation = true;//该值决定服务器是否同意本次传输
            };

            fileService.FinishedFileTransfer += (object sender, TransferFileMessageArgs e) =>
            {
                //当客户端下载、上传文件完成时，都会经过该事件，且可以通过 e.TransferType进行判断传输类型。
                UrlFileInfo urlFileInfo = e.UrlFileInfo;//获取传输完成文件信息。
            };
            //声明配置
            var config = new FileServiceConfig();

            //继承TcpService配置
            config.ListenIPHosts = new IPHost[] { new IPHost($"{Ip}:{Port1}"), new IPHost(Port2) };//同时监听两个地址
            config.BufferLength = 1024 * 64;//缓存池容量
            config.BytePoolMaxSize = 512 * 1024 * 1024;//单个线程内存池容量
            config.BytePoolMaxBlockSize = 20 * 1024 * 1024;//单个线程内存块限制
            config.Logger = new Log();//日志记录器，可以自行实现ILog接口。
            config.ServerName = "Vampirewal更新服务器";//服务名称
            config.SeparateThreadReceive = false;//独立线程接收，当为true时可能会发生内存池暴涨的情况
            config.ThreadCount = 5;//多线程数量，当SeparateThreadReceive为false时，该值只决定BytePool的数量。
            config.Backlog = 30;
            config.ClearInterval = 60 * 1000;//60秒无数据交互会清理客户端
            config.ClearType = ClearType.Receive | ClearType.Send;//清理统计
            config.MaxCount = 10000;//最大连接数

            //继承TokenService配置
            config.VerifyToken = "FileServer";//连接验证令箭，可实现多租户模式
            config.VerifyTimeout = 3 * 1000;//验证3秒超时

            //继承ProcotolService配置
            config.CanResetID = true;//是否重新设置ID

            //继承TcpRpcParser配置，以实现RPC交互
            //config.SerializationSelector = new 
            //config.NameSpace = "RRQMTest";
            //config.RPCVersion = new Version(1, 0, 0, 0);//定义此次发布的RPC版本。
            //config.ProxyToken = "FileServerRPC";//代理令箭，当客户端获取代理文件,或服务时需验证令箭

            //文件传输相关配置

            //允许下载的最大速度，此处只当作默认值，后续通过FileSocketClient对象可以对单个连接进行设置
            config.MaxDownloadSpeed = 1024 * 1024 * 10L;
            //允许上传的最大速度，此处只当作默认值，后续通过FileSocketClient对象可以对单个连接进行设置
            config.MaxUploadSpeed = 1024 * 1024 * 10L;

            fileService.Setup(config);


        }
        #endregion
    }
}
