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

            //Find which map type is selected
            if (mapTypeOption.options[mapTypeOption.value].text == "Experimental")
            {
                mgExp.SetActive(true);
            }
            else if(mapTypeOption.options[mapTypeOption.value].text == "Hex")
            {
                mgNorm.SetActive(true);
                MapGenerator mg = mgNorm.GetComponent<MapGenerator>();
                int newMapSize = 0;
                //Setup the map size
                switch(mapSizeOption.options[mapSizeOption.value].text)
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

                //Setup Cell Size
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

            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }
}

