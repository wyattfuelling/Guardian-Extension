// CyberFuelling Guardian - browser extension.
//  1. Reports the active tab's address to the Guardian desktop app (it only listens on 127.0.0.1, this computer).
//     Private / incognito tabs are never reported.
//  2. Blocks the websites on the blocklist you manage in the Guardian app.

const ENDPOINT = "http://127.0.0.1:47819/activity";
const api = typeof browser !== "undefined" ? browser : chrome;

// ---------- Talking to the app ----------

async function post(payload) {
  return fetch(ENDPOINT, {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body: JSON.stringify(payload)
  });
}

async function send(payload) {
  try {
    await post(payload);
  } catch (e) {
    // Guardian isn't running; nothing to do.
  }
}

// ---------- Active tab reporting ----------

async function reportActiveTab() {
  try {
    const win = await api.windows.getLastFocused();
    if (!win || !win.focused || win.incognito) {
      send({ type: "blur" });
      return;
    }
    const tabs = await api.tabs.query({ active: true, windowId: win.id });
    const tab = tabs[0];
    if (!tab || tab.incognito || !tab.url) {
      send({ type: "blur" });
      return;
    }
    send({ type: "tab", url: tab.url });
  } catch (e) {
    // ignore
  }
}

// ---------- Blocklist ----------

let blockedSites = [];
let lastSync = 0;

// The list is kept in storage so blocking still works after the browser restarts, even if Guardian isn't running.
const ready = api.storage.local.get("blockedSites").then((r) => {
  blockedSites = r.blockedSites || [];
});

async function syncBlocklist(force) {
  if (!force && Date.now() - lastSync < 3000) return;
  lastSync = Date.now();
  try {
    const res = await post({ type: "sync" });
    if (!res.ok) return;
    const data = await res.json();
    if (Array.isArray(data.sites)) {
      blockedSites = data.sites;
      await api.storage.local.set({ blockedSites });
      blockOpenTabs();
    }
  } catch (e) {
    // Guardian isn't running: keep using the last list we got.
  }
}

// Returns the blocked site that matches this address (including subdomains), or null.
function matchBlocked(url) {
  try {
    const u = new URL(url);
    if (u.protocol !== "http:" && u.protocol !== "https:") return null;
    let host = u.hostname.toLowerCase();
    if (host.startsWith("www.")) host = host.slice(4);
    for (const site of blockedSites) {
      if (host === site || host.endsWith("." + site)) return site;
    }
  } catch (e) {
    // not a normal URL
  }
  return null;
}

function blockedPageUrl(site) {
  return api.runtime.getURL("blocked.html") + "?site=" + encodeURIComponent(site);
}

async function blockOpenTabs() {
  try {
    const tabs = await api.tabs.query({});
    for (const tab of tabs) {
      const site = tab.url && matchBlocked(tab.url);
      if (site) api.tabs.update(tab.id, { url: blockedPageUrl(site) });
    }
  } catch (e) {
    // ignore
  }
}

// Stop navigation to a blocked site before it loads (main page only, not embedded frames).
api.webNavigation.onBeforeNavigate.addListener(async (details) => {
  if (details.frameId !== 0) return;
  await ready;
  const site = matchBlocked(details.url);
  if (site) api.tabs.update(details.tabId, { url: blockedPageUrl(site) });
});

// ---------- Wiring ----------

function refresh() {
  syncBlocklist(false);
  reportActiveTab();
}

api.tabs.onActivated.addListener(refresh);
api.tabs.onUpdated.addListener((tabId, changeInfo, tab) => {
  if (tab.active && (changeInfo.url || changeInfo.status === "complete")) refresh();
});
api.windows.onFocusChanged.addListener(reportActiveTab);
api.runtime.onStartup.addListener(() => syncBlocklist(true));
api.runtime.onInstalled.addListener(() => syncBlocklist(true));

// Heartbeat so Guardian knows the extension is alive and picks up blocklist changes.
api.alarms.create("guardian-heartbeat", { periodInMinutes: 0.5 });
api.alarms.onAlarm.addListener((alarm) => {
  if (alarm.name === "guardian-heartbeat") {
    syncBlocklist(true);
    reportActiveTab();
  }
});

syncBlocklist(true);
reportActiveTab();
