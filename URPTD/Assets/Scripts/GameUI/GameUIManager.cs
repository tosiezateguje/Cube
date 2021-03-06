using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.Profiling;

public class GameUIManager : MonoBehaviour
{
    //v________________________________________________________________________________________________________________________________________________________
    [SerializeField] TMP_Text cash, score, crystal;
    [SerializeField] GameObject StartBtn, nextWaveBtn, stats, rightPanel, rightPanelContent, upgradePanel, upgradeBtn, endScreen, endStats, mainMenuBtn, best, upgradeBtn2, popUp;
    [SerializeField] List<TMP_Text> buyTurretValues = new List<TMP_Text>();
    [SerializeField] List<TMP_Text> upgradeBtnValues = new List<TMP_Text>();
    [SerializeField] List<TMP_Text> upgradeBtnCosts = new List<TMP_Text>();
    [SerializeField] TMP_Text possibleUpgrade, levelValue, expValue, upgradeValue, upgradeValue2, possibleUpgrade2;
    [SerializeField] TMP_Text stageWave;
    [Header("Upgrade button available")]
    [SerializeField] Color buttonAvailable;
    [Header("Upgrade button not available")]
    [SerializeField] Color buttonNotAvailable, buttonMax;
    [SerializeField] Image turretIcon;
    GameManager gameManager;
    InGameAds inGameAds;


    [SerializeField] TurretBehaviour UpgradeTarget;
    [SerializeField] SpawningManager spawningManager;


    [Header("Unlocks")]
    [SerializeField] GameObject crystalShopSlot;
    [SerializeField] Animator crystalIconAnimator;

    [Header("EndScreen")]
    [SerializeField] TMP_Text endScore, endGain;
    [SerializeField] bool gameEnded = false;

    //V________________________________________________________________________________________________________________________________________________________
    public Animator CrystalIconAnimator => crystalIconAnimator;
    [SerializeField] Animator cashIconAnimator;
    public Animator CashIconAnimator => cashIconAnimator;

    //F________________________________________________________________________________________________________________________________________________________


    void Start()
    {
        gameManager = GameManager.Instance;
        upgradePanel.SetActive(false);
        nextWaveBtn.SetActive(false);
        //  Time.timeScale = 0f;

        SetListValues();
        SetCrystalShop();

    }

    public void SetStageWave(float stage, float wave)
    {
        stageWave.text = $"Stage {stage} Wave {wave}";
    }

    void SetCrystalShop()
    {
        foreach (ShopUnlock shopUnlock in gameManager.gameStats.ListOfUnlocks)
        {
            if (!shopUnlock.CanBePurchased)
                continue;

            GameObject x = Instantiate(crystalShopSlot, transform.position, transform.rotation) as GameObject;
            x.transform.SetParent(rightPanelContent.transform);
            x.GetComponent<TurretUnlockBehaviour>().SetUnlockStats(shopUnlock);
        }

        rightPanelContent.transform.localScale = new Vector3(2f,2f,1f);
    }


    void SetListValues()
    {
        foreach (Transform x in stats.transform)
        {
            if (x.name == "Upgrade")
                continue;

            upgradeBtnValues.Add(x.gameObject.transform.Find("value").GetComponent<TMP_Text>());
            upgradeBtnCosts.Add(x.gameObject.transform.Find("cost").GetComponent<TMP_Text>());
        }
    }

    public async void EndGameHandle(int score)
    {
        endScreen.SetActive(true);
        upgradePanel.SetActive(false);
        rightPanel.SetActive(false);
        upgradePanel.SetActive(false);
        nextWaveBtn.SetActive(false);
        await Task.Delay(2000);
        ScoreTransition(endScore, score);
        EndScreenTransition(endScreen.GetComponent<RectTransform>());
        await Task.Delay(500);

        endStats.SetActive(true);
        int gain = CalculateGain(score);
        ScoreTransition(endGain, gain);
        gameManager.gameStats.ChangeBalanceCCoin(gain);
        SaveSys.SAVE(gameManager.gameStats); //save
        mainMenuBtn.SetActive(true);
        await Task.Delay(10000);
        if (Time.timeScale == 1f || Time.timeScale == 0f)
            return;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenu");
    }

    public void LoadMainMenu() => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("MainMenu");

