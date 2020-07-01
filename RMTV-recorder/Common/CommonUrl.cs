using IniFile;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMTV_recorder
{
    public class CommonUrl
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Note { get; set; }      

    }

    public class CommonUrlCollection
    {
        public ObservableCollection<CommonUrl> UrlObjs;
        private int _count;
        public int Count
        {
            get
            {
                _count = UrlObjs.Count;
                return _count;
            }
        }

        public CommonUrlCollection()
        {
            UrlObjs = new ObservableCollection<CommonUrl>();
        }

        public void LoadIni()
        {
            bool result = true;
            string resultStr = "";
            string[] valueStr;

            resultStr = IniHelper.ReadValue(Parameter._iniSectionCommonLink, Parameter._iniKeyNumberOfItem, Parameter._setting_Path);
            if (resultStr.Equals(""))
            {
                result = IniHelper.WriteValue(Parameter._iniSectionCommonLink, Parameter._iniKeyNumberOfItem, "0", Parameter._setting_Path);
                resultStr = "0";
            }

            int count = 0;
            Int32.TryParse(resultStr, out count);

            if (count > 0)
            {
                string key = "";
                for (int i = 0; i < count; i++)
                {
                    key = Parameter._iniKeyUrl + (i + 1);
                    resultStr = IniHelper.ReadValue(Parameter._iniSectionCommonLink, key, Parameter._setting_Path);
                    resultStr = resultStr.Replace("\"", "");
                    valueStr = resultStr.Split(new string[] { "," }, StringSplitOptions.None);
                    if (valueStr.Length == 3)
                    {
                        AddKey(valueStr[0], valueStr[1], valueStr[2]);
                    }

                }
            }
        }

        public void SaveIni()
        {
            WriteKey(Parameter._iniKeyNumberOfItem, Count.ToString());

            string key = "";
            string value = "";
            for (int i = 0; i < Count; i++)
            {
                key = Parameter._iniKeyUrl + (i + 1);
                value = "\"" + UrlObjs[i].Name + "\"," + "\"" + UrlObjs[i].Url + "\"," + "\"" + UrlObjs[i].Note + "\"";
                WriteKey(key, value);
            }
        }

        public void WriteKey(string key, string value)
        {
            bool result = true;
            result = IniHelper.WriteValue(Parameter._iniSectionCommonLink, key, value, Parameter._setting_Path);
        }

        public void AddKey(string name, string url, string note)
        {
            int index = Count + 1;
            UrlObjs.Add(new CommonUrl
            {
                Index = index,
                Name = name,
                Url = url,
                Note = note,
            });
        }

        public void DeleteKey(CommonUrl obj)
        {
            string key = Parameter._iniKeyUrl + obj.Index;
            IniHelper.DeleteKey(Parameter._iniSectionCommonLink, key, Parameter._setting_Path);

            UrlObjs.Remove(obj);
        }
    }
}
