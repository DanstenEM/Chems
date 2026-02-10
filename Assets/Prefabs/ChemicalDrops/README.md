# Chemical Drop Prefabs

This folder stores assets used by loot generation for chemical drops.

- `CopperInventoryItem.prefab`: 2D UI inventory prefab for Copper with icon sprite assigned.
- `CopperInventoryItemObj.asset`: loot item definition used by loot crates so they only spawn items from this folder.
- `IronInventoryItemObj.asset`: chemical loot definition that maps to `IronPickup.prefab` for loot crate generation.

- `CopperPickup.prefab`: world pickup prefab that returns `CopperInventoryItemObj` when collected.
- `IronPickup.prefab`: world pickup prefab that returns `IronInventoryItemObj` when collected.
