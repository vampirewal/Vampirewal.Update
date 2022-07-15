#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：ProgramInfoViewModel
// 创 建 者：杨程
// 创建时间：2021/11/11 9:58:45
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
using System.Windows;
using Vampirewal.Core.Interface;
using Vampirewal.Core.SimpleMVVM;
using Vampirewal.Service.Model;

namespace Vampirewal.Service.ViewModel
{
    public class ProgramInfoViewModel : BillVM<ProgramModel>
    {
        private IDialogMessage Dialog { get; set; }
        public ProgramInfoViewModel(IDataContext dc, IAppConfig config, IDialogMessage dialog) : base(dc, config)
        {
            //构造函数
            Dialog = dialog;
        }

        ProgramModel pass { get; set; }

        public override void PassData(object obj)
        {
            ProgramModel pm = obj as ProgramModel;

            if (pm != null)
            {
                pass = pm;
                SetEntity(pass);
                pass.ProgramDtls.Clear();
                var programs = DC.Client.Queryable<ProgramDtl>().Where(w => w.ProgramId == Entity.BillId).OrderBy(o => o.CreateTime).ToList();

                foreach (var item in programs)
                {
                    Entity.ProgramDtls.Add(item);
                }


                //GetCurrentProgramDtls();
                IsCanEdit = false;

                Title = $"{Entity.Name} 信息";
            }
            else
            {
                IsCanEdit = true;
                Title = $"新增程序信息";
            }
        }

        private bool IsOk = false;
        public override object GetResult()
        {
            return IsOk;
        }



        #region 属性
        private bool _IsCanEdit;

        public bool IsCanEdit
        {
            get { return _IsCanEdit; }
            set { _IsCanEdit = value; DoNotify(); }
        }

        #endregion

        #region 公共方法

        #endregion

        #region 私有方法
        private void GetCurrentProgramDtls()
        {
            Entity.ProgramDtls.Clear();

            var currentlist = DC.Client.Queryable<ProgramDtl>().Where(w => w.ProgramId == Entity.BillId).OrderBy(o => o.CreateTime).ToList();
            foreach (var item in currentlist)
            {
                Entity.ProgramDtls.Add(item);
            }
        }
        #endregion

        #region 命令
        /// <summary>
        /// 新增更新信息命令
        /// </summary>
        public RelayCommand AddNewProgramDtlCommand => new RelayCommand(() =>
        {
            if (IsCanEdit)
            {
                return;
            }

            var GetResult = Dialog.OpenDialogWindow(new Core.WpfTheme.WindowStyle.DialogWindowSetting()
            {
                UiView = Messenger.Default.Send<FrameworkElement>("GetView", ViewKeys.AddNewProgramDtlView),
                WindowWidth = 400,
                WindowHeight = 450,
                IconStr = "",
                IsShowMaxButton = false,
                IsShowMinButton = false,
                PassData = Entity
            });

            if (Convert.ToBoolean(GetResult))
            {

                GetCurrentProgramDtls();
            }
        });

        /// <summary>
        /// 删除更新信息命令
        /// </summary>
        public RelayCommand DeleteProgramDtlCommand => new RelayCommand(() =>
        {
            if (!Entity.ProgramDtls.Any(a => a.Checked))
            {
                Dialog.ShowPopupWindow("请至少勾选1条信息！", (Window)View, Core.WpfTheme.WindowStyle.MessageType.Error);
                return;
            }
            else
            {

                try
                {
                    

                    foreach (var item in Entity.ProgramDtls.Where(w => w.Checked).ToList())
                    {
                        DC.DeleteEntity(item);
                        Entity.ProgramDtls.Remove(item);
                    }

                    if (!Entity.ProgramDtls.Any())
                    {
                        Entity.LatestVersion = "1.0.0.0";
                    }
                    else
                    {
                        var LastProgramDtl = Entity.ProgramDtls.OrderByDescending(o => o.CreateTime).FirstOrDefault();
                        Entity.LatestVersion = LastProgramDtl.CurrentVersion;
                    }
                    

                    DC.Client.CommitTran();

                    GetCurrentProgramDtls();
                }
                catch
                {
                    
                }

            }

        });

        public RelayCommand SaveCommand => new RelayCommand(() =>
       {

           try
           {
               
               DoAdd();
               
               IsOk = true;
               ((Window)View).Close();
           }
           catch
           {
               

           }

       });
        #endregion
    }
}
