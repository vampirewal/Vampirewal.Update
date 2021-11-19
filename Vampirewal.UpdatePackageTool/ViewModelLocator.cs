#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：ViewModelLocator
// 创 建 者：杨程
// 创建时间：2021/11/19 16:04:52
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
using Vampirewal.Core.Components;
using Vampirewal.Core.Interface;
using Vampirewal.Core.SimpleMVVM;
using Vampirewal.UpdatePackageTool.ViewModels;

namespace Vampirewal.UpdatePackageTool
{
    public class ViewModelLocator:ViewModelLocatorBaseEmpty
    {
        public override void InitLocator()
        {
            CustomIoC.Instance.Register<IAppConfig, VampirewalConfig>();
            CustomIoC.Instance.Register<IDialogMessage, VampirewalDialog>();

            CustomIoC.Instance.Register<MainViewModel>();
        }

        public MainViewModel MainViewModel=>CustomIoC.Instance.GetInstance<MainViewModel>();
    }
}
