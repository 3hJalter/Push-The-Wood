using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppOpenAds : MonoBehaviour
{
    string adUnitId = "ca-app-pub-9819920607806935/9036921148";

    private void Start()
    {
        MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;    
    }

    public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Load();
    }

    public void Load()
    {
        MaxSdk.LoadAppOpenAd(adUnitId);
    }

    public void Show()
    {
        if (MaxSdk.IsAppOpenAdReady(adUnitId))
        {
            MaxSdk.ShowAppOpenAd(adUnitId);
        }
        else
        {
            Load();
        }
    }
}
