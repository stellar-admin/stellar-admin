// Run against DocsSamples: node util/segmented-control/check.mjs http://localhost:5206
import assert from "node:assert/strict";
import { mkdirSync, writeFileSync } from "node:fs";
import { launchBrowser, sleep } from "../visual-regression/browser.mjs";

const url = process.argv[2] ?? "http://localhost:5206";
const output = process.argv[3] ?? "/tmp/stellar-admin-segmented-control";
const browser = await launchBrowser(process.env.CHROME_PATH ?? "chromium");
mkdirSync(output, { recursive: true });
try {
  await browser.send("Page.enable");
  for (const theme of ["ledger", "luma", "lyra", "maia", "mira", "nova", "rhea", "sera", "vega"]) {
    for (const mode of ["light", "dark"]) {
      for (const width of [1280, 390]) {
        await browser.send("Emulation.setDeviceMetricsOverride", {
          width,
          height: 1100,
          deviceScaleFactor: 1,
          mobile: false,
        });
        await browser.send("Page.navigate", {
          url: `${url}/SegmentedControl?theme=${theme}&mode=${mode}`,
        });
        await sleep(250);
        await browser.evaluate("document.fonts.ready");
        const comparison = await browser.evaluate(`(() => {
          const group = document.querySelector('.sa-segmented-control');
          const tabs = document.querySelector('.sa-tabs-list');
          const styles = el => {
            const s = getComputedStyle(el);
            return Object.fromEntries(['height', 'width', 'padding', 'gap', 'borderRadius', 'backgroundColor', 'color', 'fontSize', 'fontWeight', 'boxShadow'].map(key => [key, s[key]]));
          };
          return { group: styles(group), tabs: styles(tabs), selected: styles(group.querySelector('label')), active: styles(tabs.querySelector('a')), unselected: styles(group.querySelectorAll('label')[1]), inactive: styles(tabs.querySelectorAll('a')[1]) };
        })()`);
        assert.deepEqual(
          comparison.group,
          comparison.tabs,
          `${theme}/${mode}/${width} group matches tabs`,
        );
        assert.deepEqual(
          comparison.selected,
          comparison.active,
          `${theme}/${mode}/${width} selected matches active tab`,
        );
        assert.deepEqual(
          comparison.unselected,
          comparison.inactive,
          `${theme}/${mode}/${width} unselected matches inactive tab`,
        );
        if (["ledger", "nova", "vega"].includes(theme)) {
          const shot = await browser.send("Page.captureScreenshot", {
            format: "png",
            captureBeyondViewport: true,
          });
          writeFileSync(
            `${output}/${theme}-${mode}-${width}.png`,
            Buffer.from(shot.data, "base64"),
          );
        }
      }
    }
  }
  console.log("PASS: tab appearance matches across 9 themes, light/dark, desktop/mobile");

  const interaction = await browser.evaluate(`(() => {
    const group = document.querySelector('input[name="trip-view"]').closest('[role=radiogroup]');
    window.segmentedChanges = [];
    group.addEventListener('change', event => window.segmentedChanges.push({ value: event.target.value, parent: event.currentTarget === group }));
    const [list, map] = group.querySelectorAll('input');
    map.closest('label').click();
    map.closest('label').click();
    const display = document.getElementById('trip-view-result').textContent;
    list.checked = true;
    const ids = [...document.querySelectorAll('.sa-segmented-control-input')].map(el => el.id);
    const labelsWork = [...document.querySelectorAll('.sa-segmented-control-item')].every(label => label.control === label.querySelector('input'));
    const disabled = document.querySelector('input[name="available-travel"][value="Cars"]');
    disabled.closest('label').click();
    return { changes: window.segmentedChanges, display, idsUnique: new Set(ids).size === ids.length, labelsWork, disabledSelected: disabled.checked, disabledCount: document.querySelectorAll('input[name="locked-travel"]:disabled').length, independent: document.querySelector('input[name="travel-mode"]:checked').value };
  })()`);
  assert.deepEqual(interaction, {
    changes: [{ value: "Map", parent: true }],
    display: "Map",
    idsUnique: true,
    labelsWork: true,
    disabledSelected: false,
    disabledCount: 3,
    independent: "Flights",
  });

  await browser.evaluate("document.querySelector('input[name=\"trip-view\"]:checked').focus()");
  await browser.send("Input.dispatchKeyEvent", {
    type: "keyDown",
    key: "ArrowRight",
    code: "ArrowRight",
    windowsVirtualKeyCode: 39,
  });
  await browser.send("Input.dispatchKeyEvent", {
    type: "keyUp",
    key: "ArrowRight",
    code: "ArrowRight",
    windowsVirtualKeyCode: 39,
  });
  assert.deepEqual(
    await browser.evaluate(
      `({ value: document.querySelector('input[name="trip-view"]:checked').value, count: window.segmentedChanges.length, focus: document.activeElement.matches('input[value="Map"]'), ring: getComputedStyle(document.activeElement.closest('label')).boxShadow !== 'none' })`,
    ),
    { value: "Map", count: 2, focus: true, ring: true },
  );
  await browser.send("Input.dispatchKeyEvent", {
    type: "keyDown",
    key: "Tab",
    code: "Tab",
    windowsVirtualKeyCode: 9,
  });
  await browser.send("Input.dispatchKeyEvent", {
    type: "keyUp",
    key: "Tab",
    code: "Tab",
    windowsVirtualKeyCode: 9,
  });
  assert.equal(await browser.evaluate('document.activeElement.name !== "trip-view"'), true);
  console.log(
    "PASS: native change count, programmatic assignment, keyboard focus/navigation, labels, disabled and independent groups",
  );

  const field = await browser.evaluate(`(() => {
    const radio = document.querySelector('input[name="Booking.Cabin"]');
    const wrapper = radio.closest('[data-slot=field]');
    const label = wrapper.querySelector('[data-slot=field-label]');
    const invalid = document.querySelector('input[name="Validation.Cabin"]').closest('[data-slot=field]');
    return {
      wrappers: wrapper.parentElement.closest('[data-slot=field]') === null,
      label: label.textContent.trim(),
      labelTargetsRadio: label.control === radio,
      description: wrapper.querySelector('[data-slot=field-description]').textContent.trim(),
      errors: invalid.querySelectorAll('[data-slot=field-error]').length,
      error: invalid.querySelector('[data-slot=field-error]').textContent.trim(),
    };
  })()`);
  assert.deepEqual(field, {
    wrappers: true,
    label: "Cabin class",
    labelTargetsRadio: true,
    description: "Choose a cabin for your next flight.",
    errors: 1,
    error: "Choose a cabin class.",
  });
  console.log(
    "PASS: automatic field wrapper, radio label, metadata description, and single validation message",
  );

  const forms = await browser.evaluate(`(async () => {
    const form = document.querySelector('form[method=post]');
    const economy = form.querySelector('input[value="Economy"]');
    const business = form.querySelector('input[value="Business"]');
    let changes = 0;
    form.addEventListener('change', () => changes++);
    business.click();
    const selected = new FormData(form).get('Booking.Cabin');
    form.reset();
    const reset = economy.checked && changes === 1;
    const send = async value => {
      const data = new FormData(form);
      data.set('Booking.Cabin', value);
      const response = await fetch(location.pathname, { method: 'POST', body: data });
      const doc = new DOMParser().parseFromString(await response.text(), 'text/html');
      return { status: response.status, text: doc.body.textContent, fieldError: doc.querySelector('[data-valmsg-for="Booking.Cabin"][data-slot=field-error]')?.textContent.trim(), selected: doc.querySelector('input[name="Booking.Cabin"]:checked')?.value ?? null, invalid: doc.querySelectorAll('input[name="Booking.Cabin"][aria-invalid=true]').length };
    };
    return { selected, reset, valid: await send('Business'), missing: await send(''), invalid: await send('9999') };
  })()`);
  assert.equal(forms.selected, "Business");
  assert.equal(forms.reset, true);
  assert.equal(forms.valid.status, 200);
  assert.equal(forms.valid.selected, "Business");
  assert.match(forms.valid.text, /Cabin saved: Business/);
  assert.match(forms.missing.text, /Choose a cabin class/);
  assert.equal(forms.missing.fieldError, "Choose a cabin class.");
  assert.equal(forms.missing.selected, null);
  assert.equal(forms.missing.invalid, 3);
  assert.equal(forms.invalid.selected, null);
  assert.equal(forms.invalid.invalid, 3);
  console.log(
    "PASS: prefixed nullable-enum form binding, submitted selection, required/invalid errors, form reset",
  );
} finally {
  browser.ws.close();
  browser.chrome.kill();
}
