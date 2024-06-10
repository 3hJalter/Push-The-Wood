using _Game.Managers;
using _Game.Utilities;
using _Game.Utilities.Timer;
using GoogleMobileAds.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MaxSdkCallbacks;

public class BannerAds : MonoBehaviour
{
    public enum TYPE
    {
        MAX = 0,
        ADSMOB = 1,
    }
    string collapsibleBannerAdUnitId = "ca-app-pub-9819920607806935/7099802728";
    bool isBannerOpen = false;
    private bool isAdmobInited = false;
    // Start is called before the first frame update
    BannerView bannerView;
    bool isBannerPrepared = false;
    TYPE currentBannerType;
    public bool IsBannerOpen => isBannerOpen;
    void Start()
    {
        MobileAds.Initialize(initStatus =>
        {
            DevLog.Log(DevId.Hung, initStatus.ToString());
            isAdmobInited = true;
            InitBanner();
        });
    }

    public void Show(TYPE type)
    {
        isBannerOpen = true;
        currentBannerType = type;
        switch (type)
        {
            case TYPE.ADSMOB:
                Load();
                break;
        }
        
    }

    public void Hide()
    {
        if (isBannerOpen)
        {
            isBannerOpen = false;
            if (bannerView != null)
            {
                DevLog.Log(DevId.Hung, "Banner Hide");
                bannerView.Hide();
            }      
        }
    }

    public void InitBanner()
    {
        bannerView = new BannerView(collapsibleBannerAdUnitId, AdSize.Banner, AdPosition.Bottom);
        isBannerPrepared = true;

        bannerView.OnBannerAdLoaded += () =>
        {
            DevLog.Log(DevId.Hung, "Banner view loaded an ad with response : "
                + bannerView.GetResponseInfo());
        };
        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Load();
            DevLog.Log(DevId.Hung, "Banner view failed to load an ad with error : "
                + error);
        };
        bannerView.OnAdPaid += (AdValue adValue) =>
        {
            DevLog.Log(DevId.Hung, String.Format("Banner view paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
        };
        bannerView.OnAdImpressionRecorded += () =>
        {
            DevLog.Log(DevId.Hung, "Banner view recorded an impression.");
        };
        bannerView.OnAdClicked += () =>
        {
            DevLog.Log(DevId.Hung, "Banner view was clicked.");
        };
        bannerView.OnAdFullScreenContentOpened += () =>
        {
            DevLog.Log(DevId.Hung, "Banner view full screen content opened.");
        };
        bannerView.OnAdFullScreenContentClosed += () =>
        {
            DevLog.Log(DevId.Hung, "Banner view full screen content closed.");
        };
    }
    public void Load()
    {
        DevLog.Log(DevId.Hung, "Show banner collap");
        if (isAdmobInited)
        {            
            if (!isBannerPrepared)
            {
                if (bannerView != null)
                    DestroyBannerView();
                InitBanner();
            }

            var adRequest = new AdRequest.Builder().Build();
            // Create an extra parameter that aligns the bottom of the expanded ad to the
            // bottom of the bannerView.
            adRequest.Extras.Add("collapsible", "bottom");
            adRequest.Extras.Add("collapsible_request_id", RandomIDForBannerCollap());
            bannerView.LoadAd(adRequest);
            isBannerPrepared = false;
        }
    }
    private string RandomIDForBannerCollap()
    {
        int count = 5;
        string result = "";
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                int rdValue = UnityEngine.Random.Range(0, 10);
                result += rdValue.ToString();
            }
            if (i < 4)
                result += "-";
        }
        return result;
    }
    private void DestroyBannerView()
    {
        if (bannerView != null)
        {
            DevLog.Log(DevId.Hung, "Destroying banner view.");
            bannerView.Destroy();
            bannerView = null;
        }
    }
}
