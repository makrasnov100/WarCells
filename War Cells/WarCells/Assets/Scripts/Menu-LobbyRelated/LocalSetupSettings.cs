using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;

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


    public void OnPlayBtn()
    {
        Object.DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("GameScene");
        SceneManager.sceneLoaded += OnSceneLoaded;
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
            Destroy(gameObject);
        }
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

