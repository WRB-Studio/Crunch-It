using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using System;

public class AdMob : MonoBehaviour
{
    [Header("App ID")]
    public string appIdAndroid = "ca-app-pub-2603621309259391~9030398749";

    [Header("Banner")]
    public string bannerAdUnitIdAndroid = "ca-app-pub-2603621309259391/5777888750";
    private BannerView bannerView;

    [Header("Rewarded ad")]
    public string rewardedAdUnitId = "ca-app-pub-2603621309259391/9487340536";
    private RewardedAd rewardedAd;
    private bool rewardEarned = false;
    private int maxLoadTries = 10;
    private bool rewardAdIsLoaded = false;

    private string testBannerAdUnitIdAndroid = "ca-app-pub-3940256099942544/6300978111";
    private string testRewardedAdAdUnitIdAndroid = "ca-app-pub-3940256099942544/5224354917";


    private void Start()
    {
        Debug.LogWarning("AdMob is disabled!");
        return;

        if (Application.isEditor)
            return;

        string tmpAppId;

        if (Application.platform == RuntimePlatform.Android)
            tmpAppId = appIdAndroid;
        else
            tmpAppId = "unexpected_platform";

        MobileAds.Initialize(initStatus => { });

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(tmpAppId);

        requestBanner();

        createRewardedAd();
    }


    private void requestBanner()
    {
        string tmpAdUnitId;

        if (Application.isEditor)
        {
            tmpAdUnitId = "unused";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            tmpAdUnitId = bannerAdUnitIdAndroid;
        }
        else
        {
            tmpAdUnitId = "unexpected_platform";
        }

        // Create a 320x50 banner at the bottom of the screen.
        this.bannerView = new BannerView(tmpAdUnitId, AdSize.Banner, AdPosition.Bottom);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        this.bannerView.LoadAd(request);
    }

    public void createRewardedAd()
    {
        if (maxLoadTries <= 0)
            return;

        string tmpAdUnitId;

        if (Application.isEditor)
            tmpAdUnitId = "unused";
        else if (Application.platform == RuntimePlatform.Android)
            tmpAdUnitId = rewardedAdUnitId;
        else
            tmpAdUnitId = "unexpected_platform";

        this.rewardedAd = new RewardedAd(tmpAdUnitId);

        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);

        maxLoadTries--;
    }



    public void userChoseToWatchAd()
    {
        if (this.rewardedAd.IsLoaded())
        {
            this.rewardedAd.Show();
        }
        else
        {
            afterReward();
        }
    }

    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdLoaded event received");
        rewardAdIsLoaded = true;
    }

    public void HandleRewardedAdFailedToLoad(object sender, AdErrorEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToLoad event received with message: "
                             + args.Message);
        rewardAdIsLoaded = false;
        createRewardedAd();
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdOpening event received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.Message);

        afterReward();
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        rewardEarned = true;

        string type = args.Type;
        double amount = args.Amount;
        MonoBehaviour.print(
            "HandleRewardedAdRewarded event received for "
                        + amount.ToString() + " " + type);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        this.createRewardedAd();

        afterReward();
    }

    public bool getRewardAdIsLoaded()
    {
        return rewardAdIsLoaded;
    }

    private void afterReward()
    {
        GameHandler.continueAfterRewardAd(rewardEarned);
    }

}
