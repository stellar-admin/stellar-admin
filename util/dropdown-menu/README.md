# Dropdown menu verification

Run ComponentPlayground on an available port, then run the browser checks using Node and system Chromium:

```bash
dotnet run --project sandbox/ComponentPlayground --no-launch-profile --urls http://localhost:5207
node util/dropdown-menu/check.mjs http://localhost:5207
```

The script drives the `Demo/DropdownMenuItems` fixture with trusted mouse and keyboard input through the DevTools Protocol, since synthetic events skip a native button's activation. It checks that plain (button) and link items look the same in each playground theme, that a click, Enter or Space runs a plain item once and closes the menu, `close-on-click="false"`, disabled items, type-ahead, a submit item posting its form once by mouse and keyboard, a disabled submit item not posting, link navigation, submenus and Escape. It starts and stops its own Chromium process; the playground remains running.

Set `CHROME_PATH` to override the Chromium executable.
