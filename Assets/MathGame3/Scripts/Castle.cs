using System.Collections;
using System.Collections.Generic;
using TMPro ;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Castle : MonoBehaviour
{
    public int EnemyCount;
    public Sprite VictoryCastle;
    public Sprite DefeatCastle;
    [HideInInspector] public int EnemyLeft;
    public HealthBar CastlehealthBar;
    public HealthBar TroopCostBar;
    [SerializeField] private GameObject GameOverScreen;
    [SerializeField] private GameObject VictoryScreen;
    [SerializeField] private GameObject Platform;
    [SerializeField] public Image ArmyBuildUI;
    PlayerControl playerControl;
    public AudioSource WinSound;

    public GameObject PopUp;
    public Animator animator;
    public TextMeshProUGUI PopUpText;

    [SerializeField] private TextMeshProUGUI CastleHealthText;
    

    void Start()
    {
        playerControl = FindObjectOfType<PlayerControl>();
        ArmyBuildUI.enabled = false;
        ArmyBuildUI.GetComponentInChildren<TextMeshProUGUI>().enabled = false;
        ShowPopUp("Enemies with power " + EnemyCount + " has captured your castle. You need to create your army with power " + EnemyCount + " to beat them!!!  Are You Ready?");
        Platform.SetActive(false);
        CastlehealthBar.SetMaxHealth(EnemyCount);
        EnemyLeft = EnemyCount;
        GameOverScreen.SetActive(false);
        VictoryScreen.SetActive(false);
        CastleHealthText.enabled = false;
        CastleHealthText.text = EnemyCount.ToString();
        TroopCostBar.slider.maxValue = EnemyCount;
        TroopCostBar.gameObject.SetActive(false);
    }


    public void TakeDamage(int damage)
    {
        EnemyLeft -= damage;
        EnemyLeft = Mathf.Max(EnemyLeft, 0); // Ensure EnemyLeft doesn't go negative
        CastlehealthBar.SetHealth(EnemyLeft);
        CastleHealthText.text = EnemyLeft.ToString();
    }


    public void Results()
    {
        if (EnemyLeft > 0)
        {
            // Enemies remaining
            Debug.Log("Enemies remaining: " + EnemyLeft);
            Debug.Log("You Lost! ");
            PlayerControl.FindObjectOfType<PlayerControl>().GetComponent<PlayerControl>().EnemyTroops.SetActive(false);
            StartCoroutine(DelayCoroutine(1f, () => {
                for (int i = 0; i < playerControl.Parent.transform.childCount; i++)
                {
                    Destroy(playerControl.Parent.transform.GetChild(i).gameObject);
                }
                GameOverScreen.SetActive(true);
                HealthBar.Destroy(CastlehealthBar.gameObject);
                HealthBar.Destroy(TroopCostBar.gameObject);
            }));
            PlayerControl.Destroy(FindObjectOfType<PlayerControl>().gameObject);

            Platform.SetActive(false);
        }
        else if (EnemyLeft == 0)
        {
            // No enemies left, proceed to the next level
            Debug.Log("Congratulations! You've defeated all enemies!");
            PlayerControl.FindObjectOfType<PlayerControl>().GetComponent<PlayerControl>().EnemyTroops.SetActive(false);
            playerControl.DestroyAllEnemyThrowables();
            this.gameObject.GetComponent<SpriteRenderer>().sprite = VictoryCastle; 
            PlayerControl.Destroy(FindObjectOfType<PlayerControl>().gameObject);
            AudioSource.PlayClipAtPoint(WinSound.clip, transform.position);
            StartCoroutine(DelayCoroutine(2.5f, () =>
            {
                for(int i = 0; i < playerControl.Parent.transform.childCount; i++)
                {
                    Destroy(playerControl.Parent.transform.GetChild(i).gameObject);
                }
                HealthBar.Destroy(CastlehealthBar.gameObject);
                HealthBar.Destroy(TroopCostBar.gameObject);
                playerControl.troopcostUI.enabled = false;
                CastleHealthText.enabled = false;
                VictoryScreen.SetActive(true);
            }));
            
        }
        else
        {
            Debug.Log("Game Over! Your castle has been destroyed!");
            this.gameObject.GetComponent<SpriteRenderer>().sprite = DefeatCastle;
            StartCoroutine(DelayCoroutine(1f, () => {
                for (int i = 0; i < playerControl.Parent.transform.childCount; i++)
                {
                    Destroy(playerControl.Parent.transform.GetChild(i).gameObject);
                }
                GameOverScreen.SetActive(true);
                HealthBar.Destroy(CastlehealthBar.gameObject);
                HealthBar.Destroy(TroopCostBar.gameObject);
            }));
            PlayerControl.Destroy(FindObjectOfType<PlayerControl>().gameObject);
            Platform.SetActive(false);
        }
    }

    IEnumerator DelayCoroutine(float delay, System.Action action)
    {
        yield return new WaitForSecondsRealtime(delay); 
        action(); 
    }


    public void ProceedToNextLevel()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        int nextLevelIndex = currentLevelIndex + 1;
        if (nextLevelIndex <= SceneManager.sceneCountInBuildSettings && nextLevelIndex<4)
        {
            SceneManager.LoadScene("Level_"+nextLevelIndex);
        }
        else
        {
            MainMenuLoad();
        }

    }
    public void restartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenuLoad()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowPopUp(string message)
    {
        PopUp.SetActive(true);
        PopUpText.text = message;
        animator.SetTrigger("pop");    
    }
    public void DeactivatePopup()
    {
        TroopSelect[] troopSelects = FindObjectsOfType<TroopSelect>(true);
        foreach (TroopSelect troopSelect in troopSelects)
        {
            troopSelect.playerControl.GetComponent<Image>().enabled = true;
            troopSelect.PlayerTroopSlots.SetActive(true);
            troopSelect.GetComponent<Image>().gameObject.SetActive(true);
        }
        PopUp.SetActive(false);
        Platform.SetActive(true);
        CastleHealthText.enabled = true;
        TroopCostBar.gameObject.SetActive(true);  
        ArmyBuildUI.enabled = true;
        ArmyBuildUI.GetComponentInChildren<TextMeshProUGUI>().enabled = true;
    }
}
