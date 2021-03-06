using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System.Threading.Tasks;

public class AdsManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener, IUnityAdsInitializationListener
{
    public readonly static string ANDROID_KEY = "4791287", IOS_KEY = "4791286";
    [SerializeField] GameObject energyButton;
    Button energyBtn;
    [SerializeField] MainManuBtnHandle btnHandle;

    void Start() => energyBtn = energyButton.GetComponent<Button>();
    void Awake()
    {
        Advertisement.Initialize(ANDROID_KEY, false, this); //initialize ads module
        Advertisement.Load("Rewarded_Android", this);
    }
    

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if(btnHandle.GS.Energy == 100)
            return;

        energyButton.SetActive(true);
        energyBtn.interactable = true;
        energyBtn.onClick.AddListener(ShowAd);

    }

    public void ShowAd()
    {
        energyBtn.interactable = false;
        energyButton.SetActive(false);
        Advertisement.Show("Rewarded_Android", this);
        SaveSys.SAVE(btnHandle.GS);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals("Rewarded_Android") && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            btnHandle.GS.AddEnergy(20);
            btnHandle.EnergyHandle();
            Advertisement.Load("Rewarded_Android", this);
        }
    }

    // Implement Load and Show Listener error callbacks:
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {

    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {

    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

    void OnDestroy()
    {
        // Clean up the button listeners:
        
    }


    public void OnInitializationComplete()
    {
        Advertisement.Load("Rewarded_Android", this);
    }


    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        
    }



}
