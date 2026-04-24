# GhostThrough Rename Test Plan

## PR Comment Template

```md
Manual QA completed for the GhostThrough rename.

Checked:
- successful build via `compile.bat`
- `KeyboardHookRegressionTest` passes with `PASS`
- tray icon and menu entries
- keyboard and mouse activation flows
- activation behavior modes (`Hold` and `Click`)
- Ghost Mode activation/deactivation behavior
- opacity profile switching with `Ctrl+Shift+Up/Down`
- settings path rename to `%APPDATA%\GhostThrough\settings.json`
- migration from `%APPDATA%\PeekThrough\settings.json`
- new log file `ghostthrough_debug.log`
- backward-compatible fallback for `PEEKTHROUGH_LOG_LEVEL`

Result:
- no issues found in rename-specific runtime behavior
```

## QA Checklist

### Build and regression
- [ ] `compile.bat` builds `GhostThrough.exe` successfully
- [ ] `KeyboardHookRegressionTest` completes with `PASS`

### Startup and single-instance behavior
- [ ] `GhostThrough.exe` starts successfully
- [ ] tray icon appears with the expected `GhostThrough Ghost Mode` text
- [ ] launching a second instance does not create another running app instance

### Tray menu
- [ ] tray menu shows `Activation Key`
- [ ] tray menu shows `Activation Method`
- [ ] tray menu shows `Activation Mode`
- [ ] tray menu shows `Activation Hold Time`
- [ ] tray menu shows `Exit`
- [ ] `Activation Method` submenu shows all expected keyboard and mouse options
- [ ] `Activation Key` submenu shows the supported activation keys
- [ ] `Activation Mode` submenu shows `Hold` and `Click`
- [ ] `Activation Hold Time` submenu shows the supported delay values

### Keyboard activation - Hold mode
- [ ] selecting `Activation Mode` -> `Hold` is persisted
- [ ] holding the activation key for the configured delay activates Ghost Mode
- [ ] releasing the key after activation keeps Ghost Mode active
- [ ] short-pressing the activation key again deactivates Ghost Mode
- [ ] pressing `Esc` during Ghost Mode deactivates immediately

### Keyboard activation - Click mode
- [ ] selecting `Activation Mode` -> `Click` is persisted
- [ ] holding the activation key for the configured delay activates Ghost Mode
- [ ] releasing the key after activation deactivates Ghost Mode

### Mouse activation
- [ ] holding the selected mouse button for the configured delay activates Ghost Mode
- [ ] releasing the selected mouse button deactivates Ghost Mode
- [ ] pressing another mouse button before the selected one blocks activation
- [ ] pressing another mouse button while holding the selected one blocks that activation attempt

### Ghost Mode behavior
- [ ] the target window becomes semi-transparent
- [ ] mouse input passes through to windows behind it
- [ ] tooltip appears near the cursor on activation
- [ ] activation and deactivation play the expected short beep
- [ ] `Ctrl+Shift+Up` changes to the next opacity profile
- [ ] `Ctrl+Shift+Down` changes to the previous opacity profile
- [ ] profile switching while active reapplies transparency immediately

### Settings and migration
- [ ] settings are saved to `%APPDATA%\GhostThrough\settings.json`
- [ ] existing legacy settings in `%APPDATA%\PeekThrough\settings.json` are migrated on first run
- [ ] legacy settings file remains intact after migration
- [ ] migrated settings preserve the expected active profile and activation configuration

### Logging
- [ ] logs are written to `ghostthrough_debug.log`
- [ ] `GHOSTTHROUGH_LOG_LEVEL=INFO` reduces debug logging
- [ ] legacy `PEEKTHROUGH_LOG_LEVEL=INFO` still works as a fallback

### Shutdown
- [ ] exiting via tray menu closes the app cleanly
- [ ] any ghosted window is restored to its original state on exit

## Detailed Test Plan

### Goal

Verify that renaming the project runtime identity from `PeekThrough` to `GhostThrough` does not break runtime behavior, settings, logging, or user-facing workflows.

### Scope

- build
- app startup
- single-instance behavior
- tray UI
- Ghost Mode
- keyboard activation
- mouse activation
- activation behavior modes
- profile switching
- settings migration
- logging
- shutdown and cleanup

### Preconditions

- the latest `GhostThrough.exe` build is available
- a normal Windows desktop session is available
- at least one standard application window is available for transparency testing
- for the migration scenario:
  - `%APPDATA%\PeekThrough\settings.json` exists
  - `%APPDATA%\GhostThrough\settings.json` does not exist before first launch

### Test Scenarios

#### 1. Build

