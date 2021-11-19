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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        #endregion
    }
}
