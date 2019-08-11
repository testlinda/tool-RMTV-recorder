using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMTV_recorder
{
    public class M3U8Obj
    {
        public string ProgramId { get; private set; }
        public int Bandwidth { get; private set; }
        public string Resolution { get; private set; }
        public string Codecs { get; private set; }
        public string Path { get; private set; }

        public M3U8Obj(string feature, string link)
        {
            string[] parts = feature.Split(',');
            ProgramId = parts[0].Split('=')[1];
            Bandwidth = Convert.ToInt32(parts[1].Split('=')[1]);
            Resolution = parts[2].Split('=')[1];
            Codecs = string.Join(", ", parts[3].Split('=')[1], parts[4]);
            Path = link;
        }
    }

    public class M3U8
    {
        public bool InitialM3U8List()
        {
            bool ret = true;
            ret &= ParseM3U8(Parameter._m3u8_es_Path, ref GlobalVar._rmtv_link_es);
            ret &= ParseM3U8(Parameter._m3u8_en_Path, ref GlobalVar._rmtv_link_en);
            SortList(ref GlobalVar._rmtv_link_es);
            SortList(ref GlobalVar._rmtv_link_en);

            return ret;
        }

        public bool ParseM3U8(string m3u8Path, ref List<M3U8Obj> links)
        {
            links = null;
            links = new List<M3U8Obj>();

            string strm3u8 = "";
            using (StreamReader reader = new StreamReader(m3u8Path))
            {
                strm3u8 = reader.ReadToEnd();
            }

            string[] lines = strm3u8.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (line.Contains("EXT-X-STREAM-INF"))
                {
                    string line2 = lines[i + 1];

                    links.Add(new M3U8Obj(line, line2));
                    i = i + 1;
                }
            }

            return (links.Count > 0);
        }

        public void SortList(ref List<M3U8Obj> list)
        {
            list.Sort((x, y) => { return -x.Bandwidth.CompareTo(y.Bandwidth); });
        }
    }
}
