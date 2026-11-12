using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace CyberFuelling_Guardian
{
    public class WebData
    {
        // date ("yyyy-MM-dd") -> website -> seconds
        public Dictionary<string, Dictionary<string, double>> Days { get; set; } = new Dictionary<string, Dictionary<string, double>>();
        public DateTime? LastMessage { get; set; }
    }

    /// <summary>
    /// Tracks which websites you spend time on. A small browser extension tells Guardian which site is in
    /// the active tab (sent only to 127.0.0.1, i.e. this computer). While a browser is the foreground app and
    /// you're not idle, ScreenTimeTracker's seconds are credited to that site. Only the website name
    /// (e.g. "github.com") is stored: no full URLs, page titles or private/incognito tabs.
    /// Saved to %AppData%\CyberFuelling Guardian\websites.json.
    /// </summary>
    public class WebActivityTracker : IDisposable
    {
        public const int Port = 47819;
        private const int FreshSeconds = 90;        // ignore the extension's report if it's older than this
        private const int LogAfterSeconds = 15;     // log "Website visited" once a site has had this much time today
        private const int SaveEveryTicks = 30;
        private const int KeepDays = 90;
        private const string DayFormat = "yyyy-MM-dd";

        private static readonly HashSet<string> BrowserProcesses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "chrome", "msedge", "firefox", "brave", "opera", "opera_gx", "vivaldi",
            "chromium", "waterfox", "librewolf", "arc", "iexplore"
        };

        private readonly ActivityLog log;
        private readonly WebBlocklist blocklist;
        private readonly Dictionary<string, DateTime> lastBlockedLog = new Dictionary<string, DateTime>();
        private readonly System.Threading.SynchronizationContext ui;
        private readonly string filePath;
        private readonly HashSet<string> loggedVisits = new HashSet<string>();

        private WebData data = new WebData();
        private HttpListener listener;
        private string currentSite;     // site in the active tab right now (null = none / not trackable)
        private string currentSource;   // which browser's extension told us (its Origin)
        private DateTime lastReport = DateTime.MinValue;
        private int ticksSinceSave;
        private bool dirty;
        private bool disposed;

        /// <summary>Raised (on the UI thread) when new website time or a new extension message arrives.</summary>
        public event EventHandler Updated;

        /// <summary>True if Guardian is listening for the browser extension.</summary>
        public bool Listening { get; private set; }

        /// <summary>True once the extension has ever contacted Guardian.</summary>
        public bool HasEverConnected { get { return data.LastMessage.HasValue; } }

        public WebActivityTracker(ActivityLog log, WebBlocklist blocklist)
        {
            this.log = log;
            this.blocklist = blocklist;
            ui = System.Threading.SynchronizationContext.Current;

            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CyberFuelling Guardian");
            filePath = Path.Combine(folder, "websites.json");
            Load();
        }

        public static bool IsBrowser(string processName)
        {
            return processName != null && BrowserProcesses.Contains(processName);
        }

        // ---------- Public read access ----------

        /// <summary>Number of different websites visited today for at least minSeconds.</summary>
        public int TodaySiteCount(int minSeconds = 15)
        {
            Dictionary<string, double> sites;
            if (!data.Days.TryGetValue(Today(), out sites)) return 0;
            return sites.Count(kv => kv.Value >= minSeconds);
        }

        /// <summary>The websites visited on a given day, longest first.</summary>
        public List<KeyValuePair<string, TimeSpan>> Breakdown(DateTime day)
        {
            Dictionary<string, double> sites;
            if (!data.Days.TryGetValue(day.ToString(DayFormat, CultureInfo.InvariantCulture), out sites))
                return new List<KeyValuePair<string, TimeSpan>>();

            return sites
                .OrderByDescending(kv => kv.Value)
                .Select(kv => new KeyValuePair<string, TimeSpan>(kv.Key, TimeSpan.FromSeconds(kv.Value)))
                .ToList();
        }

        // ---------- Listening for the extension ----------

        public void Start()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://127.0.0.1:" + Port + "/");
                listener.Start();
                Listening = true;
                listener.BeginGetContext(OnContext, null);
            }
            catch
            {
                Listening = false;
                log.Add("Website tracking unavailable", "Port " + Port + " is in use or blocked");
            }
        }

        private void OnContext(IAsyncResult result)
        {
            HttpListenerContext context;
            try { context = listener.EndGetContext(result); }
            catch { return; }   // listener was closed

            try { listener.BeginGetContext(OnContext, null); } catch { }

            try { Handle(context); }
            catch { try { context.Response.Abort(); } catch { } }
        }

        // Runs on a thread-pool thread. Does the minimum, then hands the result to the UI thread.
        private void Handle(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // Web pages can't forge an extension origin, so this keeps random sites from posting fake data.
            string origin = request.Headers["Origin"];
            bool fromExtension = origin != null &&
                (origin.StartsWith("chrome-extension://", StringComparison.OrdinalIgnoreCase) ||
                 origin.StartsWith("moz-extension://", StringComparison.OrdinalIgnoreCase));
            if (!fromExtension)
            {
                response.StatusCode = 403;
                response.Close();
                return;
            }

            response.Headers["Access-Control-Allow-Origin"] = origin;
            response.Headers["Vary"] = "Origin";
            response.Headers["Access-Control-Allow-Methods"] = "POST, OPTIONS";
            response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
            response.Headers["Access-Control-Allow-Private-Network"] = "true";

            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = 204;
                response.Close();
                return;
            }

            if (request.HttpMethod != "POST" || request.ContentLength64 > 4096)
            {
                response.StatusCode = 400;
                response.Close();
                return;
            }

            string body;
            using (StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8))
                body = reader.ReadToEnd();

            string type = null, url = null, reportedSite = null;
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(body))
                {
                    JsonElement value;
                    if (doc.RootElement.TryGetProperty("type", out value)) type = value.GetString();
                    if (doc.RootElement.TryGetProperty("url", out value)) url = value.GetString();
                    if (doc.RootElement.TryGetProperty("site", out value)) reportedSite = value.GetString();
                }
            }
            catch
            {
                response.StatusCode = 400;
                response.Close();
                return;
            }

            if (type == "sync")
            {
                // The extension asks for the current blocklist.
                byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { sites = blocklist.Snapshot() }));
                response.StatusCode = 200;
                response.ContentType = "application/json";
                response.ContentLength64 = bytes.Length;
                response.OutputStream.Write(bytes, 0, bytes.Length);
                response.Close();
                RunOnUi(MarkConnected);
                return;
            }

            response.StatusCode = 200;
            response.Close();

            if (type == "blocked")
            {
                // The extension stopped a visit to a blocked site.
                string blocked = WebBlocklist.Normalize(reportedSite);
                if (blocked != null) RunOnUi(() => LogBlocked(blocked));
                return;
            }

            string site = type == "tab" ? SiteFromUrl(url) : null;
            RunOnUi(() => ApplyReport(origin, site));
        }

        private void RunOnUi(Action action)
        {
            if (ui != null) ui.Post(_ => action(), null);
            else action();
        }

        // Runs on the UI thread.
        private void MarkConnected()
        {
            if (disposed) return;
            data.LastMessage = DateTime.Now;
            dirty = true;
            if (Updated != null) Updated(this, EventArgs.Empty);
        }

        // Runs on the UI thread. Reloading the block page shouldn't spam the log, so repeats within 10s are skipped.
        private void LogBlocked(string site)
        {
            if (disposed) return;
            DateTime last;
            if (lastBlockedLog.TryGetValue(site, out last) && (DateTime.Now - last).TotalSeconds < 10) return;
            lastBlockedLog[site] = DateTime.Now;
            log.Add("Website blocked", site);
        }

        // Runs on the UI thread.
        private void ApplyReport(string source, string site)
        {
            if (disposed) return;

            data.LastMessage = DateTime.Now;
            dirty = true;

            if (site != null)
            {
                currentSite = site;
                currentSource = source;
                lastReport = DateTime.Now;
            }
            else if (source == currentSource)
            {
                currentSite = null;   // the browser we were following lost focus or went to a non-web page
            }
            // A "no site" report from a different browser is ignored: it's just that browser being in the background.

            if (Updated != null) Updated(this, EventArgs.Empty);
        }

        /// <summary>Hook this to ScreenTimeTracker.TimeCounted.</summary>
        public void OnTimeCounted(string processName, double seconds)
        {
            if (currentSite == null || !IsBrowser(processName)) return;
            if ((DateTime.Now - lastReport).TotalSeconds > FreshSeconds) return;

            string day = Today();
            Dictionary<string, double> sites;
            if (!data.Days.TryGetValue(day, out sites))
            {
                sites = new Dictionary<string, double>();
                data.Days[day] = sites;
            }

            double current;
            sites.TryGetValue(currentSite, out current);
            double updated = current + seconds;
            sites[currentSite] = updated;
            dirty = true;

            if (updated >= LogAfterSeconds && loggedVisits.Add(day + "|" + currentSite))
                log.Add("Website visited", currentSite);

            if (++ticksSinceSave >= SaveEveryTicks) Save();
            if (Updated != null) Updated(this, EventArgs.Empty);
        }

        /// <summary>Turns "https://www.github.com/foo?bar" into "github.com". Returns null for non-web pages.</summary>
        private static string SiteFromUrl(string url)
        {
            Uri uri;
            if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out uri)) return null;
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) return null;

            string host = uri.Host.ToLowerInvariant();
            if (host.StartsWith("www.")) host = host.Substring(4);
            return host.Length == 0 ? null : host;
        }

        private static string Today()
        {
            return DateTime.Now.ToString(DayFormat, CultureInfo.InvariantCulture);
        }

        // ---------- Saving / loading ----------

        private void Load()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    WebData loaded = JsonSerializer.Deserialize<WebData>(File.ReadAllText(filePath));
                    if (loaded != null && loaded.Days != null) data = loaded;
                }
            }
            catch
            {
                data = new WebData();   // corrupt file: start fresh
            }

            string cutoff = DateTime.Now.AddDays(-KeepDays).ToString(DayFormat, CultureInfo.InvariantCulture);
            foreach (string day in data.Days.Keys.Where(k => string.CompareOrdinal(k, cutoff) < 0).ToList())
                data.Days.Remove(day);
        }

        public void Save()
        {
            ticksSinceSave = 0;
            if (!dirty) return;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                string temp = filePath + ".tmp";
                File.WriteAllText(temp, JsonSerializer.Serialize(data));
                File.Move(temp, filePath, true);
                dirty = false;
            }
            catch { /* best-effort; try again next time */ }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            try { if (listener != null) listener.Close(); } catch { }
            Save();
        }
    }
}
