#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：ViewKeys
// 创 建 者：杨程
// 创建时间：2021/11/10 12:46:00
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

namespace Vampirewal.Service.Model
{
    public class ViewKeys
    {
        private static readonly string ViewBase = "Vampirewal.UpdateService";

        /// <summary>
        /// 主窗体
        /// </summary>
        public static string MainView = $"{ViewBase}.MainWindow";

        /// <summary>
        /// 新增新的文件信息页面
        /// </summary>
        public static string AddNewFileView = $"{ViewBase}.AddNewFileView";
    }
}
