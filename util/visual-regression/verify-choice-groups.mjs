// Run against an agent-owned DocsSamples instance: node util/visual-regression/verify-choice-groups.mjs http://localhost:5206
import assert from "node:assert/strict";
import { mkdtemp, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { launchBrowser } from "./browser.mjs";

const base = process.argv[2] ?? "http://localhost:5206";
const profile = await mkdtemp(join(tmpdir(), "stellar-choice-groups-"));
const browser = await launchBrowser(process.env.CHROME_PATH ?? "chromium", {
  hideScrollbars: false,
  extraArgs: [`--user-data-dir=${profile}`],
});
try {
  await browser.send("Page.enable");
  for (const page of ["RadioGroup", "CheckboxGroup"]) {
    for (const theme of ["observatory", "shadcn.vega"]) {
      for (const mode of ["light", "dark"]) {
        for (const width of [1280, 390]) {
          await browser.send("Emulation.setDeviceMetricsOverride", {
            width,
            height: 1100,
            deviceScaleFactor: 1,
            mobile: false,
          });
          const loaded = browser.waitForEvent("Page.loadEventFired", 15000);
          await browser.send("Page.navigate", {
            url: `${base}/${page}?theme=${theme}&mode=${mode}`,
          });
          assert.ok(await loaded, `${page} loaded`);
          await browser.evaluate(`(async () => {
            await document.fonts.ready;
            await new Promise(requestAnimationFrame);
            await Promise.all(document.getAnimations().filter(animation => animation.effect?.getComputedTiming().iterations !== Infinity).map(animation => animation.finished.catch(() => {})));
          })()`);
          const result = await browser.evaluate(`(() => {
        const inputs = [...document.querySelectorAll('input[type=checkbox], input[type=radio]')];
        return {
          count: inputs.length,
          selected: inputs.filter(input => input.checked).length,
          unique: new Set(inputs.map(input => input.id)).size === inputs.length,
          labels: inputs.every(input => document.querySelector('label[for="' + input.id + '"]')),
          overflow: document.documentElement.scrollWidth > innerWidth,
          errors: [...document.querySelectorAll('fieldset[aria-invalid=true]')].map(group => ({
            message: group.querySelector('.field-validation-error')?.textContent.trim(),
            invalidInputs: group.querySelectorAll('input[aria-invalid=true]').length,
          })),
          clientValidation: document.querySelectorAll("fieldset input[required], [data-choice-min], [data-choice-max]").length,
          cardWidth: document.querySelector('[data-slot$="-group"] > label')?.getBoundingClientRect().width,
          card: document.querySelectorAll('[data-slot$="-group"] > label > [data-slot=field]').length,
        };
      })()`);
          assert.ok(result.count >= 8, `${page}: options rendered`);
          assert.ok(result.selected > 0, `${page}: initial selections`);
          assert.equal(result.unique, true, `${page}: unique IDs`);
          assert.equal(result.clientValidation, 0, `${page}: no library client validation`);
          assert.equal(result.labels, true, `${page}: labels target inputs`);
          assert.equal(result.overflow, false, `${page}: fits ${width}`);
          assert.equal(result.card, 2, `${page}: card composition`);
          assert.deepEqual(
            result.errors,
            Array.from({ length: 1 }, () => ({
              message:
                page === "RadioGroup" ? "Choose a delivery method." : "Choose at least one extra.",
              invalidInputs: 2,
            })),
            `${page}: ModelState error appears in the default variant`,
          );
          assert.ok(result.cardWidth >= 200, `${page}: card has usable width`);
          if (process.env.CHOICE_SCREENSHOT_DIR) {
            const shot = await browser.send("Page.captureScreenshot", {
              format: "png",
              captureBeyondViewport: true,
            });
            await writeFile(
              `${process.env.CHOICE_SCREENSHOT_DIR}/${page}-${theme}-${mode}-${width}.png`,
              Buffer.from(shot.data, "base64"),
            );
          }
        }
      }
    }
  }

  const behavior = await browser.evaluate(`(async () => {
    const form = document.querySelector('form[method=post]');
    const group = form.querySelector('fieldset');
    const inputs = [...group.querySelectorAll('input[type=checkbox]')];
    for (const input of inputs) input.checked = false;
    form.reset();
    const resetCount = inputs.filter(input => input.checked).length;
    group.disabled = true;
    const disabledMarkerAbsent = ![...new FormData(form).keys()].some(key => key.startsWith('__sa_checkbox_group.'));
    group.disabled = false;
    for (const input of inputs) input.checked = false;
    const response = await fetch(location.href, { method: 'POST', body: new FormData(form) });
    const html = new DOMParser().parseFromString(await response.text(), 'text/html');
    return { resetCount, disabledMarkerAbsent,
      postStatus: response.status,
      emptyAfterPost: html.querySelectorAll('form input[type=checkbox]:checked').length,
      saved: html.querySelector('output')?.textContent.trim(),
    };
  })()`);
  assert.deepEqual(behavior, {
    resetCount: 2,
    disabledMarkerAbsent: true,
    postStatus: 200,
    emptyAfterPost: 0,
    saved: "Saved:",
  });

  const loaded = browser.waitForEvent("Page.loadEventFired", 15000);
  await browser.send("Page.navigate", { url: `${base}/RadioGroup` });
  await loaded;
  const radioPost = await browser.evaluate(`(async () => {
    const form = document.querySelector('form[method=post]');
    const data = new FormData(form);
    data.set('Order.DeliveryMethod', 'Express');
    const response = await fetch(location.href, { method: 'POST', body: data });
    const html = new DOMParser().parseFromString(await response.text(), 'text/html');
    data.set('Order.DeliveryMethod', 'invalid');
    const invalidResponse = await fetch(location.href, { method: 'POST', body: data });
    const invalid = new DOMParser().parseFromString(await invalidResponse.text(), 'text/html');
    return { value: html.querySelector('form input:checked')?.value,
      saved: html.querySelector('output')?.textContent.trim(),
      invalidStatus: invalidResponse.status,
      invalidSelections: invalid.querySelectorAll('form input:checked').length,
      error: !!invalid.querySelector('form .field-validation-error')?.textContent.trim(),
    };
  })()`);
  assert.deepEqual(radioPost, {
    value: "Express",
    saved: "Saved: Express",
    invalidStatus: 200,
    invalidSelections: 0,
    error: true,
  });

  const parity = await browser.evaluate(`(async () => {
    const load = async path => new DOMParser().parseFromString(await (await fetch(path)).text(), 'text/html');
    const signature = element => ({tag: element.tagName, slot: element.dataset.slot ?? '', classes: element.getAttribute('class') ?? '', children: [...element.children].map(signature)});
    const result = {};
    for (const type of ['Radio', 'Checkbox']) {
      const oldPage = await load('/' + type);
      const newPage = await load('/' + type + 'Group');
      const card = '[data-slot$="-group"] > label > [data-slot=field]';
      result[type] = JSON.stringify(signature(oldPage.querySelector(card))) === JSON.stringify(signature(newPage.querySelector(card)));
    }
    return result;
  })()`);
  assert.deepEqual(
    parity,
    { Radio: true, Checkbox: true },
    "Choice cards retain the existing Field/Input structure and classes",
  );
  console.log(
    "PASS: group layout, IDs, labels, reset, disabled state, POST binding, empty collection, invalid redisplay, and existing card HTML parity",
  );
} finally {
  browser.ws.close();
  browser.chrome.kill();
  await new Promise((resolve) => browser.chrome.once("exit", resolve));
  await rm(profile, { recursive: true, force: true, maxRetries: 10, retryDelay: 100 });
}
