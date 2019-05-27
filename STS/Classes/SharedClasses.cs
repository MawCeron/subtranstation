using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace STS
{
    class SharedClasses
    {
        public static DataSet CheckSubFile(string filePath)
        {
            string[] supportedSubs = { ".srt", ".ass", ".ssa", ".tra" };

            string ext = Path.GetExtension(filePath);
            if (supportedSubs.Contains(ext))
            {
                DataSet loadedSub = new DataSet();
                switch (ext)
                {
                    case ".tra":
                        loadedSub = OpenSavedProject(filePath);
                        break;
                    case ".srt":
                        var srtFile = File.ReadAllText(filePath);
                        loadedSub = SubtitleParsers.ParseSubRip(srtFile);
                        break;
                    case ".ass":
                    case ".ssa":
                        var ssaFile = File.ReadAllText(filePath);
                        loadedSub = SubtitleParsers.ParseSubStationAlpha(ssaFile);
                        break;
                }

                return loadedSub;
            }
            else
            {
                string fileName = Path.GetFileName(filePath);
                string errorMsg = String.Format("Subtitle TranStation could not open \"{0}\" because it is not a supported file type.", fileName);
                DialogWindow errorDialog = new DialogWindow();
                errorDialog.DialogTitle = "Error opening the file";
                errorDialog.Message = errorMsg;
                errorDialog.Type = DialogWindow.ErrorType;
                errorDialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                errorDialog.Show();
                return null;
            }
        }

        public static DataSet OpenSavedProject(string filePath)
        {
            DataSet ds = new DataSet();
            ds.ReadXml(filePath);            
            return ds;
        }

        public static bool SaveProject(Stream filePath, DataSet subScript)
        {
            try
            {
                subScript.WriteXml(filePath);
                return true;
            } catch (Exception)
            {
                return false;
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

        internal static void ExportTranslation(string filePath, Stream openFile, DataSet subScript)
        {
            string[] validExtensions = { ".srt", ".ass" };
            try
            {
                string ext = Path.GetExtension(filePath);
                if (validExtensions.Contains(ext))
                {                    
                    switch (ext)
                    {
                        
                        case ".srt":
                            SubtitleExporting.ToSubRip(subScript, openFile);
                            break;
                        case ".ass":
                            SubtitleExporting.ToSubStationAlpha(subScript, openFile);
                            break;
                    }                    
                }
            }
            catch (Exception)
            {
                throw;
            }

            
        }

        internal static string VisualDialogue(string dialogue)
        {
            string html = String.Empty;
            html += "<body style='overflow:hidden;background-color:#1E1E1E'>";
            html += "<p style='font-size:15pt;font-family:segoe ui;color:#A7A7A7'>{0}</p>";
            html += "</body>";

            html = String.Format(html, dialogue);
            return html;
        }
    }
}
