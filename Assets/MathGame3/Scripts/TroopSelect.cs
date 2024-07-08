using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TroopSelect : MonoBehaviour
{
    public PlayerControl playerControl;
    public GameObject troopPrefab;
    public int MaxUses;
    private int RemainingUses;
    public GameObject PlayerTroopSlots;
    public TroopStats troopStats;

    private void Start()
    {
        playerControl.GetComponent<Image>().enabled = false;
        PlayerTroopSlots.SetActive(false);
        GetComponent<Image>().gameObject.SetActive(false);
        RemainingUses = MaxUses;
        UpdateUI();
    }

    public void IncreaseRemainingUses()
    {
        RemainingUses++;
        UpdateUI();
        if (RemainingUses > 0)
        {
            GetComponent<Image>().gameObject.SetActive(true);
        }
        else if (RemainingUses == 0)
        {
            RemainingUses = 1;
            UpdateUI();
            GetComponent<Image>().gameObject.SetActive(true);
        }
    }

    public void SpawnTroop()
    {
        if (RemainingUses > 0)
        {
            playerControl.InstantiateTroop(troopPrefab);
            RemainingUses--;
            UpdateUI();
        }
    }

    public void ReactivateTroopSlot()
    {
        Debug.Log("Reactivating troop slot for " + troopStats.troopType);
        RemainingUses = 1;
        PlayerTroopSlots.SetActive(true);
        GetComponent<Image>().gameObject.SetActive(true);
        UpdateUI();
    }

    private void UpdateUI()
    {
        GetComponent<Image>().transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = "Power : " + troopStats.troopCost;
        GetComponent<Image>().GetComponentInChildren<TextMeshProUGUI>().text = RemainingUses.ToString();
    }
}
