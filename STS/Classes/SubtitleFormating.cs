using System.Text.RegularExpressions;
using System;

namespace STS
{
    class SubtitleFormating
    {
        // Subtitle TranStation
        public static string SSTFormat(string dialogue)
        {
            string reformated = dialogue;
            return reformated;
        }

        // SubRip
        public static string SubRipFormat(string translation)
        {
            try
            {
                string reformated = translation;
                var sstFormat = @"{b},{\b},{i},{\i},{u},{\u}".Split(',');
                var srtFormat = @"<b>,<\b>,<i>,<\i>,<u>,<\u>".Split(',');

                for (int i = 0; i < sstFormat.Length; i++)
                {
                    reformated = reformated.Replace(sstFormat[i], srtFormat[i]);
                }

                Match match = Regex.Match(reformated, "c:#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3}|[A-Fa-f0-9]{8})");
                if (match.Success)
                {                    
                    string[] elements = match.Value.ToString().Split(':');

                    string openTag = "{" + match.Value.ToString() + "}";
                    string closeTag = "{\\" + match.Value.ToString() + "}";
                    string newTag = String.Format("<font color=\"{0}\">", elements[1]);
                    reformated = reformated.Replace(openTag, newTag);
                    reformated = reformated.Replace(closeTag, "<\\font>");
                }


                return reformated;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        // SubstationAlpha
        public static string SubStationAlphaFormat(string translation)
        {
            string reformated = translation;           

            return reformated;
        }
    }
}
