using Janus.Lib.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            string newName = configuration.ReplacePattern;
            if (configuration.KeepSearch)
            {
                newName = newName.Replace("<current>", currentName);
            }
            else
            {
                string searchlessCurrent;
                if (configuration.IsRegex) searchlessCurrent = Regex.Replace(currentName, configuration.SearchPattern, "");
                else searchlessCurrent = currentName.Replace(configuration.SearchPattern, "");

                newName = newName.Replace("<current>", searchlessCurrent);
            }

            if (configuration.IsRegex)
            {
                newName = PlaceholdersReplacer(newName, currentName, configuration);
            }

            item.NewName = newName + extension;
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