1. Run `compile.bat`
2. Confirm the build finishes successfully
3. Confirm `GhostThrough.exe` is created

Expected result:
- build succeeds
- output file is named `GhostThrough.exe`

#### 2. First launch

1. Start `GhostThrough.exe`
2. Check that the tray icon appears
3. Hover the tray icon

Expected result:
- the application starts without errors
- the tray icon is visible
- the tray tooltip shows `GhostThrough Ghost Mode`

#### 3. Single-instance behavior

1. Launch `GhostThrough.exe` while it is already running

Expected result:
- no second running instance is created
- the existing instance continues working normally

#### 4. Tray menu

1. Open the tray menu
2. Verify the main menu entries
3. Open `Activation Method`
4. Open `Activation Key`
5. Open `Activation Mode`
6. Open `Activation Hold Time`

Expected result:
- `Activation Key` is present
- `Activation Method` is present
- `Exit` is present
- `Activation Method` lists all expected options
- `Activation Key` lists the expected supported keys
- `Activation Mode` lists `Hold` and `Click`
- `Activation Hold Time` lists supported delay values

#### 5. Keyboard activation - Hold mode

1. Select keyboard activation
2. Select `Activation Mode` -> `Hold`
3. Move the cursor over a normal application window
4. Hold the activation key for the configured delay
5. Release the key
6. Short-press the activation key again

Expected result:
- Ghost Mode activates after the hold delay
- the target window becomes semi-transparent and click-through
- releasing the key keeps Ghost Mode active
- a short follow-up press deactivates Ghost Mode

#### 6. Keyboard activation - Click mode

1. Select keyboard activation
2. Select `Activation Mode` -> `Click`
3. Move the cursor over a normal application window
4. Hold the activation key for the configured delay
5. Release the key

Expected result:
- Ghost Mode activates after the hold delay
- the target window becomes semi-transparent and click-through
- releasing the key deactivates Ghost Mode

#### 7. Escape deactivation

1. Activate Ghost Mode in keyboard mode
2. Press `Esc`

Expected result:
- Ghost Mode deactivates immediately
- the target window returns to its original state

#### 8. Mouse activation

1. Select one of the mouse activation options
2. Move the cursor over a normal application window
3. Hold the selected mouse button for the configured delay
4. Release the button

Expected result:
- Ghost Mode activates after the hold delay
- releasing the selected mouse button deactivates Ghost Mode

#### 9. Mouse conflict blocking

1. In mouse mode, press another mouse button before the selected activation button
2. Try to activate Ghost Mode
3. Repeat with another mouse button pressed after the selected button is already held

Expected result:
- conflicting button usage blocks activation
- Ghost Mode does not activate incorrectly

#### 10. Profile switching

1. Activate Ghost Mode
2. Press `Ctrl+Shift+Up`
3. Press `Ctrl+Shift+Down`

Expected result:
- profile switches in both directions
- transparency updates immediately
- tooltip reflects the currently active profile

#### 11. Tooltip and sound

1. Activate Ghost Mode
2. Deactivate Ghost Mode

Expected result:
- a tooltip appears near the cursor
- a short beep plays on activation
- a short beep plays on deactivation

#### 12. Settings path rename

1. After launch, check for `%APPDATA%\GhostThrough\settings.json`

Expected result:
- settings are written to the new `GhostThrough` app data folder

#### 13. Legacy settings migration

1. Prepare only `%APPDATA%\PeekThrough\settings.json`
2. Ensure `%APPDATA%\GhostThrough\settings.json` does not exist
3. Launch `GhostThrough.exe`

Expected result:
- `%APPDATA%\GhostThrough\settings.json` is created
- settings are copied correctly
- `%APPDATA%\PeekThrough\settings.json` remains intact

#### 14. Logging

1. Launch the app
2. Perform a few activation and deactivation actions
3. Check `ghostthrough_debug.log`
4. Set `GHOSTTHROUGH_LOG_LEVEL=INFO`
5. Launch again and confirm reduced debug logging
6. Repeat with `PEEKTHROUGH_LOG_LEVEL=INFO`

Expected result:
- logs are written to `ghostthrough_debug.log`
- `GHOSTTHROUGH_LOG_LEVEL=INFO` reduces verbose debug logging
- legacy `PEEKTHROUGH_LOG_LEVEL=INFO` still works as a fallback

#### 15. Shutdown

1. Activate Ghost Mode
2. Exit through the tray menu

Expected result:
- the app exits cleanly
- any ghosted window is restored
- no transparency artifacts remain

### Success Criteria

- all scenarios pass without errors
- the rename does not break existing user workflows
- settings migration works as expected
- backward compatibility for the legacy logging environment variable is preserved
