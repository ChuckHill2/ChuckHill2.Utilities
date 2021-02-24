using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetCookie.Helper;

namespace GetCookie.Helper
{
    internal static class Mozilla
    {
        private class CC
        {
            public string name;
            public string value;
            public long lastAccessed;
            public CC(string n, string v, string la)
            {
                name = n;
                value = v;
                lastAccessed = long.TryParse(la, out long ll) ? ll : 0;
            }
            public override string ToString() => $"{name} => {value}";
        }

        // Get cookies from gecko browser
        public static string GetCookie(string domain)
        {
            // Get firefox default profile directory
            string profile = GetFFProfileFolder();
            // Read cookies from file
            if (profile == null) return string.Empty;
            string db_location = Path.Combine(profile, "cookies.sqlite");
            // Read data from table
            SQLite sSQLite = SQLite.ReadTable(db_location, "moz_cookies");
            if (sSQLite == null) return string.Empty;
            // Get values from table: SELECT name, value FROM moz_cookies WHERE host = 'www.{0}' OR host = '.{0}' OR host = '.www.{0}' ORDER BY creationTime
            var list = new List<CC>();
            for (int i = 0; i < sSQLite.GetRowCount(); i++)
            {
                var host = sSQLite.GetValue(i, 4);
                if (!host.Equals(domain, StringComparison.OrdinalIgnoreCase)) continue;
                var name = sSQLite.GetValue(i, 2);
                var value = sSQLite.GetValue(i, 3);
                var lastAccessed = sSQLite.GetValue(i, 6);
                list.Add(new CC(name, value, lastAccessed));
            }
            if (list.Count == 0) return string.Empty;
            var sb = new StringBuilder();
            string delimiter = string.Empty;
            foreach (var cc in list.OrderBy(m => m.lastAccessed))
            {
                sb.Append(delimiter);
                sb.Append(cc.name);
                sb.Append('=');
                sb.Append(cc.value);
                delimiter = "; ";
            }
            return sb.ToString();
        }

        private static string GetFFProfileFolder()
        {
            string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string bdir = Path.Combine(Appdata, "Mozilla\\Firefox\\Profiles");
            if (!Directory.Exists(bdir)) return null;
            return Directory.EnumerateDirectories(bdir).FirstOrDefault(m => File.Exists(m + "\\logins.json") || File.Exists(m + "\\key4.db") || File.Exists(m + "\\places.sqlite"));
        }
    }
}
