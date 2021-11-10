#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：ProgramModel
// 创 建 者：杨程
// 创建时间：2021/11/10 13:28:19
// 文件版本：V1.0.0
// ===============================================================
// 功能描述：
//		
//
//----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vampirewal.Core.Models;

namespace Vampirewal.Service.Model
{
    [Table("Program_Service")]
    public class ProgramModel : TopBaseModel
    {
        private string token = "";
        private string name = "";
        private string description = "";
        private string updateDescription = "";
        private string zipFilePath = "";
        private string version = "1.0.0.0";
        private bool isForcedUpdate = false;

        public ProgramModel()
        {

        }

        /// <summary>
        /// 客户端根据这个token来找到对应的程序
        /// </summary>
        public string Token { get => token; set { token = value; DoNotify(); } }
        /// <summary>
        /// 程序名称
        /// </summary>
        public string Name { get => name; set { name = value; DoNotify(); } }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get => description; set { description = value; DoNotify(); } }
        /// <summary>
        /// 更新描述
        /// </summary>
        public string UpdateDescription { get => updateDescription; set { updateDescription = value; DoNotify(); } }
        /// <summary>
        /// 文件在服务器的地址
        /// </summary>
        public string ZipFilePath { get => zipFilePath; set { zipFilePath = value; DoNotify(); } }
        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get => version; set { version = value; DoNotify(); } }
        /// <summary>
        /// 是否强制更新
        /// </summary>
        public bool IsForcedUpdate { get => isForcedUpdate; set { isForcedUpdate = value; DoNotify(); } }
    }
}
