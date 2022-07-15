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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vampirewal.Core.Models;

namespace Vampirewal.Service.Model
{
    [Table("Program_Service")]
    public class ProgramModel : BillBaseModel
    {
        private string token = "";
        private string name = "";
        private string description = "";
        
        //private string zipFilePath = "";
        private string _LastestVersion = "1.0.0.0";
        

        public ProgramModel()
        {
            ProgramDtls = new ObservableCollection<ProgramDtl>();
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
        
        ///// <summary>
        ///// 文件在服务器的地址
        ///// </summary>
        //public string ZipFilePath { get => zipFilePath; set { zipFilePath = value; DoNotify(); } }
        /// <summary>
        /// 版本
        /// </summary>
        public string LatestVersion { get => _LastestVersion; set { _LastestVersion = value; DoNotify(); } }

        private ObservableCollection<ProgramDtl> _ProgramDtls;
        [NotMapped]
        public ObservableCollection<ProgramDtl> ProgramDtls
        {
            get { return _ProgramDtls; }
            set { _ProgramDtls = value; DoNotify(); }
        }


    }

    [Table("ProgramDtl_Service")]
    public class ProgramDtl : DetailBaseModel
    {
        /// <summary>
        /// 程序modelId
        /// </summary>
        public string ProgramId { get; set; } = "";

        /// <summary>
        /// 当前版本
        /// </summary>
        public string CurrentVersion { get; set; } = "";

        private string updateDescription = "";
        /// <summary>
        /// 更新描述
        /// </summary>
        public string UpdateDescription { get => updateDescription; set { updateDescription = value; DoNotify(); } }

        /// <summary>
        /// 更新文件路径
        /// </summary>
        public string FilePath { get; set; } = "";

        private bool isForcedUpdate = false;
        /// <summary>
        /// 是否强制更新
        /// </summary>
        public bool IsForcedUpdate { get => isForcedUpdate; set { isForcedUpdate = value; DoNotify(); } }

        private DateTime? _CreateTime;

        public DateTime? CreateTime
        {
            get
            {
                if (!_CreateTime.HasValue)
                {
                    _CreateTime = DateTime.Now;
                }

                return _CreateTime;
            }
            set
            {
                _CreateTime = value;
                DoNotify("CreateTime");
            }
        }

        private string _CreateBy;
        public string CreateBy
        {
            get
            {
                return _CreateBy;
            }
            set
            {
                _CreateBy = value;
                DoNotify();
            }
        }

    }
}
