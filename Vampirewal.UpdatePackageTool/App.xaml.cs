using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Vampirewal.Core.SimpleMVVM;
using Vampirewal.UpdatePackageTool.Model;

namespace Vampirewal.UpdatePackageTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : VampirewalApplication
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            SetAssembly(Assembly.GetExecutingAssembly());
            base.OnStartup(e);
            
            OpenWinodw(ViewKeys.MainView);
        }
    }
}
