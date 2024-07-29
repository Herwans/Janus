using Janus.Lib.Model;
using System.Text.RegularExpressions;

namespace Janus.Lib.Helper
{
    public class RegexHelper
    {
        public static string Clean(string value)
        {
            value = value.Trim(['-', ' ']);
            value = Regex.Replace(value, "-{2,}", "-");
            return Regex.Replace(value, "\\s{2,}", " ").Trim();
        }

        public static bool IsValidRegex(string value)
        {
            try
            {
                Regex.IsMatch("", value);
                return true;
            }
            catch { return false; }
        }

        public static FileItem PatternsReplacer(FileItem item, Configuration configuration)
        {
            string extension = Path.GetExtension(item.CurrentName);
            string currentName = Path.GetFileNameWithoutExtension(item.CurrentName);
            string newName = string.IsNullOrEmpty(configuration.ReplacePattern) ? currentName : configuration.ReplacePattern;

            if (configuration.KeepSearch)
            {
                newName = newName.Replace("<current>", currentName);
            }
            else
            {
                string searchlessCurrent;
                if (configuration.SearchPattern != "")
                {
                    if (configuration.IsRegex)
                        searchlessCurrent = Regex.Replace(currentName, configuration.SearchPattern, "", configuration.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
                    else
                        searchlessCurrent = currentName.Replace(configuration.SearchPattern, "", !configuration.CaseSensitive, System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    searchlessCurrent = "";
                }

                if (configuration.ReplacePattern == "")
                {
                    newName = searchlessCurrent;
                }
                else
                {
                    newName = newName.Replace("<current>", searchlessCurrent);
                }
            }

            if (configuration.IsRegex)
            {
                newName = PlaceholdersReplacer(newName, currentName, configuration);
            }

            item.NewName = newName.Trim() + extension;
            return item;
        }

        public static string PlaceholdersReplacer(string input, string original, Configuration configuration)
        {
            if (!IsValidRegex(configuration.SearchPattern)) return input;
            Match match = Regex.Match(original, configuration.SearchPattern, configuration.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            var groups = match.Groups;
            for (int i = 1; i < groups.Count; i++)
            {
                input = Regex.Replace(input, $"<g{i}>", groups[i].Value, configuration.CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            }

            return input;
        }

        public class Configuration
        {
            public bool CaseSensitive { get; set; }
            public string ReplacePattern { get; set; }
            public string SearchPattern { get; set; }
            public bool KeepSearch { get; set; }
            public bool IsRegex { get; set; }
        }
    }
}