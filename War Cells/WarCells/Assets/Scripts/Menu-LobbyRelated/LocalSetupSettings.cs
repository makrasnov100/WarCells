using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

using TMPro;
using System;

public class LocalSetupSettings : MonoBehaviour
{
    //Player Setup
    public GameObject humanPlayers;
    public GameObject easyBots;
    public GameObject mediumBots;
    public GameObject hardBots;
    //Map Setup  
    public GameObject inMapSize;
    public GameObject inCellSize;
    public GameObject inMapType;
    //Experimental Map Options
    public GameObject cellPad;
    public GameObject symmetry;
    public GameObject maxConnectDist;

    //UI transition
    public GameObject mapLoading;
    public Canvas fullUI;

    //Create Game Ad
    //private InterstitialAd interstitial;
    private RewardBasedVideoAd rewardBasedVideo;
    private bool isEditor;
    private bool isLoaded = false;

    public void Start()
    {
        //RequestInterstitial();
        // Get singleton reward based video ad reference.
        this.rewardBasedVideo = RewardBasedVideoAd.Instance;

        // Called when the user should be rewarded for watching a video.
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
        // Called when the ad is closed.
        rewardBasedVideo.OnAdClosed += HandleRewardBasedVideoClosed;
        // Called when an ad request has successfully loaded.
        rewardBasedVideo.OnAdLoaded += HandleRewardBasedVideoLoaded;
        // Called when an ad request failed to load.
        rewardBasedVideo.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;

        this.RequestRewardBasedVideo();
    }