    public int CalculateGain(int score)
    {
        int result = 0;
        result += UnityEngine.Random.Range(1, 7) + (score / 10);
        if (score < gameManager.gameStats.HighestScore)
            return result;

        result = (int)((float)result * 1.2f);
        return result;
    }

    public async void ScoreTransition(TMP_Text asset, int finalScore)
    {
        int diff = 1 + (finalScore / 99);
        int score = 0;
        while (score < finalScore)
        {
            score += diff;
            asset.text = score.ToString();
            await Task.Delay(20);
        }
        if (score > gameManager.gameStats.HighestScore)
        {
            best.SetActive(true);
            gameManager.gameStats.HighestScore = score;
        }
        asset.text = finalScore.ToString();
    }

    public async void EndScreenTransition(RectTransform x)
    {
        int i = 22;
        while (i > 0)
        {
            i--;
            x.transform.position += new Vector3(0, i, 0);
            await Task.Delay(35);
        }
    }


    public void SetUITarget(TurretBehaviour behaviour)
    {
        if (UpgradeTarget != null)
            UpgradeTarget.ChangeActive(false);
        UpgradeTarget = behaviour;
        upgradePanel.SetActive(true);
        UpgradeTarget.ChangeActive(true);
        turretIcon.sprite = UpgradeTarget.turret.icon;
        UpdateCashShop();

        SetUpgradeBtn();
        UpdateUpgradeBtn();
    }




    void Update()    //! HEAVY PERFORMANCE 
    {
        if (gameEnded)
            return;

        if (GameManager.Instance.SpawningManager.SpawningSequence)
            rightPanel.SetActive(false);
        else
            rightPanel.SetActive(true);

        cash.text = gameManager.FormatNumber(gameManager.CashAmount).ToString();
        score.text = gameManager.ScoreAmount.ToString();
        crystal.text = gameManager.CrystalAmount.ToString();

        if (!StartBtn.activeSelf)
        {
            if (spawningManager.SpawningSequence)
                nextWaveBtn.SetActive(false);
            else
                nextWaveBtn.SetActive(true);
        }

        if (UpgradeTarget != null && upgradePanel.activeSelf)
        {
            UpdateCashShop();
            UpdateUpgradeBtn();
        }

    }



    public void UpdateCashShop()
    {
        int[] costs = UpgradeTarget.turret.GetCosts;
        float[] values = UpgradeTarget.turret.GetValues;
        int[] counts = UpgradeTarget.turret.UpgradeCount;
        levelValue.text = UpgradeTarget.turret.Level.ToString();
        double exp = Math.Round((float)UpgradeTarget.turret.Exp / (float)UpgradeTarget.turret.RequiredExp * 100f, 1);
        if (exp == 100)
            exp -= 1f;
        expValue.text = exp.ToString() + "%";

        for (int i = 0; i < upgradeBtnValues.Count; i++)
        {
            if (!UpgradeTarget.turret.CheckIfUpgradePossible(i))
            {
                upgradeBtnCosts[i].text = "MAX";
                upgradeBtnCosts[i].color = buttonMax;
            }
            else
            {
                int value = costs[i];
                upgradeBtnCosts[i].text = GameManager.Instance.FormatNumber(costs[i]);

                if (value <= gameManager.CashAmount)
                    upgradeBtnCosts[i].color = buttonAvailable;
                else
                    upgradeBtnCosts[i].color = buttonNotAvailable;
            }


            if (i == 0)
                upgradeBtnValues[i].text = $"{GameManager.Instance.FormatNumber((long)(UpgradeTarget.turret.Health * 10f))}/{GameManager.Instance.FormatNumber((long)(values[i] * 10f))}";

            else if (i == 1)
                upgradeBtnValues[i].text = GameManager.Instance.FormatNumber((long)(values[i] * 600f)) + "/min";

            else if (i == 3)
                upgradeBtnValues[i].text = $"{GameManager.Instance.FormatNumber((long)(60f / (1f / values[i])))}/min";

            else if (i == 4)
                upgradeBtnValues[i].text = $"{GameManager.Instance.FormatNumber((long)values[i])}m";

            else if (i == 5)
                upgradeBtnValues[i].text = $"{GameManager.Instance.FormatNumber((long)values[i])}%";

            else if (i == 6)
                upgradeBtnValues[i].text = $"{GameManager.Instance.FormatNumber(100 + (long)values[i])}%";

            else
                upgradeBtnValues[i].text = GameManager.Instance.FormatNumber((long)(values[i] * 10f));


        }
    }


