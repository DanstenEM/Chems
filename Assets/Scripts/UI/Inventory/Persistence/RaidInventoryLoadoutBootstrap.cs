using System.Collections;
using UnityEngine;

public class RaidInventoryLoadoutBootstrap : MonoBehaviour
{
    [SerializeField] private float resolveTimeoutSeconds = 5f;
    [SerializeField] private float resolveRetryDelaySeconds = 0.1f;
    [SerializeField] private InventoryItemObj[] lookupFallbackItems;

    private const string BootstrapObjectName = "RaidInventoryLoadoutBootstrap";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBootstrapExists()
    {
        if (FindObjectOfType<RaidInventoryLoadoutBootstrap>() != null)
        {
            return;
        }

        var bootstrapObject = new GameObject(BootstrapObjectName);
        bootstrapObject.AddComponent<RaidInventoryLoadoutBootstrap>();
    }

    private IEnumerator Start()
    {
        float elapsed = 0f;
        InventorySystem gameplayInventory = null;

        while (elapsed < resolveTimeoutSeconds)
        {
            gameplayInventory = InventorySystem.GameplayInventory;
            if (gameplayInventory != null && HasSlotsReady(gameplayInventory))
            {
                break;
            }

            yield return new WaitForSeconds(resolveRetryDelaySeconds);
            elapsed += resolveRetryDelaySeconds;
        }

        if (gameplayInventory == null)
        {
            Debug.LogWarning("[InventoryLoadout] Gameplay inventory not found within timeout. Skipping raid loadout restore.");
            Destroy(gameObject);
            yield break;
        }

        if (!HasSlotsReady(gameplayInventory))
        {
            Debug.LogWarning("[InventoryLoadout] Gameplay inventory slots are not ready within timeout. Skipping raid loadout restore.");
            Destroy(gameObject);
            yield break;
        }

        var lookup = InventorySnapshotMapper.BuildLookupWithFallbacks(
            InventorySnapshotMapper.BuildLookupFromResources(),
            lookupFallbackItems);

        var raidLoadout = InventoryPersistenceService.LoadRaidLoadout();
        int restoredItemCount = InventorySnapshotMapper.RestoreSnapshot(gameplayInventory, raidLoadout, lookup, true);

        Debug.Log($"[InventoryLoadout] Raid loadout restore complete. Restored item count: {restoredItemCount}.");
        Destroy(gameObject);
    }

    private static bool HasSlotsReady(InventorySystem inventorySystem)
    {
        var slots = inventorySystem != null ? inventorySystem.GetSlots() : null;
        return slots != null && slots.Length > 0;
    }
}
