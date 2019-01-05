using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RMTV_recorder
{

    public partial class winCustom : Window
    {
        private ucCustom _usercontrol;
        private bool _IscloseButtonVisible = true;

        public winCustom()
        {
            InitializeComponent();
            this.Loaded += winCustom_Load;
            
        }

        private void winCustom_Load(object sender, RoutedEventArgs e)
        {
            if (!_IscloseButtonVisible)
            {
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
            }
        }

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public ucCustom winContent
        {
            set
            {
                _usercontrol = value;
                if (value == null)
                    return;

                if (_usercontrol is ucCustom)
                {
                    ((ucCustom)_usercontrol).CloseDialog += new ucCustom.CloseDialogHandler(Custom_CloseDialog);
                }

                this.SizeToContent = SizeToContent.WidthAndHeight;
                this.grid_body.Width = _usercontrol.Width;
                this.grid_body.Height = _usercontrol.Height;
                this.grid_body.Children.Add(_usercontrol);
            }
            get { return _usercontrol; }
        }

        

        public bool CloseButtonVisible
        {
            set
            {
                _IscloseButtonVisible = value;
            }
            get { return _IscloseButtonVisible; }
        }

        private void Custom_CloseDialog(object sender, bool bApply, EventArgs e)
        {
            if (bApply)
                this.DialogResult = true;
            else
                this.DialogResult = false;
        }
    }
}
