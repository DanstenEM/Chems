# Full Implementation Plan: Menu Stash -> Raid Inventory Loadout

## 1) Goal
Enable the player to choose raid items in the main menu stash UI and reliably spawn into the raid with exactly that chosen loadout.

## 2) Problem recap
Current systems persist:
- `stash` (long-term storage)
- `post_extract_inventory` (raid result shown in menu)

But there is no explicit persistence contract for **"what I am taking into the next raid"**, so scene transition loses menu selection intent.

## 3) Product behavior (target UX)
1. Player opens stash management in menu.
2. Player moves items from stash to the "to-raid" side (currently extracted/loadout candidate column).
3. Player presses **Play**.
4. Game saves that "to-raid" snapshot as **Raid Loadout**.
5. Raid scene loads and gameplay inventory is restored from that snapshot.
6. After raid end:
   - Extraction: resulting inventory is written to `post_extract_inventory` (existing behavior), then menu flow handles merge/move to stash.
   - Death/failure: no phantom reappearance of old loadout.

## 4) Architecture changes

### 4.1 Persistence layer (`InventoryPersistenceService`)
Add a dedicated key and API:
- `DefaultRaidLoadoutKey = "raid_loadout"`
- `SaveRaidLoadout(SavedInventory inventory)`
- `LoadRaidLoadout()`
- `ClearRaidLoadout()`

Design notes:
- Keep format identical to existing saves (`SavedInventoryWrapper`) for compatibility and simplicity.
- Keep `stash` and `post_extract_inventory` untouched.
- Maintain fail-safe behavior: returns empty snapshot on missing/corrupted file.

### 4.2 Menu layer (`MainMenuController`)
In `Play()` flow:
1. `PersistStashView()` (already present).
2. Build snapshot from the loadout-side slots (`extractedSlots` in current UI naming).
3. Save via `SaveRaidLoadout(...)`.
4. Load raid scene.

Decision point:
- If save fails, **log warning and continue** for now (non-blocking) to avoid hard lockouts.
- Optional future hardening: configurable policy (`blockOnLoadoutSaveFailure`).

### 4.3 Raid bootstrap layer (new component)
Create `RaidInventoryBootstrap` in gameplay scene.

Responsibilities:
- Run once on raid scene load.
- Resolve `InventorySystem.GameplayInventory` safely.
- Build item lookup using `InventorySnapshotMapper.BuildLookupWithFallbacks(...)`.
- Load `LoadRaidLoadout()` and restore via `InventorySnapshotMapper.RestoreSnapshot(..., clearBeforeRestore: true)`.
- Emit clear logs (restored stack count, skipped items, overflow).

Execution-order strategy:
- Run in `Start()` and retry for a short window if inventory not yet registered.
- Or integrate with Zenject initialization order if project bootstrap already uses it consistently.

### 4.4 End-of-raid lifecycle contract
Define strict ownership of each save file:
- `stash`: long-term storage, menu-owned.
- `post_extract_inventory`: raid result for menu review/transfer, extraction/death systems own writes.
- `raid_loadout`: pre-raid intent, menu writes, raid reads.

Recommended transitions:
- On Play: overwrite `raid_loadout` with current selected loadout.
- On successful raid bootstrap: optionally keep file for diagnostics OR clear immediately after successful restore (recommended: keep until raid end for crash recovery).
- On extraction back to menu: refresh post-extract, then clear `raid_loadout`.
- On death/game over return: clear `raid_loadout` to prevent stale reuse.

## 5) Data rules and edge-case policy

### 5.1 Snapshot validity rules
When restoring loadout:
- Ignore entries with empty `itemId` or `count <= 0`.
- Ignore unknown item IDs not present in lookup.
- Preserve existing per-category semantics:
  - weapons as distinct units
  - stackables aggregated by stack logic in `InventorySystem.AddItem`

### 5.2 Capacity overflow behavior
If snapshot exceeds slot capacity:
- Restore until full.
- Log warning with dropped count.
- Do not crash or partially corrupt inventory UI.

### 5.3 Backward compatibility
- Missing `raid_loadout` file => empty inventory snapshot.
- Corrupted JSON => catch, log, fallback to empty.
- Existing stash/extraction saves remain readable and unchanged.