    void SetUpgradeBtn()
    {
        if (UpgradeTarget.PossibleUpgrade == null)
        {
            upgradeBtn.SetActive(false);
            return;
        }

        if (!UpgradeTarget.turret.CanBeUpgradedToNext())
        {
            upgradeBtn.SetActive(false);
            return;
        }

        SetUpgradeBtnActive();
    }

    void SetUpgradeBtnActive()
    {
        if (UpgradeTarget.PossibleUpgrade == null || !UpgradeTarget.PossibleUpgrade.IsUnlocked)
        {
            if (UpgradeTarget.PossibleUpgrade2 == null)
            {
                upgradeBtn.SetActive(false);
                return;
            }


            if (UpgradeTarget.PossibleUpgrade2.IsUnlocked)
            {
                upgradeBtn.SetActive(true);
                possibleUpgrade.text = UpgradeTarget.PossibleUpgrade2.UnlockName;
                upgradeBtn.GetComponent<Button>().onClick.AddListener(UpgradeToSecondNextTurret);
                upgradeBtn2.SetActive(false);
                return;
            }

            upgradeBtn.SetActive(false);
            return;
        }


        upgradeBtn.SetActive(true);
        possibleUpgrade.text = UpgradeTarget.PossibleUpgrade.UnlockName;
        upgradeBtn.GetComponent<Button>().onClick.AddListener(UpgradeToNextTurret);
        upgradeBtn2.SetActive(false);
        if (UpgradeTarget.PossibleUpgrade2 == null)
            return;

        if (UpgradeTarget.PossibleUpgrade2.IsUnlocked)
        {
            upgradeBtn2.SetActive(true);
            possibleUpgrade2.text = UpgradeTarget.PossibleUpgrade2.UnlockName;
            upgradeBtn2.GetComponent<Button>().onClick.AddListener(UpgradeToSecondNextTurret);
        }

    }



    public void UpdateUpgradeBtn()
    {
        if (UpgradeTarget.PossibleUpgrade == null)
            return;

        if (!upgradeBtn.activeSelf && UpgradeTarget.turret.CanBeUpgradedToNext())
            SetUpgradeBtnActive();

        if (!upgradeBtn.activeSelf)
            return;


        upgradeValue.text = gameManager.FormatNumber(UpgradeTarget.PossibleUpgrade.UpgradeCostInGame).ToString();

        if (UpgradeTarget.PossibleUpgrade.UpgradeCostInGame > gameManager.CrystalAmount)
        {
            upgradeBtn.GetComponent<Button>().interactable = false;
            upgradeValue.color = buttonNotAvailable;
        }
        else
        {
            upgradeBtn.GetComponent<Button>().interactable = true;
            upgradeValue.color = buttonAvailable;
        }

        if (!upgradeBtn2.activeSelf)
            return;

        upgradeValue2.text = UpgradeTarget.PossibleUpgrade2.UpgradeCostInGame.ToString();

        if (UpgradeTarget.PossibleUpgrade2.UpgradeCostInGame > gameManager.CrystalAmount)
        {
            upgradeBtn2.GetComponent<Button>().interactable = false;
            upgradeValue2.color = buttonNotAvailable;
        }
        else
        {
            upgradeBtn2.GetComponent<Button>().interactable = true;
            upgradeValue2.color = buttonAvailable;
        }

    }


    void UpgradeToNextTurret()
    {
        if (UpgradeTarget.UpgradeTurret())
            upgradePanel.SetActive(false);
    }

    void UpgradeToSecondNextTurret()
    {
        if (UpgradeTarget.SecondUpgradeTurret())
            upgradePanel.SetActive(false);
    }




    // button actions
    public void StartGame()
    {
        if (gameManager.Turrets.Count < 1)
            return;

        Time.timeScale = 1f;
        GameManager.Instance.SpawningManager.StartWave();
        StartBtn.SetActive(false);
        
    }

    #region pickups
    int generatedCrystal, generatedCash, generatedBoost, generatedStrength, generatedDuration;

    public bool ActivateBoost(int id, int str, int dur)
    {
        if(UpgradeTarget.turret.boostActivated)
            return false;

        UpgradeTarget.turret.BoostEffect(id, str, dur);
        return true;
    }
   


