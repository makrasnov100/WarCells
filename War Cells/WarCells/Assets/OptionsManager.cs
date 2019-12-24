using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class OptionsManager : MonoBehaviour
{

    public static OptionsManager Instance;

    //UI Variables
    public GameObject fullUI;
    public GameObject inGameParts;
    public GameObject darkBG;
    //Input Variables
    public Slider curVolumeInput;
    public Button showCellTextYes;
    public Button showCellTextNo;
    public Button mainMenuBtn;
    public Button closeOptionsMenuBtn;
    //Control Variables
    public bool isShowCellText = true;
    public AudioSource gameMusic;

    //Ad Variables
    // test intermidiate ad = "ca-app-pub-3940256099942544/5224354917";
    public string newGameAd = "ca-app-pub-3940256099942544/5224354917";


    // Start is called before the first frame update
    void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(this.gameObject);
    }

    private void Start()
    {
        MobileAds.Initialize(initStatus => { });
        UpdateCellTextValue(isShowCellText);
    }

    //Options and controls
    public void UpdateGameVolume()
    {
        gameMusic.volume = curVolumeInput.value;
    }

    public void UpdateCellTextValue(bool isEnabled)
    {
        isShowCellText = isEnabled;
        if (isShowCellText)
        {
            showCellTextYes.interactable = false;
            showCellTextNo.interactable = true;
        }
        else
        {
            showCellTextYes.interactable = true;
            showCellTextNo.interactable = false;
        }

        //Cell actual cells if in game
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            GameObject[] mapGens = GameObject.FindGameObjectsWithTag("mapGenerator");
            foreach (GameObject generator in mapGens)
            {
                MapGenerator curMapGen = generator.GetComponent<MapGenerator>();
                if (curMapGen)
                {
                    foreach (GameObject cell in curMapGen.newCells)
                    {
                        cell.GetComponent<CellIdentity>().UpdateCellLabel();
                    }
                    continue;
                }

                MapGenExperimental curMapGenExp = generator.GetComponent<MapGenExperimental>();
                if (curMapGenExp)
                {
                    foreach (List<GameObject> cellList in curMapGenExp.cells)
                    {
                        foreach (GameObject cell in cellList)
                        {
                            cell.GetComponent<CellIdentity>().UpdateCellLabel();
                        }
                    }
                }
            }

        }
    }

    //Show Options Menu
    public void ShowMenu()
    {
        if (!fullUI.activeInHierarchy)
            fullUI.SetActive(true);
        else
        {
            HideMenu();
            return;
        }

        curVolumeInput.value = gameMusic.volume;
        UpdateCellTextValue(isShowCellText);

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            inGameParts.SetActive(true);
        }
        else
        {
            inGameParts.SetActive(false);
        }
    }

    public void HideMenu()
    {
        fullUI.SetActive(false);
    }

    public void MainMenu()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            HideMenu();
            GameObject.Find("OptionsPlexus").GetComponent<Animator>().SetBool("isHowered", false);
            GameObject.Find("OptionsPlexus").GetComponent<Animator>().SetBool("isClicked", false);
            GameObject mainUICont = GameObject.FindGameObjectWithTag("menuUiCont");
            if (mainUICont)
            {
                MenuAnimControl uiControlComp = mainUICont.GetComponent<MenuAnimControl>();
                if (uiControlComp)
                {
                    uiControlComp.menuLast = "NA";
                }
            }
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
            HideMenu();
        }
    }

}
