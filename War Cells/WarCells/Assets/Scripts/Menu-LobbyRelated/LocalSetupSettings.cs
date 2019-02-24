using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using TMPro;

public class LocalSetupSettings : MonoBehaviour
{
    //Player Setup
    public int humanPlayers;
    public int easyBots;
    public int mediumBots;
    public int hardBots;
    //Map Setup  
    public TMP_Dropdown inMapSize;

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
            // - player manager
            GameObject pm = GameObject.FindGameObjectWithTag("playerManager");
            // - correct map manager

            GameObject mg = null;
            GameObject mgNorm = GameObject.FindGameObjectWithTag("mapGenerator");
            GameObject mgExp = GameObject.FindGameObjectWithTag("mapGeneratorExp");

            if (mgNorm != null)
                mg = mgNorm;
            else if (mgExp != null)
                mg = mgExp;

            if (pm != null)
                pm.GetComponent<PlayerManager>().SetHumanPlayers(int.Parse(inPlayers.text));
            if (mg != null)
            {
                int tempMapSizeVar = 4 * (1 +  inMapSize.value);
                mg.GetComponent<MapGenerator>().SetMapSize(tempMapSizeVar);
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }

    //Accessors/Mutators
    void humanPlayers()
    {

    }

    void 

}

