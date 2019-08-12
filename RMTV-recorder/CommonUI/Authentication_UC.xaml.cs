using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace RMTV_recorder
{
    /// <summary>
    /// Interaction logic for Authentication_UC.xaml
    /// </summary>
    public partial class Authentication_UC : ucCustom
    {
        public Authentication_UC()
        {
            InitializeComponent();
            Loaded += Authentication_UC_Loaded;
        }

        private void Authentication_UC_Loaded(object sender, RoutedEventArgs e)
        {
            List<int> index = GetRandomListItems();

            for (int i = 0; i < 5; i++)
            {
                listview_c1.Items.Add(((Code)index.ElementAt(i)).ToString());
                listview_c2.Items.Add(((Code)index.ElementAt(i + 5)).ToString());
                listview_c3.Items.Add(((Code)index.ElementAt(i + 10)).ToString());
                listview_c4.Items.Add(((Code)index.ElementAt(i + 15)).ToString());
            }
        }

        private List<int> GetRandomListItems()
        {
            Random random = new Random();
            IEnumerable<int> numbers = Enumerable.Range(0, Enum.GetNames(typeof(Code)).Length);
            List<int> listNumbers = numbers.ToList();
            DoShuffle(listNumbers);

            return listNumbers;
        }

        public void DoShuffle(List<int> list)
        {
            Random random = new Random();
            int n = list.Count;

            for (int i = list.Count - 1; i > 1; i--)
            {
                int rnd = random.Next(i + 1);

                int value = list[rnd];
                list[rnd] = list[i];
                list[i] = value;
            }
        }

        public bool CheckAuthentication()
        {
            string keyFile = CommonFunc.GetKeyfilePath();
            if (!System.IO.File.Exists(keyFile))
            {
                return false;
            }
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string text = tb_code.Text;
            if (!text.Equals(""))
            {
                System.IO.File.Create(System.IO.Path.Combine(Parameter._resourceFullPath, text) + ".txt");
            }

            base.OnCloseDialog(this, CheckAuthentication(), e);
        }

        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            tb_code.Text = "";
            listview_c1.UnselectAll();
            listview_c2.UnselectAll();
            listview_c3.UnselectAll();
            listview_c4.UnselectAll();
        }

        private void listview_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as ListView).SelectedItems;
            if (item.Count > 0)
            {
                tb_code.Text += item[item.Count-1].ToString();
            }
        }

        public enum Code
        {
            i,
            am,
            not,
            a,
            crazy,
            //
            super,
            great,
            sergio,
            ramos,
            marcelo,
            //
            benzema,
            ronaldo,
            real,
            madrid,
            messi,
            //
            luka,
            modric,
            asensio,
            fan,
            lover,
        }
    }
}
