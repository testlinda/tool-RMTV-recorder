using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string path = GetPath();
            if (System.IO.File.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                MessageBox.Show("Please fill in the correct relative path in file path.ini.");
            }

            Shutdown(1);
            return;
        }

        public static string GetPath()
        {
            string strPath = "";
            string pathFile = "path.ini";
            if (File.Exists(pathFile))
            {
                using (StreamReader reader = new StreamReader(pathFile))
                {
                    strPath = reader.ReadLine() ?? "";
                }

                if (!strPath.Equals(""))
                {
                    strPath = AppDomain.CurrentDomain.BaseDirectory + strPath;
                }
            }
            else
            {
                System.IO.File.Create(pathFile);
            }

            return strPath;
        }
    }
}
