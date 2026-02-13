using System.Collections;
using UnityEngine;

public class RaidInventoryLoadoutBootstrap : MonoBehaviour
{
    [SerializeField] private float resolveTimeoutSeconds = 3f;
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
            if (gameplayInventory != null)
            {
                break;
            }

            yield return new WaitForSeconds(resolveRetryDelaySeconds);
            elapsed += resolveRetryDelaySeconds;
        }

        if (gameplayInventory == null)
        {
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
}
