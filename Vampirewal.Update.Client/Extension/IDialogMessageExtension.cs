#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：IDialogMessageExtension
// 创 建 者：杨程
// 创建时间：2021/11/12 14:33:54
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

namespace Vampirewal.Update.Client.Extension
{
    public static class IDialogMessageExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="Title">App中文名称</param>
        /// <param name="ServerIp">服务器IP地址</param>
        /// <param name="ServerProt">服务器端口号</param>
        /// <param name="AppVersion">本地程序版本号</param>
        /// <param name="AppToken">与服务器i对应的Token</param>
        public static void OpenUpdateWindow(this IDialogMessage dialog, string Title, string ServerIp, string ServerProt, string AppVersion, string AppToken)
        {
            UpdateView updateView = new UpdateView(Title, ServerIp, ServerProt, AppVersion, AppToken);
            updateView.ShowDialog();
        }
    }
}
