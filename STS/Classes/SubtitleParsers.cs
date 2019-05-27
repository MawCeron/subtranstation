using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace STS
{
    class SubtitleParsers
    {
        private static DataSet subScript = new DataSet();              
        
        public static DataSet ParseSubRip(string subFile)
        {
            try
            {
                DataTable loadedSubs = new DataTable();
                loadedSubs.Columns.Add("Start");
                loadedSubs.Columns.Add("End");
                loadedSubs.Columns.Add("Text");
                loadedSubs.Columns.Add("Translation");
                loadedSubs.Columns["Translation"].DefaultValue = String.Empty;

                var lines = Regex.Split(subFile, "\r\n\r\n");
                if (lines.Length < 2)
                    lines = Regex.Split(subFile, "\n\n");

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string subPart = line;
                    while (subPart.IndexOf("\n") < 1)
                    {
                        subPart = subPart.Remove(0, 1);
                    }
                    string[] elements = Regex.Split(subPart, "\n");

                    int x;
                    bool isNumeric = Int32.TryParse(elements[0], out x);
                    if (!isNumeric)
                        throw new Exception();

                    DataRow dialog = loadedSubs.NewRow();
                    string[] delimiters = { "-->", "- >", "->" };
                    string[] times = elements[1].Split(delimiters, StringSplitOptions.None);

                    if (ValidateTimeCodes(times))
                        throw new Exception();

                    dialog[0] = times[0].Trim();
                    dialog[1] = times[1].Trim();

                    string text = "";
                    for (int i = 2; i < elements.Length; i++)
                    {
                        text += elements[i].Trim() + "<br />";
                    }

                    text = text.Remove(text.Length - 6);
                    dialog[2] = text;

                    loadedSubs.Rows.Add(dialog);
                }

                if (loadedSubs.Rows.Count == 0)
                    throw new Exception();

                loadedSubs.TableName = "Dialogue";
                subScript.Tables.Add(loadedSubs);

                return subScript;
            }
            catch (Exception)
            {
                string errorMsg = @"This is not a valid SubRip file. Please try again.";
                DialogWindow errorDialog = new DialogWindow();
                errorDialog.DialogTitle = "Incorrect subtitle format";
                errorDialog.Message = errorMsg;
                errorDialog.Type = DialogWindow.ErrorType;
                errorDialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;               
                errorDialog.Show();
                return null;
            }
        }

        public static DataSet ParseSubStationAlpha(string subFile)
        {
            string[] neededSections = { "[Script Info]", "[V4+ Styles]", "[V4 Styles]", "[Events]" };
            DataSet subScript = new DataSet();
            DataTable info = new DataTable();
            DataTable styles = new DataTable();
            DataTable events = new DataTable();

            try
            {
                string[] sections = Regex.Split(subFile, "\r\n\r\n");
                if (sections.Length < 2)
                    sections = Regex.Split(subFile, "\n\n");

                foreach (string section in sections)
                {
                    string[] elements = section.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    if (neededSections.Contains(elements[0]))
                    {
                        switch (elements[0])
                        {
                            case "[Script Info]":
                                if (info.Rows.Count > 0)
                                    throw new Exception();


                                for (int i = 1; i < elements.Length; i++)
                                {
                                    if (elements[i].StartsWith(";"))
                                        continue;

                                    string[] keys = elements[i].Split(':');
                                    DataColumn column = new DataColumn(keys[0].Trim());
                                    column.DefaultValue = keys[1].Trim();
                                    info.Columns.Add(column);
                                }

                                DataRow infoRow = info.NewRow();
                                info.Rows.Add(infoRow);
                                break;

                            case "[V4+ Styles]":
                            case "[V4 Styles]":
                                for (int i = 1; i < elements.Length; i++)
                                {
                                    if (elements[i].StartsWith(";"))
                                        continue;

                                    string[] keys = elements[i].Split(':');
                                    int fCount = 0;
                                    switch (keys[0])
                                    {
                                        case "Format":
                                            if (fCount > 0)
                                                throw new Exception();
                                            string[] names = keys[1].Split(',');
                                            foreach (string name in names) { styles.Columns.Add(name.Trim()); }
                                            fCount++;
                                            break;

                                        case "Style":
                                            string[] values = keys[1].Trim().Split(',');
                                            styles.Rows.Add(values);
                                            break;
                                    }
                                }
                                break;

                            case "[Events]":
                                for (int i = 1; i < elements.Length; i++)
                                {
                                    if (elements[i].StartsWith(";"))
                                        continue;

                                    string[] keys = Regex.Split(elements[i], ": ");
                                    int fCount = 0;
                                    switch (keys[0])
                                    {
                                        case "Format":
                                            if (fCount > 0)
                                                throw new Exception();
                                            string[] names = keys[1].Trim().Split(',');
                                            foreach (string name in names) { events.Columns.Add(name.Trim()); }
                                            fCount++;
                                            break;

                                        case "Dialogue":
                                        case "Comment":
                                            string[] values = keys[1].Split(',');
                                            DataRow eventRow = events.NewRow();
                                            for (int j = 0; j < 10; j++)
                                            {
                                                if (j == 9)
                                                {
                                                    string text = string.Join(", ", values.Skip(9)).Replace("\\N", " \\N ");
                                                    eventRow[j] = text;
                                                    continue;
                                                }

                                                eventRow[j] = values[j];
                                            }
                                            events.Rows.Add(eventRow);
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }

                if (!SubStationAlphaVerification(styles, events))
                    throw new Exception();                

                info.TableName = "Info";
                styles.TableName = "Style";
                events.Columns.Add("Translation");
                events.TableName = "Dialogue";
                subScript.Tables.Add(info);
                subScript.Tables.Add(styles);
                subScript.Tables.Add(events);

                return subScript;
            }
            catch (Exception)
            {
                string errorMsg = @"This is not a valid SubStation Alpha file. Please try again.";
                DialogWindow errorDialog = new DialogWindow();
                errorDialog.DialogTitle = "Incorrect subtitle format";
                errorDialog.Message = errorMsg;
                errorDialog.Type = DialogWindow.ErrorType;
                errorDialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                errorDialog.Show();
                return null;
            }
        }

        private static bool ValidateTimeCodes(string[] times)
        {
            TimeSpan initial = new TimeSpan(0, 0, 0);
            TimeSpan final = new TimeSpan(0, 0, 0);

            bool isTime;

            for (int i = 0; i < 2; i++)
            {
                var match = Regex.Match(times[i], "[0-9]+:[0-9]+:[0-9]+([,\\.][0-9]+)?");
                if (!match.Success)
                    return false;
                switch (i)
                {
                    case 0:
                        isTime = TimeSpan.TryParse(times[i].Replace(',', '.'), out initial);
                        if (!isTime)
                            return false;
                        break;
                    case 1:
                        isTime = TimeSpan.TryParse(times[i].Replace(',', '.'), out final);
                        if (!isTime)
                            return false;
                        break;
                }
            }

            if (final.TotalMilliseconds > initial.TotalMilliseconds)
                return false;

            return true;
        }

        private static bool SubStationAlphaVerification(DataTable styles, DataTable events)
        {
            try
            {
                string[] colStyles = "Name,Fontname,Fontsize,PrimaryColour,SecondaryColour,OutlineColour,BackColour,Bold,Italic,Underline,StrikeOut,ScaleX,ScaleY,Spacing,Angle,BorderStyle,Outline,Shadow,Alignment,MarginL,MarginR,MarginV,Encoding".Split(',');

                foreach (DataColumn column in styles.Columns)
                    if (!colStyles.Contains(column.ColumnName))
                        return false;

                foreach (DataRow row in styles.Rows)
                {
                    if (!Regex.IsMatch(row["PrimaryColour"].ToString(), "^&H([A-Fa-f0-9]{8})$"))
                        return false;
                    if (!Regex.IsMatch(row["SecondaryColour"].ToString(), "^&H([A-Fa-f0-9]{8})$"))
                        return false;
                    if (!Regex.IsMatch(row["OutlineColour"].ToString(), "^&H([A-Fa-f0-9]{8})$"))
                        return false;
                    if (!Regex.IsMatch(row["BackColour"].ToString(), "^&H([A-Fa-f0-9]{8})$"))
                        return false;
                }

                foreach (DataRow row in events.Rows)
                {
                    if (!Regex.IsMatch(row["Start"].ToString(), "[0-9]+:[0-9]+:[0-9]+([,\\.][0-9]+)?"))
                        return false;
                    if (!Regex.IsMatch(row["End"].ToString(), "[0-9]+:[0-9]+:[0-9]+([,\\.][0-9]+)?"))
                        return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