## 6) Implementation breakdown (task list)

### Milestone A: Persistence API
- [ ] Add raid loadout key and 3 helper methods in `InventoryPersistenceService`.
- [ ] Add small inline docs/comments describing ownership and intent of each key.

### Milestone B: Save-on-Play integration
- [ ] In `MainMenuController.Play()`, save selected raid loadout snapshot before scene load.
- [ ] Add debug logs for successful/failed save.

### Milestone C: Raid restore bootstrap
- [ ] Implement `RaidInventoryBootstrap` MonoBehaviour.
- [ ] Add serialized fallback item list if needed for lookup parity with menu.
- [ ] Hook component into gameplay scene prefab/root.

### Milestone D: Lifecycle cleanup hooks
- [ ] Clear `raid_loadout` on raid completion pathways (extraction/death return path).
- [ ] Verify no stale loadout persists unintentionally.

### Milestone E: Observability
- [ ] Standardize logs with prefixes (`[InventoryLoadout]`).
- [ ] Report counts: loaded stacks, restored stacks, skipped unknown IDs, overflow drops.

## 7) Pseudocode-level flow

### 7.1 Menu Play
```text
Play():
  PersistStashView()
  loadout = BuildSnapshotFromSlots(extractedSlots)
  ok = InventoryPersistenceService.SaveRaidLoadout(loadout)
  if !ok: log warning
  LoadScene(playSceneName)
```

### 7.2 Raid bootstrap
```text
Start():
  inv = ResolveGameplayInventoryWithRetry()
  if inv == null: log error; return

  lookup = BuildLookupFromResourcesAndFallbacks()
  snapshot = InventoryPersistenceService.LoadRaidLoadout()
  restored = InventorySnapshotMapper.RestoreSnapshot(inv, snapshot, lookup, clearBeforeRestore: true)
  log restored summary
```

### 7.3 Raid end
```text
OnRaidFinished(result):
  if result == Extracted:
    save post_extract_inventory (existing)
  ClearRaidLoadout()
  LoadScene(mainMenu)
```

## 8) Testing strategy

### 8.1 Manual QA matrix
1. **Happy path**
   - Put known items into loadout column in menu.
   - Press Play.
   - Verify identical items in raid inventory.
2. **Empty loadout**
   - Start with no selected items.
   - Verify empty raid inventory (except intentional starter defaults if enabled).
3. **Overflow**
   - Save more items than raid capacity.
   - Verify truncation + warning logs.
4. **Unknown item IDs**
   - Inject one invalid item ID into save file.
   - Verify skip + warning; valid items still load.
5. **Corrupted file**
   - Corrupt `raid_loadout` JSON.
   - Verify fallback to empty without crash.
6. **Death path**
   - Die and return to menu.
   - Verify stale loadout not auto-reapplied next raid unless reselected.
7. **Extraction path**
   - Extract and return menu.
   - Verify post-extraction flow still works and stash merge remains unaffected.

### 8.2 Regression checklist
- Stash clear button still only affects stash.
- Existing `post_extract_inventory` save/load unchanged.
- Inventory dragging/quick transfer behavior unaffected.

## 9) Risks and mitigations
- **Init race** between bootstrap and inventory slot readiness.
  - Mitigation: retry/late-init and explicit error logs.
- **Scene wiring forgotten** (bootstrap not attached).
  - Mitigation: assertion log in scene load and QA checklist item.
- **Naming ambiguity** (`extractedSlots` actually used as loadout source).
  - Mitigation: optional refactor names later (`loadoutSlots`) to reduce confusion.

## 10) Rollout sequence
1. Implement code behind feature path.
2. Validate locally with QA matrix.
3. Merge with verbose logs enabled.
4. After stabilization, reduce log verbosity if needed.

## 11) Acceptance criteria (final)
- Pressing Play after selecting items in menu results in those items present in raid inventory on spawn.
- No data-loss during transition besides defined and logged invalid/overflow items.
- No regressions in stash management and post-extraction handling.
- Failure modes are graceful (no crashes, no blocked scene load unless intentionally configured).
