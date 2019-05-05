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

        public void DownloadESM3U8()
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile("https://rmtv24hweblive-lh.akamaihd.net/i/rmtv24hwebes_1@300661/master.m3u8", Parameter._m3u8_es_Path);
            }
        }

        public void DownloadENM3U8()
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile("https://rmtv24hweblive-lh.akamaihd.net/i/rmtv24hweben_1@300662/master.m3u8", Parameter._m3u8_en_Path);
            }
        }

    }
}
