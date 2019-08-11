using IniFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RMTV_recorder
{
    /// <summary>
    /// Interaction logic for Info_UC.xaml
    /// </summary>
    public partial class Info_UC : ucCustom
    {
        private string release_version = "";
        private int click_count = 0;

        public Info_UC()
        {
            InitializeComponent();

#if DEBUG
            release_version = "Debug";
#else
            release_version = "Release";
#endif
            label_version.Content = GetVersion() + " (" + release_version + ")";
            label_author.Content = GetCopyright();

            //debug info
            label_struniqle.Content = GlobalVar._uniqueStr;
            RefreshDebugInfoVisibility();
        }

        private string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            object[] obj = assembly.GetCustomAttributes(false);
            foreach (object o in obj)
            {
                if (o.GetType() == typeof(System.Reflection.AssemblyFileVersionAttribute))
                {
                    AssemblyFileVersionAttribute ava = (AssemblyFileVersionAttribute)o;
                    return ava.Version;
                }
            }
            return string.Empty;
        }

        private string GetCopyright()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            object[] obj = assembly.GetCustomAttributes(false);
            foreach (object o in obj)
            {
                if (o.GetType() == typeof(System.Reflection.AssemblyCopyrightAttribute))
                {
                    AssemblyCopyrightAttribute aca = (AssemblyCopyrightAttribute)o;
                    return aca.Copyright;
                }
            }
            return string.Empty;
        }

        private void RefreshDebugInfoVisibility()
        {
            sp_debuginfo.Visibility = (GlobalVar._debugmode) ? Visibility.Visible : Visibility.Hidden;
        }

        private void page_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            click_count++;

            if (click_count == Parameter.debug_on_click)
            {
                GlobalVar._debugmode = !GlobalVar._debugmode;
                RefreshDebugInfoVisibility();
                CommonFunc.ToastMessage(label_message, 
                                        String.Format("Debug mode is {0}!", GlobalVar._debugmode ? "ON" : "OFF"),
                                        2);
                click_count = 0;
            }
        }
    }
}
