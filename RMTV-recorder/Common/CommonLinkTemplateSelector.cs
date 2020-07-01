using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RMTV_recorder
{
    public class CommonLinkTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            ContentPresenter presenter = (ContentPresenter)container;

            if (presenter.TemplatedParent is ComboBox)
            {
                return (DataTemplate)presenter.FindResource("CommonLinkComboCollapsed");
            }
            else // Templated parent is ComboBoxItem
            {
                return (DataTemplate)presenter.FindResource("CommonLinkComboExpanded");
            }
        }
    }
}
