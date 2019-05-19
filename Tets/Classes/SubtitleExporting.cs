using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tets
{
    class SubtitleExporting
    {
        public static bool ToSubRip(DataTable translatedSubs, Stream exportedSubs)
        {
            try
            {                
                if (translatedSubs.Rows.Count > 0)
                {
                    StreamWriter newFile = new StreamWriter(exportedSubs);
                    for (int i = 0;i < translatedSubs.Rows.Count; ++i)
                    {                        
                        newFile.WriteLine(i + 1);
                        string timeCodes = String.Format(@"{0} --> {1}", translatedSubs.Rows[i]["Start"].ToString(), translatedSubs.Rows[i]["End"].ToString());
                        newFile.WriteLine(timeCodes);
                        string translation = translatedSubs.Rows[i]["Translation"].ToString();
                        if (!String.IsNullOrEmpty(translation))
                        {
                            string[] delimeters = { "||", "\\n" };
                            string[] dialogues = translation.Split(delimeters, StringSplitOptions.None);
                            foreach (string dialogue in dialogues)
                                newFile.WriteLine(dialogue.Trim());
                        } else
                        {
                            translation = translatedSubs.Rows[i]["Dialogue"].ToString();
                            string[] delimeters = { "||", "\\n" };
                            string[] dialogues = translation.Split(delimeters, StringSplitOptions.None);
                            foreach (string dialogue in dialogues)
                                newFile.WriteLine(dialogue.Trim());
                        }
                        newFile.WriteLine();
                    }
                    newFile.Dispose();
                    newFile.Close();
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
