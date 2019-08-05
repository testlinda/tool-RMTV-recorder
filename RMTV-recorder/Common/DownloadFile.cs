using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace RMTV_recorder
{
    class DownloadFile
    {
        public DownloadFile()
        {

        }

        public bool Download(string url, string path)
        {
            bool ret = true;
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, path);
                }
                catch
                {
                    ret = false;
                }
            }

            return ret;
        }

        public bool DownloadESM3U8()
        {
            return Download(Parameter.uri_RMTV_m3u8_es, Parameter._m3u8_es_Path);
        }

        public bool DownloadENM3U8()
        {
            return Download(Parameter.uri_RMTV_m3u8_en, Parameter._m3u8_en_Path);
        }

    }
}
