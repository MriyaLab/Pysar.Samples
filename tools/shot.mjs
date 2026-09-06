// Minimal CDP screenshot helper: opens a URL in headless Chrome (started separately with
// --remote-debugging-port), waits a fixed delay so a WebAssembly app finishes its first render,
// then writes a full-page PNG. Chrome's own --screenshot flag fires at load time, and its
// --virtual-time-budget breaks the Blazor runtime's timers, so neither is usable here.
import { writeFileSync } from 'node:fs';

const [, , url, out, delayMs = '20000', width = '1440', height = '950', script] = process.argv;

const targets = await (await fetch('http://127.0.0.1:9222/json/list')).json();
const page = targets.find(t => t.type === 'page');
const ws = new WebSocket(page.webSocketDebuggerUrl);

let id = 0;
const pending = new Map();

const send = (method, params = {}) =>
    new Promise(resolve => {
        const messageId = ++id;
        pending.set(messageId, resolve);
        ws.send(JSON.stringify({ id: messageId, method, params }));
    });

ws.addEventListener('message', event => {
    const message = JSON.parse(event.data);
    pending.get(message.id)?.(message.result);
});

await new Promise(resolve => ws.addEventListener('open', resolve));

await send('Emulation.setDeviceMetricsOverride', {
    width: Number(width), height: Number(height), deviceScaleFactor: 2, mobile: false,
});
await send('Page.navigate', { url });
await new Promise(resolve => setTimeout(resolve, Number(delayMs)));

// Optional interaction - picking another report from the toolbar, for instance - followed by a
// second wait so the newly selected report has time to render before the shot is taken.
if (script) {
    await send('Runtime.evaluate', { expression: script, awaitPromise: true });
    await new Promise(resolve => setTimeout(resolve, Number(delayMs)));
}

const { data } = await send('Page.captureScreenshot', { format: 'png' });
writeFileSync(out, Buffer.from(data, 'base64'));

console.log(`saved ${out}`);
ws.close();
process.exit(0);
