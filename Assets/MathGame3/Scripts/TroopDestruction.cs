using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopDestruction : MonoBehaviour
{
    public PlayerControl playerControl; // Reference to PlayerControl
    private Dictionary<string, TroopSelect> troopSelectCache = new Dictionary<string, TroopSelect>();

    private void Start()
    {
        CacheTroopSelects();
    }

    private void OnMouseDown()
    {
        OnTroopClicked(gameObject);
    }

    private void CacheTroopSelects()
    {
        TroopSelect[] troopSelects = FindObjectsOfType<TroopSelect>();
        foreach (TroopSelect troopSelect in troopSelects)
        {
            TroopStats stats = troopSelect.troopPrefab.GetComponent<TroopStats>();
            if (stats != null)
            {
                troopSelectCache[stats.troopType] = troopSelect;
            }
        }
    }

    public void OnTroopClicked(GameObject troop)
    {
        TroopStats troopStats = troop.GetComponent<TroopStats>();
        if (troopStats == null)
        {
            Debug.LogError("TroopStats component not found on clicked troop: " + troop.name);
            return;
        }

        if (troopSelectCache.TryGetValue(troopStats.troopType, out TroopSelect troopSelect))
        {
            troopSelect.IncreaseRemainingUses();
        }
        else
        {
            Debug.LogError("TroopSelect component not found for troop prefab: " + troopStats.troopType);
        }

        playerControl.RecordDestroyedTroopPosition(troop.transform.position, troop.name);
        playerControl.DecreaseTotalCost(troopStats.troopCost);
        Destroy(troop);
    }
}
