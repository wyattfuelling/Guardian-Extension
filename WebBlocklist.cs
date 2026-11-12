using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace CyberFuelling_Guardian
{
    public class BlocklistData
    {
        public List<string> Sites { get; set; } = new List<string>();
    }

    /// <summary>
    /// The list of blocked websites. Saved to %AppData%\CyberFuelling Guardian\blocklist.json and
    /// handed to the browser extension, which does the actual blocking.
    /// A blocked site also blocks its subdomains (blocking "example.com" blocks "news.example.com").
    /// </summary>
    public class WebBlocklist
    {
        private static readonly Regex ValidSite = new Regex(@"^([a-z0-9]([a-z0-9-]*[a-z0-9])?\.)+([a-z]{2,}|xn--[a-z0-9-]+)$", RegexOptions.Compiled);

        private readonly object sync = new object();
        private readonly string filePath;
        private List<string> sites = new List<string>();

        /// <summary>Raised whenever the list changes.</summary>
        public event EventHandler Changed;

        public WebBlocklist()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CyberFuelling Guardian");
            filePath = Path.Combine(folder, "blocklist.json");
            Load();
        }

        /// <summary>A sorted copy of the blocked sites (safe to call from any thread).</summary>
        public string[] Snapshot()
        {
            lock (sync) { return sites.OrderBy(s => s, StringComparer.Ordinal).ToArray(); }
        }

        public int Count
        {
            get { lock (sync) { return sites.Count; } }
        }

        public bool Add(string input, out string site, out string error)
        {
            site = Normalize(input);
            error = null;

            if (site == null)
            {
                error = "That doesn't look like a valid website address (try something like example.com).";
                return false;
            }

            lock (sync)
            {
                if (sites.Contains(site))
                {
                    error = site + " is already blocked.";
                    return false;
                }
                sites.Add(site);
                Save();
            }

            RaiseChanged();
            return true;
        }

        public bool Remove(string site)
        {
            bool removed;
            lock (sync)
            {
                removed = sites.Remove(site);
                if (removed) Save();
            }
            if (removed) RaiseChanged();
            return removed;
        }

        /// <summary>
        /// Turns whatever the user typed ("https://www.Example.com/page?x=1", "example.com:8080") into
        /// "example.com". Returns null if it isn't a plausible website name.
        /// </summary>
        public static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string s = input.Trim().ToLowerInvariant();

            int scheme = s.IndexOf("://", StringComparison.Ordinal);
            if (scheme >= 0) s = s.Substring(scheme + 3);

            int end = s.IndexOfAny(new[] { '/', '?', '#' });
            if (end >= 0) s = s.Substring(0, end);

            int at = s.LastIndexOf('@');
            if (at >= 0) s = s.Substring(at + 1);

            int colon = s.IndexOf(':');
            if (colon >= 0) s = s.Substring(0, colon);

            s = s.Trim('.');
            if (s.StartsWith("www.", StringComparison.Ordinal)) s = s.Substring(4);

            try { s = new IdnMapping().GetAscii(s); }   // international names -> xn-- form
            catch { return null; }

            return ValidSite.IsMatch(s) ? s : null;
        }

        private void RaiseChanged()
        {
            EventHandler handler = Changed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void Load()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    BlocklistData loaded = JsonSerializer.Deserialize<BlocklistData>(File.ReadAllText(filePath));
                    if (loaded != null && loaded.Sites != null)
                        sites = loaded.Sites.Select(Normalize).Where(s => s != null).Distinct().ToList();
                }
            }
            catch
            {
                sites = new List<string>();
            }
        }

        // Caller holds the lock.
        private void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                string temp = filePath + ".tmp";
                File.WriteAllText(temp, JsonSerializer.Serialize(new BlocklistData { Sites = sites }));
                File.Move(temp, filePath, true);
            }
            catch { /* best-effort */ }
        }
    }
}
