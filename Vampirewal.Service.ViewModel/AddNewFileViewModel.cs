#region << 文 件 说 明 >>

/*----------------------------------------------------------------
// 文件名称：AddNewFileViewModel
// 创 建 者：杨程
// 创建时间：2021/11/10 14:20:50
// 文件版本：V1.0.0
// ===============================================================
// 功能描述：
//
//
//----------------------------------------------------------------*/

#endregion

using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Vampirewal.Core.Interface;
using Vampirewal.Core.SimpleMVVM;
using Vampirewal.Service.Model;

namespace Vampirewal.Service.ViewModel
{
    public class AddNewFileViewModel : BaseCRUDVM<ProgramModel>
    {
        private IDialogMessage Dialog { get; set; }

        public AddNewFileViewModel(IDataContext dc, IDialogMessage dialog, IAppConfig config) : base(dc, config)
        {
            //构造函数
            Dialog = dialog;
        }

        public override void InitData()
        {
            comboxValues = new ObservableCollection<ComboxValue>();
            GetProgramData();
            Title = "新增文件信息";
        }

        private bool IsResult = false;
        private bool isCanEdit = true;

        public override object GetResult()
        {
            return IsResult;
        }

        #region 属性

        public ObservableCollection<ComboxValue> comboxValues { get; set; }

        public ComboxValue SelectVaule { get; set; }

        public bool IsCanEdit { get => isCanEdit; set { isCanEdit = value; DoNotify(); } }
        #endregion



        #region 私有方法

        private void GetProgramData()
        {
            var current = DC.Set<ProgramModel>().Distinct().Select(s => new ComboxValue()
            {
                key = s.Token,
                Value = s.Name,
                Des = s.Description,
                Version = s.Version
            }).ToList();

            foreach (var item in current)
            {
                comboxValues.Add(item);
            }
        }

        #endregion

        #region 命令

        public override RelayCommand SaveCommand => new RelayCommand(() =>
          {
              if (SelectVaule != null)
              {

              }

              using (var trans = DC.Database.BeginTransaction())
              {
                  try
                  {
                      DoAdd();
                      trans.Commit();
                      Dialog.ShowPopupWindow("新增成功！", WindowsManager.Windows["MainView"], Core.WpfTheme.WindowStyle.MessageType.Successful);
                      IsResult = true;
                      ((Window)View).Close();
                  }
                  catch (Exception ex)
                  {
                      trans.Rollback();
                      Dialog.ShowPopupWindow($"新增失败！错误信息：{ex.Message}", (Window)View, Core.WpfTheme.WindowStyle.MessageType.Successful);

                  }
              }
          });

        /// <summary>
        /// 选择历史项目的命令
        /// </summary>
        public RelayCommand<ComboxValue> SelectOldDataCommand => new RelayCommand<ComboxValue>((c) =>
        {
            if (c != null)
            {
                SelectVaule = c;

                Entity.Token = c.key;
                Entity.Name = c.Value;
                Entity.Description = c.Des;


                IsCanEdit = false;
            }
        });

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
                Entity.ZipFilePath = ofd.FileName;
                
            }
        });

        #endregion
    }

    public class ComboxValue
    {
        public string key { get; set; }

        public string Value { get; set; }

        public string Des { get; set; }

        public string Version { get; set; }
    }
}
