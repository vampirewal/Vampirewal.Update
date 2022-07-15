#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：MainViewModel
// 创 建 者：杨程
// 创建时间：2021/11/19 16:11:30
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vampirewal.Core;
using Vampirewal.Core.Interface;
using Vampirewal.Core.SimpleMVVM;

namespace Vampirewal.UpdatePackageTool.ViewModels
{
    public class MainViewModel:ViewModelBase
    {
        private IDialogMessage Dialog { get; set; }
        public MainViewModel(IAppConfig config,IDialogMessage dialog):base(config)
        {
            //构造函数
            Dialog=dialog;
            Config.LoadAppConfig();

            Title = Config.AppChineseName;
        }

        #region 属性

        #endregion

        #region 公共方法

        #endregion

        #region 私有方法

        #endregion

        #region 命令
        public RelayCommand TextChangedCommand => new RelayCommand(() => 
        {
            Config.Save();
        });

        public RelayCommand SelectFloderCommand => new RelayCommand(() => 
        {
            FolderBrowserDialog fb=new FolderBrowserDialog();
            if(fb.ShowDialog() == DialogResult.OK)
            {
                Config.FileUploadOptions.UploadDir = fb.SelectedPath;

                Config.Save();
            }
        });

        public RelayCommand SelectFileCommand => new RelayCommand(() => 
        {
            //OpenFileDialog ofd=new OpenFileDialog()
            //{
            //    Multiselect = true,
            //};

            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}temp.zip"))
            {
                File.Delete($"{AppDomain.CurrentDomain.BaseDirectory}temp.zip");
            }

            //List<string> FileNames = new List<string>()
            //{
            //    "OperationHelper.DataAccess.dll",
            //    "OperationHelper.DataAccess.pdb",
            //    "OperationHelper.deps.json",
            //    "OperationHelper.dll",
            //    "OperationHelper.exe",
            //    "OperationHelper.Model.dll",
            //    "OperationHelper.Model.pdb",
            //    "OperationHelper.pdb",
            //    "OperationHelper.runtimeconfig.dev.json",
            //    "OperationHelper.runtimeconfig.json",
            //    "OperationHelper.ViewModel.dll",
            //    "OperationHelper.ViewModel.pdb",
            //    "Vampirewal.Core.VContainer.dll"
            //};

            List<string> FileNames = new List<string>()
            {
                "BlindDate.DataAccess.dll",
                "BlindDate.DataAccess.pdb",
                "BlindDate.deps.json",
                "BlindDate.dll",
                "BlindDate.exe",
                "BlindDate.Model.dll",
                "BlindDate.Model.pdb",
                "BlindDate.pdb",
                "BlindDate.runtimeconfig.dev.json",
                "BlindDate.runtimeconfig.json",
                "BlindDate.ViewModel.dll",
                "BlindDate.ViewModel.pdb",
                "BlindDate.ProxyModels.pdb",
                "BlindDate.ProxyModels.dll"
            };

            var TempPath = $"{AppDomain.CurrentDomain.BaseDirectory}Temp";

            Directory.CreateDirectory(TempPath);

            foreach (var item in FileNames)
            {
                var sub = item.Split('\\').ToList();

                string path = $"{Config.FileUploadOptions.UploadDir}\\{item}";

                File.Copy(path, $"{TempPath}\\{item}");


            }

            System.IO.Compression.ZipFile.CreateFromDirectory(TempPath, $"{AppDomain.CurrentDomain.BaseDirectory}temp.zip", System.IO.Compression.CompressionLevel.Fastest, false, Encoding.UTF8); //解压

            Directory.Delete(TempPath, true);

        });
        #endregion

        
    }
}
