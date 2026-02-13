# Plan: Transfer Selected Stash Items Into Raid Inventory

## Problem statement
Players can move items inside the main menu stash UI, but when they press **Play** and enter the raid scene, the gameplay inventory is not restored from those menu selections.

## Current behavior (observed in code)
1. Main menu persists two snapshots (`post_extract_inventory` and `stash`) when leaving stash or pressing Play.
2. Raid gameplay inventory (`InventorySystem` marked as gameplay inventory) is initialized with starter/default logic, but no persistence restore step is executed on scene load.
3. Result: stash adjustments made in menu never become the runtime inventory in raid.

## Realization plan

### 1) Introduce explicit "Raid Loadout" persistence key and API
- Extend `InventoryPersistenceService` with a dedicated loadout key and helpers:
  - `SaveRaidLoadout(SavedInventory inventory)`
  - `LoadRaidLoadout()`
  - `ClearRaidLoadout()`
- Keep stash and post-extraction saves untouched to preserve existing flows.

### 2) Build a menu-side loadout snapshot at Play time
- In `MainMenuController.Play()`:
  - Persist current stash UI state first (already done).
  - Build a `SavedInventory` snapshot from the **extracted/in-raid candidate slots** (the inventory column representing what player takes into raid).
  - Save it using `SaveRaidLoadout` before loading the raid scene.
- Add guardrails/logging if saving fails, and decide fallback (block scene load vs continue with warning; recommended: continue with warning for now).

### 3) Restore raid loadout into gameplay inventory on raid scene boot
- Add a dedicated bootstrap component (e.g. `RaidInventoryBootstrap`) in gameplay scene scope.
- On start/initialize:
  - Resolve `InventorySystem.GameplayInventory`.
  - Build item lookup via `InventorySnapshotMapper.BuildLookupWithFallbacks(...)`.
  - Load snapshot from `LoadRaidLoadout()`.
  - Apply with `InventorySnapshotMapper.RestoreSnapshot(..., clearBeforeRestore: true)`.
- Ensure restore runs after inventory slots are ready (script execution order or delayed initialize if needed).

### 4) Define lifecycle rules after raid end/death/extraction
- Decide when to clear/overwrite raid loadout:
  - On successful extraction: overwrite from gameplay extraction result (or clear and rely on post-extraction flow).
  - On player death: clear raid loadout to avoid stale carryover.
  - On returning to menu: menu should still load stash + extracted as today.
- Document this as a deterministic state machine (Menu Stash -> Raid Loadout -> Raid Result).

### 5) Validation & test checklist
- Happy path:
  - Move items from stash to extracted/loadout in menu.
  - Press Play.
  - Verify same stacks appear in raid inventory slots.
- Empty loadout path:
  - Start raid with no selected items -> raid inventory should be empty (except intended defaults).
- Edge cases:
  - Unknown `itemId` in saved file should be skipped safely.
  - Loadout larger than slot capacity should partially restore with warning.
  - Missing/corrupted save file should not break scene load.
- Regression:
  - Existing stash management persistence remains unchanged.
  - Extraction saving (`post_extract_inventory`) still functions.

## Suggested implementation order
1. Persistence API extension (`InventoryPersistenceService`).
2. Save loadout in `MainMenuController.Play()`.
3. Add raid bootstrap restore component and wire into gameplay scene.
4. Add logs + lifecycle cleanup hooks.
5. Execute manual QA checklist above.

## Acceptance criteria
- Items selected in menu for raid are present in gameplay inventory immediately after entering raid.
- No selected items are lost during scene transition (except invalid/overflow entries, which are logged).
- Stash and post-extraction systems continue to behave as before.
