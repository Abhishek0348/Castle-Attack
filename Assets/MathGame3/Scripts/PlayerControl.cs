using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerControl : MonoBehaviour
{
    public Transform Parent;
    public GameObject EnemyTroops;
    private float moveSpeed = 12.0f;
    private float throwSpeed = 10.0f;
    public Castle castle;
    int totalCost = 0;
    private Dictionary<GameObject, Animator> troopAnimators = new Dictionary<GameObject, Animator>();
    private Queue<Vector3> availablePositions = new Queue<Vector3>();
    private Queue<string> availableNames = new Queue<string>();
    private int nextTroopIndex = 1;

    [SerializeField] public TextMeshProUGUI troopcostUI;
    private int cost;

    private List<Transform> troopsIdleList = new List<Transform>();

    private void Start()
    {
        troopcostUI.enabled = false;
    }

    public void InstantiateTroop(GameObject troopPrefab)
    {
        Vector3 spawnPosition = availablePositions.Count > 0 ? availablePositions.Dequeue() : new Vector3(Parent.position.x + Parent.childCount, Parent.position.y, Parent.position.z);
        string troopName = availableNames.Count > 0 ? availableNames.Dequeue() : "Troop" + nextTroopIndex++;

        Debug.Log("Instantiating troop at position: " + spawnPosition);
        GameObject newTroop = Instantiate(troopPrefab, spawnPosition, Parent.rotation, Parent);
        newTroop.name = troopName;

        TroopStats troopStats = newTroop.GetComponent<TroopStats>();
        if (troopStats != null)
        {
            totalCost += troopStats.troopCost;
        }

        TroopDestruction troopDestruction = newTroop.AddComponent<TroopDestruction>();
        troopDestruction.playerControl = this;

        Animator troopAnimator = newTroop.GetComponent<Animator>();
        if (troopAnimator != null)
        {
            troopAnimators.Add(newTroop, troopAnimator);
        }

        if (Parent.childCount > 0)
        {
            troopcostUI.gameObject.SetActive(true);
        }

        SortTroops();
        UpdateTroopCostUI();
    }

    private void SortTroops()
    {
        List<Transform> troops = new List<Transform>();

        foreach (Transform troop in Parent)
        {
            troops.Add(troop);
        }

        troops.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

        for (int i = 0; i < troops.Count; i++)
        {
            troops[i].SetSiblingIndex(i);
        }
    }

    public void StartWar()
    {
        castle.ArmyBuildUI.enabled = false;
        castle.ArmyBuildUI.GetComponentInChildren<TextMeshProUGUI>().enabled = false;

        Debug.Log("Starting war");
        EnemyAttack(EnemyTroops);
        MoveTroopsToCastle(castle.GetComponent<Collider2D>());
    }

    private void EnemyAttack(GameObject enemyTroopsParent)
    {
        foreach (Transform enemyTroop in enemyTroopsParent.transform)
        {
            Animator animator = enemyTroop.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("OnAttack");
                StartCoroutine(EnemyThrowablesCoroutine(enemyTroop));
            }
            else
            {
                Debug.LogWarning($"No Animator component found on {enemyTroop.name}");
            }
        }
    }

    private List<GameObject> enemyThrowables = new List<GameObject>();

    IEnumerator EnemyThrowablesCoroutine(Transform enemyTroop)
    {
        TroopStats troopStats = enemyTroop.GetComponent<TroopStats>();
        Animator enemyAnimator = enemyTroop.GetComponent<Animator>();
        if (troopStats != null && enemyAnimator != null)
        {
            GameObject throwablePrefab = troopStats.Throwables;
            Transform shotPoint = troopStats.shotPoint.transform;

            while (true)
            {
                yield return new WaitForSeconds(1f);
                GameObject throwable = Instantiate(throwablePrefab, shotPoint.position, Quaternion.identity);
                enemyThrowables.Add(throwable);
                StartCoroutine(MoveEnemyThrowable(throwable));
            }
        }
    }

    public void DestroyAllEnemyThrowables()
    {
        foreach (GameObject throwable in enemyThrowables)
        {
            if (throwable != null)
            {
                Destroy(throwable);
            }
        }
        enemyThrowables.Clear();
    }

    IEnumerator MoveEnemyThrowable(GameObject throwable)
    {
        throwable.GetComponent<Rigidbody2D>().velocity = 10 * throwable.transform.right;
        yield return null;
    }

    public void MoveTroopsToCastle(Collider2D castleCollider)
    {
        Transform[] childTroops = new Transform[Parent.childCount];
        int index = 0;
        foreach (Transform child in Parent)
        {
            child.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            childTroops[index++] = child;
        }
        StartCoroutine(MoveTroops(childTroops, castleCollider));
    }

    IEnumerator MoveTroops(Transform[] troops, Collider2D castleCollider)
    {
        List<IEnumerator> coroutines = new List<IEnumerator>();

        foreach (Transform troop in troops)
        {
            TroopStats troopStats = troop.GetComponent<TroopStats>();
            if (troopStats != null)
            {
                coroutines.Add(MoveChildrenCoroutine(troop, castleCollider.transform.position, troopStats.troopCost, troopStats.troopType));
            }
        }

        foreach (var coroutine in coroutines)
        {
            StartCoroutine(coroutine);
        }

        yield return StartCoroutine(CheckTroopsIdle());
    }

    IEnumerator CheckTroopsIdle()
    {
        yield return new WaitUntil(() => troopsIdleList.Count == Parent.childCount);
        yield return new WaitForSeconds(0.5f);
        castle.Results();
    }

    IEnumerator MoveChildrenCoroutine(Transform child, Vector3 castlePosition, int troopCost, string troopType)
    {
        float xOffset = UnityEngine.Random.Range(-1f, 1f);
        TroopStats troopStats = child.GetComponent<TroopStats>();
        Animator childAnimator = troopAnimators[child.gameObject];
        Vector3 targetPosition = castlePosition;

        switch (troopType)
        {
            case "Knight":
                targetPosition.x += 3 + 0.5f;
                break;
            case "Wizard":
                targetPosition.x += 7 + xOffset;
                break;
            case "Archer":
                targetPosition.x += 10 + xOffset;
                break;
            default:
                Debug.LogWarning("Unknown troop type: " + troopStats.troopType);
                yield break;
        }

        childAnimator.SetTrigger("RunToCastle");
        while (Vector3.Distance(child.position, targetPosition) > 0.1f)
        {
            child.position = Vector3.MoveTowards(child.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        childAnimator.SetTrigger("ReachedCastle");

        switch (troopType)
        {
            case "Knight":
                childAnimator.SetTrigger("AttackCastle");
                yield return new WaitForSeconds(1);
                castle.TakeDamage(troopCost);
                break;
            case "Wizard":
                childAnimator.SetTrigger("WizardAttack");
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(Shoot(troopCost, castlePosition, troopStats));
                Debug.Log("Wizard Damage : " + troopCost);
                break;
            case "Archer":
                childAnimator.SetTrigger("ArcherAttack");
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(Shoot(troopCost / 2, castlePosition, troopStats));
                yield return new WaitForSeconds(1f);
                yield return StartCoroutine(Shoot(troopCost / 2, castlePosition, troopStats));
                Debug.Log("Archer Damage : " + troopCost);
                break;
        }

        totalCost -= troopCost;
        UpdateTroopCostUI();
        childAnimator.SetTrigger("TroopIdle");

        troopsIdleList.Add(child);
    }

    IEnumerator Shoot(int cost, Vector3 castlePosition, TroopStats troopStats)
    {
        GameObject throwable = Instantiate(troopStats.Throwables, troopStats.shotPoint.transform.position, Quaternion.identity);
        Vector3 targetPosition = new Vector3(castlePosition.x + 3, castlePosition.y, castlePosition.z);

        while (Vector3.Distance(throwable.transform.position, targetPosition) > 0.1f)
        {
            throwable.transform.position = Vector3.MoveTowards(throwable.transform.position, targetPosition, throwSpeed * Time.deltaTime);
            yield return null;
        }

        castle.TakeDamage(cost);
        Debug.Log("Taken Damage : " + cost);
        Destroy(throwable);
    }

    public void RecordDestroyedTroopPosition(Vector3 position, string name)
    {
        availablePositions.Enqueue(position);
        availableNames.Enqueue(name);
    }

    public void DecreaseTotalCost(int amount)
    {
        totalCost -= amount;
        UpdateTroopCostUI();
    }

    private void UpdateTroopCostUI()
    {
        troopcostUI.enabled = true;

        if (troopcostUI != null)
        {
            castle.TroopCostBar.SetHealth(totalCost);
            troopcostUI.text = totalCost.ToString();
            castle.TroopCostBar.GetComponentInChildren<UnityEngine.UI.Image>().color = totalCost <= castle.TroopCostBar.slider.maxValue ? Color.green : Color.red;

            if (totalCost == castle.TroopCostBar.slider.maxValue)
            {
                TroopSelect[] troopSelects = FindObjectsOfType<TroopSelect>();
                foreach (TroopSelect troopSelect in troopSelects)
                {
                    Debug.Log(troopSelect.name);
                    troopSelect.PlayerTroopSlots.SetActive(false);
                    troopSelect.GetComponent<UnityEngine.UI.Image>().gameObject.SetActive(false);
                }
                Invoke("StartWar", 1.0f);
            }
        }
    }
}
