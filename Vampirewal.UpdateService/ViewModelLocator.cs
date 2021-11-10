﻿#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：ViewModelLocator
// 创 建 者：杨程
// 创建时间：2021/11/10 12:29:46
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
using Vampirewal.Service.DataAccess;
using Vampirewal.Service.ViewModel;

namespace Vampirewal.UpdateService
{
    public class ViewModelLocator : ViewModelLocatorBase
    {
        public override void InitLocator()
        {
            CustomIoC.Instance.Register<IDataContext, ServiceDataContext>();

            CustomIoC.Instance.Register<MainViewModel>();
            CustomIoC.Instance.Register<AddNewFileViewModel>();
        }

        public MainViewModel MainViewModel=>CustomIoC.Instance.GetInstance<MainViewModel>();
        
        public AddNewFileViewModel AddNewFileViewModel=>CustomIoC.Instance.GetInstanceWithoutCaching<AddNewFileViewModel>();
    }
}
