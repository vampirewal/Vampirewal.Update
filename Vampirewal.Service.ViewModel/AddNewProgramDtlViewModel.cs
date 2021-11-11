#region << 文 件 说 明 >>
/*----------------------------------------------------------------
// 文件名称：AddNewProgramDtlViewModel
// 创 建 者：杨程
// 创建时间：2021/11/11 14:25:02
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
using System.Windows.Forms;
using Vampirewal.Core.Interface;
using Vampirewal.Core.SimpleMVVM;
using Vampirewal.Service.Model;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Vampirewal.Service.ViewModel
{
    public class AddNewProgramDtlViewModel:BaseCRUDVM<ProgramDtl>
    {
        private IDialogMessage Dialog { get; set; }
        public AddNewProgramDtlViewModel(IDataContext dc,IDialogMessage dialog):base(dc)
        {
            //构造函数
            Dialog=dialog;
            

            Title = "新增更新文件";
        }

        private ProgramModel programModel { get; set; }
        public override void PassData(object obj)
        {
            ProgramModel model = obj as ProgramModel;
            if (model != null)
            {
                programModel = model;
                Entity.ProgramId = programModel.ID;
            }
        }

        private bool IsOK=false;
        public override object GetResult()
        {
            return IsOK;
        }

        #region 属性

        #endregion

        #region 公共方法

        #endregion

        #region 私有方法
        private bool CheckData()
        {
            if(DC.Set<ProgramDtl>().Any(w=>w.CurrentVersion== Entity.CurrentVersion))
            {
                Dialog.ShowPopupWindow("当前版本号和历史记录内重复！", (Window)View, Core.WpfTheme.WindowStyle.MessageType.Error);
                return false;
            }
            //var OldVersion=DC.Set<ProgramDtl>().Where(w=>w.ProgramId== ProgramModelId).OrderByDescending(o=>o.CreateTime).FirstOrDefault();

            
            return true;
        }

        
        #endregion

        #region 命令
        public RelayCommand SelectFileSavePathCommand => new RelayCommand(() =>
        {
            //FolderBrowserDialog fb = new FolderBrowserDialog();

            //if (fb.ShowDialog() == DialogResult.OK)
            //{
            //    Entity.ZipFilePath = fb.SelectedPath;
            //}

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "RAR文件(*.rar*)|*.rar*|ZIP文件(*.zip)|*.zip*";
            ofd.RestoreDirectory = true;
            ofd.FilterIndex = 1;
            ofd.Multiselect = false;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Entity.FilePath = ofd.FileName;

            }
        });

        public override RelayCommand SaveCommand => new RelayCommand(() => 
        {
            if (!CheckData())
            {
                return;
            }

            using (var trans=DC.Database.BeginTransaction())
            {
                try
                {
                    programModel.LatestVersion = Entity.CurrentVersion;
                    DC.UpdateEntity(programModel);

                    DoAdd();
                    trans.Commit();
                    IsOK = true;

                    ((Window)View).Close();
                }
                catch 
                {
                    trans.Rollback();
                    
                }
            }
        });
        #endregion
    }
}
