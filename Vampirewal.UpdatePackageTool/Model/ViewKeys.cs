#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：ViewKeys
// 创 建 者：杨程
// 创建时间：2021/11/19 16:09:14
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

namespace Vampirewal.UpdatePackageTool.Model
{
    public class ViewKeys
    {

        private static readonly string BaseViewKey = "Vampirewal.UpdatePackageTool";

        /// <summary>
        /// 主窗体
        /// </summary>
        public static readonly string MainView = $"{BaseViewKey}.MainWindow";
    }
}
