#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：ServiceDataContext
// 创 建 者：杨程
// 创建时间：2021/11/10 12:28:21
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
using Vampirewal.Core.DBContexts;
using Vampirewal.Core.Interface;
using Vampirewal.Core.Models;
using Vampirewal.Service.Model;

namespace Vampirewal.Service.DataAccess
{
    public class ServiceDataContext : VampirewalDbBase
    {
        public ServiceDataContext(IAppConfig appConfig) : base(appConfig)
        {
            //构造函数
            //appConfig.Save();
        }

        /// <summary>
        /// 日志
        /// </summary>
        //public DbSet<Logger> Loggers
        //{
        //    get;
        //    set;
        //}

        //public DbSet<ProgramModel> ProgramModels { get; set; }

        //public DbSet<ProgramDtl> ProgramDtls { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    //optionsBuilder.UseSqlite("Data Source=VCodeGeneratorDataBase.db");
        //    optionsBuilder.UseSqlite(AppConfig.ConnectionStrings[0].Value);


        //}

        

        protected override void CodeFirst()
        {
            CreateTable<Logger>();
            CreateTable<ProgramModel>();
            CreateTable<ProgramDtl>();
        }

        
    }
}
