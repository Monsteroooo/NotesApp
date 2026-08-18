const jsdom = require("jsdom");
const { JSDOM } = jsdom;
JSDOM.fromURL("https://uicdn.toast.com/editor/latest/toastui-editor-all.min.js", { runScripts: "dangerously" }).then(dom => {
  console.log("Globals:", Object.keys(dom.window).filter(k => k.includes("toast")));
}).catch(console.error);
