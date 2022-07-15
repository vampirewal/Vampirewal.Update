﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Vampirewal.Core.SimpleMVVM;
using Vampirewal.Update.ClientApp.Model;

namespace Vampirewal.Update.ClientApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : VampirewalApplication
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }
        //public override Assembly CurrentAssembly()
        //{
        //    return Assembly.GetExecutingAssembly();
        //}

        protected override string FirstWindowName()
        {
            return ViewKeys.MainView;
        }


    }
}