    public void OnPlayBtn()
    {
        //Pause the game music
        OptionsManager.Instance.gameMusic.Pause();

        //play the add if loaded
        if (!isEditor && isLoaded && rewardBasedVideo.IsLoaded())
        {
            mapLoading.SetActive(true);
            rewardBasedVideo.Show();
        }
        else
        {
            PlayGame();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            //Build references
            PlayerManager pm = GameObject.FindGameObjectWithTag("playerManager").GetComponent<PlayerManager>();
            GameObject mgNorm = GameObject.FindGameObjectWithTag("mapGenerator");
            GameObject mgExp = GameObject.FindGameObjectWithTag("mapGeneratorExp");

            //Setup of playuer manager
            if (pm != null)
            {
                pm.SetHumanPlayers(int.Parse(humanPlayers.GetComponent<TMP_InputField>().text));
                pm.SetBotPlayers(int.Parse(easyBots.GetComponent<TMP_InputField>().text),
                                 int.Parse(mediumBots.GetComponent<TMP_InputField>().text),
                                 int.Parse(hardBots.GetComponent<TMP_InputField>().text));
            }
                
            //Add references too dropdowns
            TMP_Dropdown mapTypeOption = inMapType.GetComponent<TMP_Dropdown>();
            TMP_Dropdown mapSizeOption = inMapSize.GetComponent<TMP_Dropdown>();
            TMP_Dropdown cellSizeOption = inCellSize.GetComponent<TMP_Dropdown>();
            TMP_Dropdown symmetryOption = symmetry.GetComponent<TMP_Dropdown>();

            //Find which map type is selected
            if (mapTypeOption.options[mapTypeOption.value].text == "Hex")
            {
                mgNorm.SetActive(true);
                mgExp.SetActive(false);
                MapGenerator mg = mgNorm.GetComponent<MapGenerator>();
                int newMapSize = 0;
                // Setup the map size
                switch (mapSizeOption.options[mapSizeOption.value].text)
                {
                    case "Small":
                        newMapSize = 1;
                        break;
                    case "Medium":
                        newMapSize = 3;
                        break;
                    case "Large":
                        newMapSize = 5;
                        break;
                    case "HUUGE":
                        newMapSize = 10;
                        break;
                }
                mg.SetMapSize(newMapSize);

                // Setup Cell Size
                int newMinCellSize = 0;
                int newMaxCellSize = 0;
                switch (cellSizeOption.options[cellSizeOption.value].text)
                {
                    case "Normal":
                        newMinCellSize = 4;
                        newMaxCellSize = 12;
                        break;
                    case "Small":
                        newMinCellSize = 2;
                        newMaxCellSize = 6;
                        break;
                    case "Large":
                        newMinCellSize = 8;
                        newMaxCellSize = 20;
                        break;
                    case "Huge":
                        newMinCellSize = 14;
                        newMaxCellSize = 30;
                        break;
                }
                mg.SetMinCellSize(newMinCellSize);
                mg.SetMaxCellSize(newMaxCellSize);

            }
            else
            {
                mgNorm.SetActive(false);
                mgExp.SetActive(true);
                MapGenExperimental mg = mgExp.GetComponent<MapGenExperimental>();

                //Set Cell Padding
                mg.cellPadding = float.Parse(cellPad.GetComponent<TMP_InputField>().text);
                mg.maxConnectionDistance = float.Parse(maxConnectDist.GetComponent<TMP_InputField>().text);

                // Connection Prevalence
                mg.connectionPrevalence = 1;

                //Set Map Size 
                switch (mapSizeOption.options[mapSizeOption.value].text)
                {
                    case "Small":
                        mg.mapSizeVertical = 6;
                        mg.mapSizeHorizontal = 6;
                        break;
                    case "Medium":
                        mg.mapSizeVertical = 10;
                        mg.mapSizeHorizontal = 10;
                        break;
                    case "Large":
                        mg.mapSizeVertical = 16;
                        mg.mapSizeHorizontal = 16;
                        break;
                    case "HUUGE":
                        mg.mapSizeVertical = 24;
                        mg.mapSizeHorizontal = 24;
                        break;
                }

                //Set Cell Capacities
                switch (cellSizeOption.options[cellSizeOption.value].text)
                {
                    case "Normal":
                        mg.minCellSize = 4;
                        mg.minCellSize = 12;
                        break;
                    case "Small":
                        mg.minCellSize = 2;
                        mg.minCellSize = 6;
                        break;
                    case "Large":
                        mg.minCellSize = 8;
                        mg.minCellSize = 20;
                        break;
                    case "Huge":
                        mg.minCellSize = 14;
                        mg.minCellSize = 30;
                        break;
                }

                //Set Cell Symmetry
                switch (symmetryOption.options[symmetryOption.value].text)
                {
                    case "None":
                        mg.horizontalSym = false;
                        mg.verticalSym = false;
                        break;
                    case "Vertical":
                        mg.horizontalSym = false;
                        mg.verticalSym = true;
                        break;
                    case "Horizontal":
                        mg.horizontalSym = true;
                        mg.verticalSym = false;
                        break;
                    case "Full":
                        mg.horizontalSym = true;
                        mg.verticalSym = true;
                        break;
                }

                //Switch Grid Offset
                switch (mapTypeOption.options[mapTypeOption.value].text)
                {
                    case "Skew":
                        mg.diagonalLayout = true;
                        mg.randomizerLayout = false;
                        break;
                    case "Clustered":
                        mg.diagonalLayout = false;
                        mg.randomizerLayout = true;
                        break;
                    case "Grid":
                        mg.diagonalLayout = false;
                        mg.randomizerLayout = false;
                        break;
                }
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            //#if !UNITY_EDITOR
            //    interstitial.Destroy();
            //#endif
            Destroy(gameObject);
        }
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    //    private void RequestInterstitial()
    //    {
    //#if UNITY_ANDROID && !UNITY_EDITOR
    //        string adUnitId = OptionsManager.Instance.newGameAd;
    //#elif UNITY_EDITOR
    //        isEditor = true;
    //        string adUnitId = "";
    //        return;
    //#else
    //        string adUnitId = "unexpected_platform";
    //#endif

    //        // Initialize an InterstitialAd.
    //        this.interstitial = new InterstitialAd(adUnitId);

    //        // Link add close funtion
    //        this.interstitial.OnAdClosed += PlayGame;

    //        // Create an empty ad request.
    //        AdRequest request = new AdRequest.Builder().Build();
    //        // Load the interstitial with the request.
    //        this.interstitial.LoadAd(request);
    //    }

    private void RequestRewardBasedVideo()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            string adUnitId = OptionsManager.Instance.newGameAd;
        #elif UNITY_EDITOR
            string adUnitId = "unexpected_platform";
            isEditor = true;
            return;
        #else
            string adUnitId = "unexpected_platform";
        #endif
        isEditor = false;
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        this.rewardBasedVideo.LoadAd(request, adUnitId);
    }

    //Called when user finishied watching ad
    public void PlayGame()
    {
        //Resume music
        OptionsManager.Instance.gameMusic.UnPause();

        //Go to game Scene
        ContinueToGame();
    }

    public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
    {
        isLoaded = false;
        PlayGame();
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        isLoaded = false;
        PlayGame();
    }
    public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
    {
        isLoaded = true;
    }

    public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        isLoaded = false;
    }

    public void ContinueToGame()
    {
        fullUI.enabled = false;
        GameObject.DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("GameScene");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
}

