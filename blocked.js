// Shown instead of a blocked website. Tells the Guardian app about the blocked attempt (so it appears in the activity log).
const site = new URLSearchParams(location.search).get("site") || "this website";
document.getElementById("site").textContent = site;

fetch("http://127.0.0.1:47819/activity", {
  method: "POST",
  headers: { "Content-Type": "text/plain" },
  body: JSON.stringify({ type: "blocked", site: site })
}).catch(() => { /* Guardian isn't running */ });

document.getElementById("back").addEventListener("click", () => {
  if (history.length > 1) history.back();
  else window.close();
});