    [SerializeField] Sprite defaultIcon;
    [SerializeField] Image iconSlot;
    [SerializeField] TMP_Text defaultName;
    [Header("PopUpOptions")]
    [SerializeField] TMP_Text cashRewardValue, crystalRewardValue, strengthValue, durationValue;
    [SerializeField] GameObject cashPopUp, boostPopUp;
    public GameObject BoostPopUP => boostPopUp;
    [SerializeField] List<Boost> boostIcons = new List<Boost>();
    public List<Boost> BoostIcons => boostIcons;
    public void GeneratePopUpValue(bool isBoost)
    {
        popUp.SetActive(true);
        Time.timeScale = 0f;
        cashPopUp.SetActive(false);
        boostPopUp.SetActive(false);
        iconSlot.sprite = defaultIcon;
        defaultName.text = "You received:";
        ClosePanel();

        if (isBoost)
        {
            boostPopUp.SetActive(true);
            generatedBoost = (int)UnityEngine.Random.Range(0, boostIcons.Count);
            defaultName.text = boostIcons[generatedBoost].Name;
            iconSlot.sprite = boostIcons[generatedBoost].Icon;
            generatedStrength = (int)UnityEngine.Random.Range(10, 90);
            generatedDuration = (int)UnityEngine.Random.Range(4, 10);
            strengthValue.text = $"Boost strength: +{generatedStrength}%";
            durationValue.text = $"Boost strength: {generatedDuration}s";
            return;
        }

        cashPopUp.SetActive(true);
        float minValue = (3000f * gameManager.Difficulty * gameManager.Stage);
        generatedCash = (int)UnityEngine.Random.Range(minValue, minValue * 2f);

        minValue = (20 * gameManager.Difficulty * gameManager.Stage);
        generatedCrystal = (int)UnityEngine.Random.Range(minValue, minValue * 2f);

        popUp.SetActive(true);
        cashRewardValue.text = generatedCash.ToString();
        crystalRewardValue.text = generatedCrystal.ToString();
    }

    public void GetNormalBoost()
    {
        popUp.SetActive(false);
        Time.timeScale = 1f;
        SpawnSkillPrefab();
    }


    [Header("SkillList")]
    [SerializeField] GameObject skillList;
    [SerializeField] GameObject boostPrefab;

    public void SpawnSkillPrefab()
    {
        if (skillList.transform.childCount >= 5)
        {
            stageWave.text = "Too many boosts";
            return;
        }
        GameObject x = Instantiate(boostPrefab, Vector3.zero, Quaternion.identity);
        x.GetComponent<BoostBehaviour>().SetStats(generatedBoost, generatedStrength, generatedDuration);
        x.transform.SetParent(skillList.transform);
    }

    public void GetAdBoost()
    {
        generatedStrength *= 2;
        generatedDuration *= 2;
        popUp.SetActive(false);
        Time.timeScale = 1f;
        SpawnSkillPrefab();

    }

    public void GetNormalReward()
    {
        gameManager.CashAmount += generatedCash;
        gameManager.CrystalAmount += generatedCrystal;
        popUp.SetActive(false);
        Time.timeScale = 1f;
    }

    public void GetAdReward()
    {
        gameManager.CashAmount += (generatedCash * 2);
        gameManager.CrystalAmount += (generatedCrystal * 2);
        popUp.SetActive(false);
        Time.timeScale = 1f;
    }

    #endregion


    public void BuyMaxHealth() => UpgradeTarget.turret.BuyMaxHealthBoost();
    public void BuyHealthRegen() => UpgradeTarget.turret.BuyHealthRegenBoost();
    public void BuyDamage() => UpgradeTarget.turret.BuyDamageBoost();
    public void BuyFireRate() => UpgradeTarget.turret.BuyFireRateBoost();
    public void BuyFireRange() => UpgradeTarget.turret.BuyFireRangeBoost();
    public void BuyRotationSpeed() => UpgradeTarget.turret.BuyRotationSpeedBoost();
    public void BuyCritDamage() => UpgradeTarget.turret.BuyCritDamageBoost();
    public void BuyCritRate() => UpgradeTarget.turret.BuyCritRateBoost();
    public void ClosePanel()
    {
        UpgradeTarget.ChangeActive(false);
        upgradePanel.SetActive(false);
        gameManager.CameraPoint.StartCameraMovement();

    }


}

[Serializable]
public class Boost
{
    public string Name;
    public Sprite Icon;
}