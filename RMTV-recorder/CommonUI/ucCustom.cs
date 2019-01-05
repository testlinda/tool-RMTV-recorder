using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;


namespace RMTV_recorder
{
    public partial class ucCustom : UserControl
    {
        public delegate void CloseDialogHandler(object sender, bool bResult, EventArgs e);
        public event CloseDialogHandler CloseDialog;

        protected virtual void OnCloseDialog(object sender, bool bResult, EventArgs e)
        {
            if (CloseDialog != null)
                CloseDialog(sender, bResult, e);
        }
    }
}
