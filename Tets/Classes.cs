using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Tets
{
    class Classes
    {
        public static DataTable CheckSubFile(string filePath)
        {
            string[] supportedSubs = { ".srt", ".ass" };

            string ext = Path.GetExtension(filePath);
            if (supportedSubs.Contains(ext))
            {
                DataTable loadedSub = new DataTable();
                switch (ext)
                {
                    case ".srt":
                        var subFile = File.ReadAllText(filePath);
                        loadedSub = SubtitleParsers.ParseSubRip(subFile);
                        break;
                }

                return loadedSub;
            }
            else
            {
                string fileName = Path.GetFileName(filePath);
                string errorMsg = String.Format("Subtitle TranStation could not open \"{0}\" because it is not a supported file type.", fileName);
                MessageBox.Show(errorMsg, "File Not Supported",MessageBoxButton.OK,MessageBoxImage.Error);
                return null;
            }
        }


        

        public static string GetGoogleTranslation(string dialog,string langFrom, string langTo)
        {
            string data = String.Empty;
            string encodedDialog = System.Net.WebUtility.UrlEncode(dialog);

            string urlAddress = String.Format(@"http://translate.google.com/m?hl={0}&sl={1}&q={2}",langTo,langFrom,encodedDialog);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (response.CharacterSet == null)
                {
                    readStream = new StreamReader(receiveStream);
                }
                else
                {
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                }

                data = readStream.ReadToEnd();               

                response.Close();
                readStream.Close();
            }

            var translation = ParseHTML(data);
            return translation;
        }

        private static string ParseHTML(string data)
        {
            string pattern = "class=\"t0\">(.*?)<";
            string result = String.Empty;

            var matches = Regex.Matches(data, pattern);

            if (matches.Count > 0)
                foreach (Match match in matches)
                    result += match.Groups[1];

            return result;
        }
    }
}
