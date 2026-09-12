import { spawn } from "node:child_process";

export async function launchBrowser(
  browserBinary,
  { hideScrollbars = true, headless = true, extraArgs = [] } = {},
) {
  const port = 9222 + Math.floor(Math.random() * 500);
  const chrome = spawn(
    browserBinary,
    [
      ...(headless ? ["--headless=new"] : []),
      `--remote-debugging-port=${port}`,
      "--no-first-run",
      "--no-default-browser-check",
      "--disable-gpu",
      "--force-color-profile=srgb",
      ...(hideScrollbars ? ["--hide-scrollbars"] : []),
      "--font-render-hinting=none",
      "--window-size=1400,1000",
      ...extraArgs,
      "about:blank",
    ],
    { stdio: "ignore" },
  );

  let wsUrl;
  for (let i = 0; i < 100 && !wsUrl; i++) {
    await sleep(200);
    try {
      const targets = await (await fetch(`http://127.0.0.1:${port}/json`)).json();
      wsUrl = targets.find((t) => t.type === "page")?.webSocketDebuggerUrl;
    } catch {
      /* not up yet */
    }
  }
  if (!wsUrl) {
    chrome.kill();
    throw new Error(`${browserBinary} did not expose a debugging endpoint`);
  }

  const ws = new WebSocket(wsUrl);
  await new Promise((resolveOpen, reject) => {
    ws.onopen = resolveOpen;
    ws.onerror = reject;
  });

  let messageId = 0;
  const pending = new Map();
  const eventWaiters = [];
  ws.onmessage = (event) => {
    const message = JSON.parse(event.data);
    if (message.id && pending.has(message.id)) {
      pending.get(message.id)(message);
      pending.delete(message.id);
    } else if (message.method) {
      for (const waiter of [...eventWaiters]) {
        if (waiter.method === message.method) {
          eventWaiters.splice(eventWaiters.indexOf(waiter), 1);
          waiter.resolve(message.params);
        }
      }
    }
  };

  const send = (method, params = {}) =>
    new Promise((resolveSend, reject) => {
      const id = ++messageId;
      pending.set(id, (message) => {
        if (message.error) reject(new Error(`${method}: ${message.error.message}`));
        else resolveSend(message.result);
      });
      ws.send(JSON.stringify({ id, method, params }));
    });

  const waitForEvent = (method, timeoutMs) =>
    new Promise((resolveWait) => {
      const waiter = { method, resolve: resolveWait };
      eventWaiters.push(waiter);
      setTimeout(() => {
        const index = eventWaiters.indexOf(waiter);
        if (index >= 0) {
          eventWaiters.splice(index, 1);
          resolveWait(null);
        }
      }, timeoutMs);
    });

  const evaluate = async (expression) => {
    const result = await send("Runtime.evaluate", {
      expression,
      returnByValue: true,
      awaitPromise: true,
    });
    if (result.exceptionDetails) {
      throw new Error(`page evaluate failed: ${result.exceptionDetails.text}`);
    }
    return result.result?.value;
  };

  return { chrome, ws, send, waitForEvent, evaluate };
}

export const sleep = (ms) => new Promise((resolveSleep) => setTimeout(resolveSleep, ms));
